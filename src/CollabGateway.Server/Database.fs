﻿module CollabGateway.Server.Database

open System
open System.IO
open System.Net.Http
open Marten
open CollabGateway.Shared.API
open CollabGateway.Shared.Events
open Newtonsoft.Json
open Weasel.Core
open JasperFx.CodeGeneration
open Npgsql

module DatabaseTestHelpers =
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

    let createFreeEmailDomainTableAsync databaseName =
        let commandStr = $"CREATE TABLE IF NOT EXISTS \"%s{databaseName}\".public.\"FreeEmailProviders\" (\"Id\" UUID PRIMARY KEY, \"Domain\" TEXT UNIQUE);"
        execNonQueryAsync connStr commandStr

    let createFreeEmailDomainTable databaseName =
        let commandStr = $"CREATE TABLE IF NOT EXISTS \"%s{databaseName}\".public.\"FreeEmailProviders\" (\"Id\" UUID PRIMARY KEY, \"Domain\" TEXT UNIQUE);"
        execNonQueryAsync connStr commandStr |> ignore

    let getTableRowCountAsync connStr databaseName =
        task {
            let commandStr = $"SELECT COUNT(*) FROM \"%s{databaseName}\".public.\"FreeEmailProviders\";"
            use conn = new NpgsqlConnection(connStr)
            use cmd = new NpgsqlCommand(commandStr, conn)
            do! conn.OpenAsync()
            let! result = cmd.ExecuteScalarAsync() |> Async.AwaitTask
            return result :?> int64
        }

    let upsertFreeEmailDomainsAsync =
        task {
            let filePath = "FreeEmailDomains.txt"
            let databaseName = parseDatabase connStr
            do! createFreeEmailDomainTableAsync databaseName
            let! fileLines = File.ReadAllLinesAsync(filePath) |> Async.AwaitTask
            let domains = fileLines |> Array.filter (fun line -> not (String.IsNullOrWhiteSpace(line))) |> Set.ofArray |> Set.toArray
            let fileRowCount = domains.Length |> int64
            let! tableRowCount = getTableRowCountAsync connStr databaseName
            if tableRowCount < fileRowCount then
                Console.WriteLine $"Upserting FreeEmailProviders table with {fileRowCount - tableRowCount} new rows."
                for domainName in domains do
                    let commandStr = $"INSERT INTO \"%s{databaseName}\".public.\"FreeEmailProviders\" (\"Id\", \"Domain\") VALUES ('%s{Guid.NewGuid().ToString()}', '%s{domainName}') ON CONFLICT (\"Domain\") DO NOTHING;"
                    do! execNonQueryAsync connStr commandStr
            else
                Console.WriteLine "No new rows to upsert."
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
        | EstablishStreamToken (streamToken, timeStamp) ->
            use session = store.LightweightSession()
            let! streamState = session.Events.FetchStreamStateAsync(streamToken) |> Async.AwaitTask
            let event =
                if streamState = null then
                    UserStreamInitiated { Id = Guid.NewGuid(); TimeStamp = timeStamp; StreamID = streamToken }
                else
                    UserStreamResumed { Id = Guid.NewGuid(); TimeStamp = timeStamp; StreamID = streamToken }
            session.Events.Append(streamToken, [| event :> obj |]) |> ignore
            do! session.SaveChangesAsync() |> Async.AwaitTask
        | EstablishUserClientIP (streamToken, timeStamp, clientIP) ->
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
                        UserClientIPUpdated { Id = Guid.NewGuid(); TimeStamp = timeStamp; UserClientIP = clientIP; UserGeoInfo = userGeoInfo }
                    else
                        UserClientIPDetected { Id = Guid.NewGuid(); TimeStamp = timeStamp; UserClientIP = clientIP; UserGeoInfo = userGeoInfo }
                session.Events.Append(streamToken, [| event :> obj |]) |> ignore
                do! session.SaveChangesAsync() |> Async.AwaitTask
        | ProcessEmailStatus (streamToken, timeStamp, eventToken, emailAddress, status) ->
            use session = store.LightweightSession()
            let event = EmailStatusAppended { Id = Guid.NewGuid(); TimeStamp = timeStamp; EventToken = eventToken; EmailAddress = emailAddress; Status = status }
            session.Events.Append(streamToken, [| event :> obj |]) |> ignore
            do! session.SaveChangesAsync() |> Async.AwaitTask
        | ProcessUnsubscribeStatus(streamToken, timeStamp, eventToken, emailAddress, status) ->
            use session = store.LightweightSession()
            let event = UnsubscribeStatusAppended { Id = Guid.NewGuid(); TimeStamp = timeStamp; EventToken = eventToken; EmailAddress = emailAddress; Status = status }
            session.Events.Append(streamToken, [| event :> obj |]) |> ignore
            do! session.SaveChangesAsync() |> Async.AwaitTask
        | ProcessPageVisited (streamToken, timeStamp, pageName) ->
            use session = store.LightweightSession()
            let pageCase =
                match pageName with
                | DataPolicyPage -> DataPolicyPageVisited { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | HomePage -> HomePageVisited { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | ProjectPage -> ProjectPageVisited { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | CMSDataPage -> DataPageVisited { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | SignUpPage -> SignupPageVisited { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | RowerPage -> RowerPageVisited { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | SpeakEZPage -> SpeakEZPageVisited { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | ContactPage -> ContactPageVisited { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | PartnersPage -> PartnersPageVisited { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | ActivityPage -> SummaryActivityPageVisited { Id = Guid.NewGuid(); TimeStamp = timeStamp }
            session.Events.Append(streamToken, [| pageCase :> obj |]) |> ignore
            do! session.SaveChangesAsync() |> Async.AwaitTask
        | ProcessButtonClicked (streamToken, timeStamp, buttonName) ->
            use session = store.LightweightSession()
            let buttonCase =
                match buttonName with
                | HomeButton -> HomeButtonClicked { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | HomeProjectButton -> HomeProjectButtonClicked { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | HomeSignUpButton -> HomeSignUpButtonClicked { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | ProjectButton -> ProjectButtonClicked { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | ProjectDataButton -> ProjectDataButtonClicked { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | ProjectSignUpButton -> ProjectSignUpButtonClicked { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | CMSDataButton -> DataButtonClicked { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | CMSDataSignUpButton -> DataSignUpButtonClicked { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | SignUpButton -> SignUpButtonClicked { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | RowerButton -> RowerButtonClicked { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | RowerSignUpButton -> RowerSignUpButtonClicked { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | SpeakEZButton -> SpeakEZButtonClicked { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | SpeakEZSignUpButton -> SpeakEZSignUpButtonClicked { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | ContactButton -> ContactButtonClicked { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | PartnersButton -> PartnersButtonClicked { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | RowerSiteButton -> RowerSiteButtonClicked { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | CuratorSiteButton -> CuratorSiteButtonClicked { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | TableauSiteButton -> TableauSiteButtonClicked { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | PowerBISiteButton -> PowerBISiteButtonClicked { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | ThoughtSpotSiteButton -> ThoughtSpotSiteButtonClicked { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | SpeakEZSiteButton -> SpeakEZSiteButtonClicked { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | DataPolicyAcceptButton -> DataPolicyAcceptButtonClicked { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | DataPolicyDeclineButton -> DataPolicyDeclineButtonClicked { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | DataPolicyResetButton -> DataPolicyResetButtonClicked { Id = Guid.NewGuid(); TimeStamp = timeStamp }
                | ActivityButton -> SummaryActivityButtonClicked { Id = Guid.NewGuid(); TimeStamp = timeStamp }
            session.Events.Append(streamToken, [| buttonCase :> obj |]) |> ignore
            do! session.SaveChangesAsync() |> Async.AwaitTask
        | ProcessStreamClose (streamToken, timeStamp) ->
            use session = store.LightweightSession()
            let event = UserStreamClosed { Id = Guid.NewGuid(); TimeStamp = timeStamp; StreamID = streamToken }
            session.Events.Append(streamToken, [| event :> obj |]) |> ignore
            do! session.SaveChangesAsync() |> Async.AwaitTask
        | ProcessContactForm (streamToken, timeStamp, contactForm) ->
            use session = store.LightweightSession()
            let event = ContactFormSubmitted { Id = Guid.NewGuid(); TimeStamp = timeStamp; Form = contactForm }
            session.Events.Append(streamToken, [| event :> obj |]) |> ignore
            do! session.SaveChangesAsync() |> Async.AwaitTask
        | ProcessSignUpForm (streamToken, timeStamp, signUpForm) ->
            use session = store.LightweightSession()
            let event = SignUpFormSubmitted { Id = Guid.NewGuid(); TimeStamp = timeStamp; Form = signUpForm }
            session.Events.Append(streamToken, [| event :> obj |]) |> ignore
            do! session.SaveChangesAsync() |> Async.AwaitTask
        | ProcessSmartFormInput (streamToken, timeStamp, smartFormRawContent) ->
            use session = store.LightweightSession()
            let event = SmartFormSubmitted { Id = Guid.NewGuid(); TimeStamp = timeStamp; ClipboardInput =  smartFormRawContent }
            session.Events.Append(streamToken, [| event :> obj |]) |> ignore
            do! session.SaveChangesAsync() |> Async.AwaitTask
        | ProcessSmartFormResult (streamToken, timeStamp, form) ->
            use session = store.LightweightSession()
            let event = SmartFormResultReturned { Id = Guid.NewGuid(); TimeStamp = timeStamp; Form = form }
            session.Events.Append(streamToken, [| event :> obj |]) |> ignore
            do! session.SaveChangesAsync() |> Async.AwaitTask
        return! loop ()
    }
    loop ()
)