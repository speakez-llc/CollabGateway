module CollabGateway.Server.Database

open System
open System.IO
open System.Net.Http
open FSharp.Data
open Marten
open CollabGateway.Shared.API
open CollabGateway.Shared.Events
open Newtonsoft.Json
open Weasel.Core
open JasperFx.CodeGeneration
open Npgsql

module DatabaseHelpers =
    let execNonQueryAsync connStr commandStr =
        task {
            use conn = new NpgsqlConnection(connStr)
            use cmd = new NpgsqlCommand(commandStr, conn)
            do! conn.OpenAsync()
            do! cmd.ExecuteNonQueryAsync() |> Task.map (fun _ -> ())
        }

    let connStr = Environment.GetEnvironmentVariable("GATEWAY_STORE")

    let parseDatabase (connectionString: string) =
        let parts = connectionString.Split(';')
        let databasePart = parts |> Array.find (_.StartsWith("Database="))
        databasePart.Split('=') |> Array.last

    let getEmailTableRowCountAsync connStr databaseName =
        task {
            let commandStr = $"SELECT COUNT(*) FROM \"%s{databaseName}\".public.\"FreeEmailProviders\";"
            use conn = new NpgsqlConnection(connStr)
            use cmd = new NpgsqlCommand(commandStr, conn)
            do! conn.OpenAsync()
            let! result = cmd.ExecuteScalarAsync() |> Async.AwaitTask
            return result :?> int64
        }

    let getGicsTableRowCountAsync connStr databaseName =
        task {
            let commandStr = $"SELECT COUNT(*) FROM \"%s{databaseName}\".public.\"GicsTaxonomy\";"
            use conn = new NpgsqlConnection(connStr)
            use cmd = new NpgsqlCommand(commandStr, conn)
            do! conn.OpenAsync()
            let! result = cmd.ExecuteScalarAsync() |> Async.AwaitTask
            return result :?> int64
        }

    let createFreeEmailDomainTableAsync databaseName =
        let commandStr = $"CREATE TABLE IF NOT EXISTS \"%s{databaseName}\".public.\"FreeEmailProviders\" (\"Id\" UUID PRIMARY KEY, \"Domain\" TEXT UNIQUE);"
        execNonQueryAsync connStr commandStr

    createFreeEmailDomainTableAsync connStr |> ignore

    let createGicsTaxonomyTable databaseName =
        let commandStr = $"""
            CREATE TABLE IF NOT EXISTS "%s{databaseName}".public."GicsTaxonomy" (
                "Id" UUID PRIMARY KEY,
                "SubIndustryCode" TEXT UNIQUE,
                "SubIndustry" TEXT,
                "Definition" TEXT,
                "IndustryCode" TEXT,
                "Industry" TEXT,
                "IndustryGroupCode" TEXT,
                "IndustryGroup" TEXT,
                "SectorCode" TEXT,
                "Sector" TEXT
            );
        """
        execNonQueryAsync connStr commandStr

    type GicsCsv = CsvProvider<"GICS.csv", HasHeaders=true, Schema="SubIndustryCode (string), SubIndustry (string), Definition (string), IndustryCode (string), Industry (string), IndustryGroupCode (string), IndustryGroup (string), SectorCode (string), Sector (string)">

    let upsertGicsTaxonomyAsync =
        task {
            let filePath = "GICS.csv"
            let databaseName = parseDatabase connStr
            do! createGicsTaxonomyTable databaseName
            let csv = GicsCsv.Load(filePath)
            let rows = csv.Rows |> Seq.toArray
            let fileRowCount = rows.Length |> int64
            let! tableRowCount = getGicsTableRowCountAsync connStr databaseName
            if tableRowCount < fileRowCount then
                Console.WriteLine $"Upserting GicsTaxonomy table with {fileRowCount - tableRowCount} new rows."
                for row in rows do
                    let subIndustryCode = row.SubIndustryCode
                    let subIndustry = row.SubIndustry
                    let definition = row.Definition
                    let industryCode = row.IndustryCode
                    let industry = row.Industry
                    let industryGroupCode = row.IndustryGroupCode
                    let industryGroup = row.IndustryGroup
                    let sectorCode = row.SectorCode
                    let sector = row.Sector
                    let commandStr = $"""
                        INSERT INTO "%s{databaseName}".public."GicsTaxonomy"
                        ("Id", "SubIndustryCode", "SubIndustry", "Definition", "IndustryCode", "Industry", "IndustryGroupCode", "IndustryGroup", "SectorCode", "Sector")
                        VALUES ('%s{Guid.NewGuid().ToString()}', '%s{subIndustryCode}', '%s{subIndustry}', '%s{definition}', '%s{industryCode}', '%s{industry}', '%s{industryGroupCode}', '%s{industryGroup}', '%s{sectorCode}', '%s{sector}')
                        ON CONFLICT ("SubIndustryCode") DO UPDATE
                        SET "SubIndustry" = EXCLUDED."SubIndustry", "Definition" = EXCLUDED."Definition", "IndustryCode" = EXCLUDED."IndustryCode", "Industry" = EXCLUDED."Industry", "IndustryGroupCode" = EXCLUDED."IndustryGroupCode", "IndustryGroup" = EXCLUDED."IndustryGroup", "SectorCode" = EXCLUDED."SectorCode", "Sector" = EXCLUDED."Sector";
                    """
                    do! execNonQueryAsync connStr commandStr
            else
                Console.WriteLine "No new GICS Taxonomy rows to upsert."
        }

    upsertGicsTaxonomyAsync |> ignore


    let upsertFreeEmailDomainsAsync =
        task {
            let filePath = "FreeEmailDomains.txt"
            let databaseName = parseDatabase connStr
            do! createFreeEmailDomainTableAsync databaseName
            let! fileLines = File.ReadAllLinesAsync(filePath) |> Async.AwaitTask
            let domains = fileLines |> Array.filter (fun line -> not (String.IsNullOrWhiteSpace(line))) |> Set.ofArray |> Set.toArray
            let fileRowCount = domains.Length |> int64
            let! tableRowCount = getEmailTableRowCountAsync connStr databaseName
            if tableRowCount < fileRowCount then
                Console.WriteLine $"Upserting FreeEmailProviders table with {fileRowCount - tableRowCount} new rows."
                for domainName in domains do
                    let commandStr = $"INSERT INTO \"%s{databaseName}\".public.\"FreeEmailProviders\" (\"Id\", \"Domain\") VALUES ('%s{Guid.NewGuid().ToString()}', '%s{domainName}') ON CONFLICT (\"Domain\") DO NOTHING;"
                    do! execNonQueryAsync connStr commandStr
            else
                Console.WriteLine "No new Webmail Domain rows to upsert."
        }

    upsertFreeEmailDomainsAsync |> ignore

