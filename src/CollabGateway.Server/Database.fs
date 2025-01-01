module CollabGateway.Server.Database

open System
open System.Net.Http
open System.Threading.Tasks
open Marten
open CollabGateway.Shared.API
open CollabGateway.Shared.Events
open Newtonsoft.Json
open Npgsql
open Weasel.Core
open JasperFx.CodeGeneration


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

let collectStreamsForArchive (events: seq<Marten.Events.IEvent>) =
    events
    |> Seq.groupBy (fun e -> e.StreamId)
    |> Seq.filter (fun (_, events) ->
        events
        |> Seq.forall (fun e ->
            match e.Data with
            | :? ButtonEventCase as eventCase ->
                match eventCase with
                | DataPolicyAcceptButtonClicked _ | DataPolicyDeclineButtonClicked _ -> false
                | _ -> true
            | _ -> true))
    |> Seq.map fst
    |> Set.ofSeq

let archiveEmptyStreams (): Async<unit> =
    async {
        use session = store.LightweightSession()
        let! allEvents = session.Events.QueryAllRawEvents() |> Task.FromResult |> Async.AwaitTask
        let streamsToArchive = collectStreamsForArchive allEvents
        for streamId in streamsToArchive do
            session.Events.ArchiveStream(streamId) |> ignore
        do! session.SaveChangesAsync() |> Async.AwaitTask
    }

let getGeoInfo (clientIP: ClientIP) =
    task {
        let geoipifyToken = Environment.GetEnvironmentVariable("GEOIPIFY_TOKEN")
        if String.IsNullOrEmpty(geoipifyToken) then
            return failwith "GeoApify API key is not set."
        else
            let url = $"https://geo.ipify.org/api/v2/country,city?apiKey=%s{geoipifyToken}&ipAddress=%s{clientIP}"
            use httpClient = new HttpClient()
            let! response = httpClient.GetAsync(url)
            if response.IsSuccessStatusCode then
                let! responseBody = response.Content.ReadAsStringAsync()
                try
                    let geoInfo = JsonConvert.DeserializeObject<GeoInfo>(responseBody)
                    return geoInfo
                with
                | ex -> return failwith $"Failed to deserialize GeoInfo: {ex.Message}"
            else
                let! responseBody = response.Content.ReadAsStringAsync()
                return failwith $"GeoIP API request failed: {response.StatusCode} - {responseBody}"
    }
    |> Async.AwaitTask

let getNearestIndustries (vector: float[]) : Async<GicsTaxonomy array> =
    async {
        let vectorString = $"""[{vector |> Array.map (fun x -> x.ToString("G17")) |> String.concat ","}]"""
        let connectionString =
            match Environment.GetEnvironmentVariable("GATEWAY_STORE") with
            | null | "" -> failwith "Environment variable GATEWAY_STORE is not set."
            | connStr -> connStr

        let commandStr =
            $"""
            SELECT "SectorCode", "Sector", "IndustryGroupCode", "IndustryGroup",
                   "IndustryCode", "Industry", "SubIndustryCode", "SubIndustry"
            FROM "GicsTaxonomy"
            ORDER BY "Embedding" <-> '{vectorString}'::vector
            LIMIT 3;
            """

        use conn = new NpgsqlConnection(connectionString)
        use cmd = new NpgsqlCommand(commandStr, conn)

        do! conn.OpenAsync() |> Async.AwaitTask
        let! reader = cmd.ExecuteReaderAsync() |> Async.AwaitTask
        let results = ResizeArray<GicsTaxonomy>()

        while reader.Read() do
            let gicsTaxonomy = {
                SectorCode = reader.GetString(0)
                SectorName = reader.GetString(1)
                IndustryGroupCode = reader.GetString(2)
                IndustryGroupName = reader.GetString(3)
                IndustryCode = reader.GetString(4)
                IndustryName = reader.GetString(5)
                SubIndustryCode = if reader.IsDBNull(6) then "" else reader.GetString(6)
                SubIndustryName = if reader.IsDBNull(7) then "" else reader.GetString(7)
            }
            results.Add(gicsTaxonomy)
        results |> Seq.iter (fun gicsTaxonomy ->
            Console.WriteLine $"{gicsTaxonomy.SectorName} > {gicsTaxonomy.IndustryGroupName} > {gicsTaxonomy.IndustryName} > {gicsTaxonomy.SubIndustryName}")
        return results |> Array.ofSeq
    }

let eventProcessor = MailboxProcessor<EventProcessingMessage>.Start(fun inbox ->
    let rec loop () = async {
        let! msg = inbox.Receive()
        Console.WriteLine $"Processing message: {msg}"
        try
            match msg with
            | ArchiveEmptyStreams ->
                archiveEmptyStreams() |> ignore
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
        with
        | ex -> Console.WriteLine $"Error processing message: {ex.Message}"
        return! loop ()
    }
    loop ()
)