let configureMarten (options: StoreOptions) =
    let connectionString =
        match Environment.GetEnvironmentVariable("GATEWAY_STORE") with
        | null | "" -> failwith "Environment variable GATEWAY_STORE is not set."
        | connStr -> connStr
    options.Connection(connectionString)
    options.GeneratedCodeMode <- TypeLoadMode.Auto
    options.AutoCreateSchemaObjects <- AutoCreate.All
    options.Events.AddEventType(typeof<StreamEventCase>)
    options.Events.AddEventType(typeof<PageEventCase>)
    options.Events.AddEventType(typeof<ButtonEventCase>)
    options.Events.AddEventType(typeof<FormEventCase>)
    options.Events.AddEventType(typeof<ClientIPEvent>)

let store = DocumentStore.For(Action<StoreOptions>(configureMarten))

let getGeoInfo (clientIP: ClientIP) =
    task {
        let geoipifyToken = Environment.GetEnvironmentVariable("GEOIPIFY_TOKEN")
        if String.IsNullOrEmpty(geoipifyToken) then
            return failwith "GeoApify API key is not set."
        else
            let url = $"https://geo.ipify.org/api/v2/country,city?apiKey=%s{geoipifyToken}&ipAddress=%s{clientIP}"
            use httpClient = new HttpClient()
            let! response = httpClient.GetStringAsync(url)
            try
                let geoInfo = JsonConvert.DeserializeObject<GeoInfo>(response)
                return geoInfo
            with
            | ex -> return failwith $"Failed to deserialize GeoInfo: {ex.Message}"
    }
    |> Async.AwaitTask

let eventProcessor = MailboxProcessor<EventProcessingMessage>.Start(fun inbox ->
    let rec loop () = async {
        let! msg = inbox.Receive()
        match msg with
        | EstablishStreamToken (timeStamp, streamToken) ->
            use session = store.LightweightSession()
            let! streamState = session.Events.FetchStreamStateAsync(streamToken) |> Async.AwaitTask
            let event =
                if streamState = null then
                    UserStreamInitiated { TimeStamp = timeStamp; Id = Guid.NewGuid();  StreamID = streamToken }
                else
                    UserStreamResumed { TimeStamp = timeStamp; Id = Guid.NewGuid(); StreamID = streamToken }
            session.Events.Append(streamToken, [| event :> obj |]) |> ignore
            do! session.SaveChangesAsync() |> Async.AwaitTask
        | EstablishUserClientIP (timeStamp, streamToken, clientIP) ->
            use session = store.LightweightSession()
            let! allEvents = session.Events.FetchStreamAsync(streamToken) |> Async.AwaitTask
            let unwrappedEvents =
                allEvents
                |> Seq.map (_.Data )
            let sessionEvents = unwrappedEvents |> Seq.choose (function | :? ClientIPEventCase as e -> Some e | _ -> None)
            let existingEvent =
                sessionEvents
                |> Seq.tryFind (function
                    | UserClientIPDetected e when e.UserClientIP = clientIP -> true
                    | UserClientIPUpdated e when e.UserClientIP = clientIP -> true
                    | _ -> false)
            match existingEvent with
            | Some _ ->
                Console.WriteLine $"No new event needed for IP: {clientIP} as it matches an existing event."
            | None ->
                let! userGeoInfo = getGeoInfo clientIP
                let event =
                    if sessionEvents |> Seq.exists (function | UserClientIPDetected _ | UserClientIPUpdated _ -> true) then
                        UserClientIPUpdated {  TimeStamp = timeStamp; Id = Guid.NewGuid(); UserClientIP = clientIP; UserGeoInfo = userGeoInfo }
                    else
                        UserClientIPDetected { TimeStamp = timeStamp; Id = Guid.NewGuid(); UserClientIP = clientIP; UserGeoInfo = userGeoInfo }
                session.Events.Append(streamToken, [| event :> obj |]) |> ignore
                do! session.SaveChangesAsync() |> Async.AwaitTask
        | ProcessEmailStatus (timeStamp, streamToken, verificationToken, emailAddress, status) ->
            use session = store.LightweightSession()
            let event = EmailStatusAppended { TimeStamp = timeStamp; Id = Guid.NewGuid(); VerificationToken = verificationToken; EmailAddress = emailAddress; Status = status }
            session.Events.Append(streamToken, [| event :> obj |]) |> ignore
            do! session.SaveChangesAsync() |> Async.AwaitTask
        | ProcessUnsubscribeStatus(timeStamp, streamToken, subscribeToken, emailAddress, status) ->
            use session = store.LightweightSession()
            let event = SubscribeStatusAppended { TimeStamp = timeStamp; Id = Guid.NewGuid(); SubscriptionToken = subscribeToken; EmailAddress = emailAddress; Status = status }
            session.Events.Append(streamToken, [| event :> obj |]) |> ignore
            do! session.SaveChangesAsync() |> Async.AwaitTask
        | ProcessPageVisited (timeStamp, streamToken, pageName) ->
            use session = store.LightweightSession()
            let pageCase =
                match pageName with
                | DataPolicyPage -> DataPolicyPageVisited { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | HomePage -> HomePageVisited { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | ProjectPage -> ProjectPageVisited { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | CMSDataPage -> DataPageVisited { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | SignUpPage -> SignupPageVisited { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | RowerPage -> RowerPageVisited { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | SpeakEZPage -> SpeakEZPageVisited { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | ContactPage -> ContactPageVisited { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | PartnersPage -> PartnersPageVisited { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | ActivityPage -> SummaryActivityPageVisited { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | OverviewPage -> OverviewPageVisited { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | UserSummaryPage -> UserSummaryPageVisited { TimeStamp = timeStamp; Id = Guid.NewGuid() }
            session.Events.Append(streamToken, [| pageCase :> obj |]) |> ignore
            do! session.SaveChangesAsync() |> Async.AwaitTask
        | ProcessButtonClicked (timeStamp, streamToken, buttonName) ->
            use session = store.LightweightSession()
            let buttonCase =
                match buttonName with
                | HomeButton -> HomeButtonClicked { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | HomeProjectButton -> HomeProjectButtonClicked { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | HomeSignUpButton -> HomeSignUpButtonClicked { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | ProjectButton -> ProjectButtonClicked { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | ProjectDataButton -> ProjectDataButtonClicked { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | ProjectSignUpButton -> ProjectSignUpButtonClicked { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | CMSDataButton -> DataButtonClicked { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | CMSDataSignUpButton -> DataSignUpButtonClicked { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | SignUpButton -> SignUpButtonClicked { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | RowerButton -> RowerButtonClicked { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | RowerSignUpButton -> RowerSignUpButtonClicked { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | SpeakEZButton -> SpeakEZButtonClicked { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | SpeakEZSignUpButton -> SpeakEZSignUpButtonClicked { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | ContactButton -> ContactButtonClicked { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | PartnersButton -> PartnersButtonClicked { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | RowerSiteButton -> RowerSiteButtonClicked { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | CuratorSiteButton -> CuratorSiteButtonClicked { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | TableauSiteButton -> TableauSiteButtonClicked { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | PowerBISiteButton -> PowerBISiteButtonClicked { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | ThoughtSpotSiteButton -> ThoughtSpotSiteButtonClicked { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | SpeakEZSiteButton -> SpeakEZSiteButtonClicked { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | DataPolicyAcceptButton -> DataPolicyAcceptButtonClicked { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | DataPolicyDeclineButton -> DataPolicyDeclineButtonClicked { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | DataPolicyResetButton -> DataPolicyResetButtonClicked { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | ActivityButton -> SummaryActivityButtonClicked { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | OverviewButton -> OverviewButtonClicked { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | UserSummaryButton -> UserSummaryButtonClicked { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | ContactActivityButton -> ContactActivityButtonClicked { TimeStamp = timeStamp; Id = Guid.NewGuid() }
                | SignUpActivityButton -> SignUpActivityButtonClicked { TimeStamp = timeStamp; Id = Guid.NewGuid() }
            session.Events.Append(streamToken, [| buttonCase :> obj |]) |> ignore
            do! session.SaveChangesAsync() |> Async.AwaitTask
        | ProcessStreamClose (timeStamp, streamToken) ->
            use session = store.LightweightSession()
            let event = UserStreamClosed { TimeStamp = timeStamp; Id = Guid.NewGuid(); StreamID = streamToken }
            session.Events.Append(streamToken, [| event :> obj |]) |> ignore
            do! session.SaveChangesAsync() |> Async.AwaitTask
        | ProcessContactForm (timeStamp, streamToken, contactForm) ->
            use session = store.LightweightSession()
            let event = ContactFormSubmitted { TimeStamp = timeStamp; Id = Guid.NewGuid(); Form = contactForm }
            session.Events.Append(streamToken, [| event :> obj |]) |> ignore
            do! session.SaveChangesAsync() |> Async.AwaitTask
        | ProcessSignUpForm (timeStamp, streamToken, signUpForm) ->
            use session = store.LightweightSession()
            let event = SignUpFormSubmitted { TimeStamp = timeStamp; Id = Guid.NewGuid(); Form = signUpForm }
            session.Events.Append(streamToken, [| event :> obj |]) |> ignore
            do! session.SaveChangesAsync() |> Async.AwaitTask
        | ProcessSmartFormInput (timeStamp, streamToken, smartFormRawContent) ->
            use session = store.LightweightSession()
            let event = SmartFormSubmitted { TimeStamp = timeStamp; Id = Guid.NewGuid(); ClipboardInput =  smartFormRawContent }
            session.Events.Append(streamToken, [| event :> obj |]) |> ignore
            do! session.SaveChangesAsync() |> Async.AwaitTask
        | ProcessSmartFormResult (timeStamp, streamToken, form) ->
            use session = store.LightweightSession()
            let event = SmartFormResultReturned { TimeStamp = timeStamp; Id = Guid.NewGuid(); Form = form }
            session.Events.Append(streamToken, [| event :> obj |]) |> ignore
            do! session.SaveChangesAsync() |> Async.AwaitTask
        return! loop ()
    }
    loop ()
)