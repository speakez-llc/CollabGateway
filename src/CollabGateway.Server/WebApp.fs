module CollabGateway.Server.WebApp

open System
open System.Net
open System.Text
open System.Net.Http
open Giraffe
open Giraffe.GoodRead
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Microsoft.Extensions.Logging
open CollabGateway.Shared.API
open CollabGateway.Shared.Events
open CollabGateway.Shared.Errors
open CollabGateway.Server.EmailHelpers
open CollabGateway.Server.Database
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open Npgsql

let private apiKey = Environment.GetEnvironmentVariable("NOTIFICATION_KEY")
let private serverName =
    let value = Environment.GetEnvironmentVariable("VITE_BACKEND_URL")
    match value with
    | null | "" -> failwith "VITE_BACKEND_URL environment variable is not set."
    | value -> value

let private ollamaEndpoint =
    let value = Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT")
    match value with
    | null | "" -> failwith "OLLAMA_ENDPOINT environment variable is not set."
    | value -> value

let getMessage (input: string): Async<string> =
    async {
        return $"Received message: {input}"
    }
let formatJson string =
    let jObject = JObject.Parse(string)
    JsonConvert.SerializeObject(jObject, Formatting.Indented)
        .Replace(" ", "&nbsp;")
        .Replace("\n", "<br>")

let validateClipboardText (text: string) =
    if String.IsNullOrWhiteSpace(text) then
        false
    else
        // Add more validation logic if needed
        true

type EmbeddingResponse = {
    embeddings: float[][]
}

type TextRequest = {
    model: string
    input: string
}

let generateVector (text: string) =
    async {
        use client = new HttpClient()
        let requestUri = $"{ollamaEndpoint}/api/embed"
        let content = new StringContent(JsonConvert.SerializeObject({ model = "granite-embedding:latest"; input = text }), Encoding.UTF8, "application/json")
        let! response = client.PostAsync(requestUri, content) |> Async.AwaitTask
        if not response.IsSuccessStatusCode then
            let! errorContent = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            Console.WriteLine $"HTTP error: {response.StatusCode} - {errorContent}"
        let! responseBody = response.Content.ReadAsStringAsync() |> Async.AwaitTask
        try
            let embeddingResponse = JsonConvert.DeserializeObject<EmbeddingResponse>(responseBody)
            if embeddingResponse.embeddings = null || embeddingResponse.embeddings.Length = 0 then
                failwith "Embedding is null or empty"
            let vectorBody = embeddingResponse.embeddings[0]
            return vectorBody
        with
        | ex ->
            Console.WriteLine $"Error deserializing response: {ex.Message}"
            return Array.empty<float>
    }

let processSemanticSearch (text: string) =
    async {
        let! textVector = generateVector text
        let! nearestIndustries = getNearestIndustries textVector
        return nearestIndustries
    }

let processSmartForm (timeStamp: EventDateTime, streamToken: StreamToken, text: SmartFormRawContent) : Async<SignUpForm> =
    async {
        try
            let endpoint = $"{ollamaEndpoint}/api/chat"
            eventProcessor.Post(ProcessSmartFormInput (timeStamp, streamToken, text))
            use httpClient = new HttpClient()
            let requestPayload = {
                model = "llama3.2:1b"
                messages = [
                    {
                        role = "system"
                        content = "Extract the following information from the text and respond with a JSON object with ONLY the following keys: Name, Email, JobTitle, Department, Phone, StreetAddress1, StreetAddress2, City, StateProvince, PostCode, Country. Do not modify any text information in the extracted values. Where more than one phone is available, always take the office phone where indicated. Otherwise take the first phone value. PostCode and ZIP Code are often interchangeable terms in common use. Extract for the reasonable value for the appropriate code given the address text. For each key, infer a value from inputs. For fields without any corresponding information in inputs, use the value null."
                    };
                    {
                        role = "user"
                        content = "Houston Haynes Managing Partner Rower Consulting O: (404) 689-9467 A: 1 W Ct Square Suite 750, Decatur, GA 30030 W: https://www.rowerconsulting.com E: hhaynes@rowerconsulting.com"
                    };
                    {
                        role = "assistant"
                        content = "{\n    \"Name\": \"Houston Haynes\",\n    \"Email\": \"hhaynes@rowerconsulting.com\",\n    \"Company\": \"Rower Consulting\",\n    \"JobTitle\": \"Managing Partner\",\n    \"Phone\": \"(404) 689-9467\",\n    \"StreetAddress1\": \"1 W Ct Square\",\n    \"StreetAddress2\": \"Suite 750\",\n    \"City\": \"Decatur\",\n    \"StateProvince\": \"GA\",\n    \"PostCode\": \"30030\",\n    \"Country\": null\n}"
                    };
                    {                    
                        role = "user"
                        content = "Michael Johnson Chief Financial Officer Finance Department FinancePro Inc. 4321 Maple Street Suite 234 New York NY 10001 USADesk: +1 (555) 246-8102 Email: michael.johnson@financepro.fake"
                    };
                    {
                        role = "assistant"
                        content = "{\n \"City\": \"New York\",\n \"Country\": \"US\",\n \"Department\": \"Finance Department\",\n \"Email\": \"michael.johnson@financeproinc.com\"\n   , \"JobTitle\": \"Chief Financial Officer\" ,\n \"Name\": \"Michael Johnson\",\n \"Phone\": \"+1 (555) 246-8102\",\n \"PostCode\": \"10001\",\n \"StateProvince\": \"NY\",\n \"StreetAddress1\": \"4321 Maple Street\",\n \"StreetAddress2\": \"Suite 234\"\n}"
                    }
                    {
                        role = "user"
                        content = text
                    }
                ]
                stream = false
                format = {
                    ``type`` = "object"
                    properties = Map.ofList [
                        ("Name", { ``type`` = "string" })
                        ("Email", { ``type`` = "string" })
                        ("JobTitle", { ``type`` = "string" })
                        ("Department", { ``type`` = "string"})
                        ("Phone", { ``type`` = "string" })
                        ("StreetAddress1", { ``type`` = "string" })
                        ("StreetAddress2", { ``type`` = "string" })
                        ("City", { ``type`` = "string" })
                        ("StateProvince", { ``type`` = "string" })
                        ("PostCode", { ``type`` = "string" })
                        ("Country", { ``type`` = "string" })
                    ]
                    required = ["Name"; "Email"; "JobTitle"; "Department"; "Phone"; "StreetAddress1"; "StreetAddress2"; "City"; "StateProvince"; "PostCode"; "Country"]
                }
            }
            
            let content = new StringContent(JsonConvert.SerializeObject(requestPayload), Encoding.UTF8, "application/json")
            
            let! response = httpClient.PostAsync(endpoint, content) |> Async.AwaitTask
            let! responseBody = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            
            // Add debug logging
            printfn "Raw Response: %s" responseBody
            
            if String.IsNullOrEmpty(responseBody) then
                raise (ServerException(ServerError.ProcessError ProcessErrorKind.EmptyResponse))
                    
            if response.StatusCode <> HttpStatusCode.OK then
                raise (ServerException(ServerError.ProcessError (ProcessErrorKind.InvalidStatusCode (int response.StatusCode))))
                    
            let jsonResponse = JObject.Parse(responseBody)
            printfn "Parsed JSON: %A" jsonResponse
            
            let messageContent =
                match jsonResponse.SelectToken("message.content") with  // Updated path
                | null -> 
                    match jsonResponse.SelectToken("response") with    // Fallback path
                    | null -> raise (ServerException(ServerError.ProcessError ProcessErrorKind.NoContentField))
                    | token -> token.ToString()
                | token -> token.ToString()
                    
            printfn "Message Content: %s" messageContent
                    
            if String.IsNullOrEmpty(messageContent) then
                raise (ServerException(ServerError.ProcessError ProcessErrorKind.EmptyContent))
                    
            let form = JsonConvert.DeserializeObject<SignUpForm>(messageContent)
            
            if isNull (box form) then
                raise (ServerException(ServerError.ProcessError ProcessErrorKind.DeserializationError))
            
            eventProcessor.Post(ProcessSmartFormResult (timeStamp, streamToken, form))
            return form
                
        with 
        | :? ServerException as ex -> 
            printfn "Server Exception: %A" ex
            return raise ex
        | ex -> 
            printfn "Unexpected Exception: %A" ex
            return raise (ServerException(ServerError.Exception ex.Message))
    }

let establishStreamToken (timeStamp: EventDateTime, streamToken: StreamToken) = async {
    eventProcessor.Post(EstablishStreamToken (timeStamp, streamToken))
    }

let appendUnsubscribeStatus (timeStamp: EventDateTime, streamToken: StreamToken, subscribeToken: SubscriptionToken, emailAddress: EmailAddress, status: SubscribeStatus) = async {
    eventProcessor.Post(ProcessUnsubscribeStatus (timeStamp, streamToken, subscribeToken, emailAddress, status))
    Console.WriteLine $"Event Store unsubscribe link: {serverName}/api/prefs/confirm?token={subscribeToken}&api-key={apiKey}"
    }

let appendEmailStatus (timeStamp: EventDateTime, streamToken: StreamToken, verificationToken: VerificationToken, email: EmailAddress, status: EmailStatus) = async {
    eventProcessor.Post(ProcessEmailStatus (timeStamp, streamToken, verificationToken, email, status))
    Console.WriteLine $"Event Store verification link: {serverName}/api/prefs/confirm?token={verificationToken}&api-key={apiKey}"
    }

let processStreamClose (timeStamp: EventDateTime, streamToken: StreamToken) = async {
    eventProcessor.Post(ProcessStreamClose (timeStamp, streamToken))
    }

let processPageVisited (timeStamp: EventDateTime, streamToken: StreamToken, pageName: PageName) = async {
    eventProcessor.Post(ProcessPageVisited (timeStamp, streamToken, pageName))
    }

let processButtonClicked (timeStamp: EventDateTime, streamToken: StreamToken, buttonName: ButtonName) = async {
    eventProcessor.Post(ProcessButtonClicked (timeStamp, streamToken, buttonName))
    }

let processUserClientIP (timeStamp: EventDateTime,streamToken: StreamToken,  clientIP: ClientIP) = async {
    eventProcessor.Post(EstablishUserClientIP (timeStamp, streamToken, clientIP))
}

let processContactForm (timeStamp: EventDateTime,streamToken: StreamToken,  form: ContactForm) = async {
    eventProcessor.Post(ProcessContactForm (timeStamp, streamToken, form))
    return "Ok"
}

let processSignUpForm (timeStamp: EventDateTime, streamToken: StreamToken, form: SignUpForm) = async {
    eventProcessor.Post(ProcessSignUpForm (timeStamp, streamToken, form))
    return "Ok"
}

let flagEmailDomain (domain: string) = async {
    let connStr = Environment.GetEnvironmentVariable("GATEWAY_STORE")
    use conn = new NpgsqlConnection(connStr)
    use cmd = new NpgsqlCommand($"SELECT COUNT(*) FROM public.\"FreeEmailProviders\" WHERE \"Domain\" = @domain", conn)
    cmd.Parameters.AddWithValue("@domain", domain) |> ignore
    do! conn.OpenAsync() |> Async.AwaitTask
    let! result = cmd.ExecuteScalarAsync() |> Async.AwaitTask
    let count = result :?> int64
    return count > 0L
}

let parseDatabase (connectionString: string) =
        let parts = connectionString.Split(';')
        let databasePart = parts |> Array.find (_.StartsWith("Database="))
        databasePart.Split('=') |> Array.last

let getGicsTaxonomyAsync () =
    async {
        let connStr = Environment.GetEnvironmentVariable("GATEWAY_STORE")
        let databaseName = parseDatabase(connStr)
        let commandStr = $"SELECT * FROM \"%s{databaseName}\".public.\"GicsTaxonomy\";"
        use conn = new NpgsqlConnection(connStr)
        use cmd = new NpgsqlCommand(commandStr, conn)
        do! conn.OpenAsync() |> Async.AwaitTask
        use! reader = cmd.ExecuteReaderAsync() |> Async.AwaitTask
        let results = ResizeArray<GicsTaxonomy>()
        while reader.Read() do
            let subIndustryCode = if reader.IsDBNull(1) then "" else reader.GetString(1)
            let subIndustryName = if reader.IsDBNull(2) then "" else reader.GetString(2)
            let gicsTaxonomy = {
                SectorCode = reader.GetString(8)
                SectorName = reader.GetString(9)
                IndustryGroupCode = reader.GetString(6)
                IndustryGroupName = reader.GetString(7)
                IndustryCode = reader.GetString(4)
                IndustryName = reader.GetString(5)
                SubIndustryCode = subIndustryCode
                SubIndustryName = subIndustryName
            }
            results.Add(gicsTaxonomy)
        return results.ToArray()
    }

let service = {
    GetMessage = getMessage
    EstablishStreamToken = establishStreamToken
    EstablishUserClientIP = processUserClientIP
    AppendUnsubscribeStatus = appendUnsubscribeStatus
    AppendEmailStatus = appendEmailStatus
    FlagWebmailDomain = flagEmailDomain
    ProcessContactForm = processContactForm
    ProcessStreamClose = processStreamClose
    ProcessPageVisited = processPageVisited
    ProcessButtonClicked = processButtonClicked
    ProcessSmartForm = processSmartForm
    RetrieveSmartFormSubmittedCount = Aggregates.retrieveSmartFormSubmittedCount
    ProcessSignUpForm = processSignUpForm
    RetrieveDataPolicyChoice = Aggregates.retrieveDataPolicyChoice
    RetrieveEmailStatus = Aggregates.retrieveLatestEmailStatus
    RetrieveUnsubscribeStatus = Aggregates.retrieveLatestSubscribeStatus
    RetrieveLatestSubscriptionToken = Aggregates.getLatestSubscriptionToken
    RetrieveLatestVerificationToken = Aggregates.getLatestVerificationToken
    RetrieveContactFormSubmitted = Aggregates.retrieveContactFormSubmitted
    RetrieveSignUpFormSubmitted = Aggregates.retrieveSignUpFormSubmitted
    RetrieveUserSummary = Aggregates.retrieveUserSummaryAggregate
    RetrieveFullUserStream = Projections.retrieveFullUserStreamProjection
    RetrieveAllUserNames = Projections.retrieveUserNameProjection
    RetrieveOverviewTotals = Projections.retrieveOverviewTotals
    RetrieveClientIPLocations = Projections.retrieveClientIPLocations
    RetrieveVerifiedEmailDomains = Projections.retrieveVerifiedEmailDomains
    SendEmailVerification = sendEmailVerification
    CheckIfAdmin = Aggregates.retrieveAdminStatus
    LoadGicsTaxonomy = getGicsTaxonomyAsync
    RetrieveCountOfEmptyStreams = Projections.retrieveCountOfEmptyStreams
    ArchiveEmptyStreams = archiveEmptyStreams
    ProcessSemanticSearch = processSemanticSearch
}

let webApp : HttpHandler =
    let remoting logger =
        Remoting.createApi()
        |> Remoting.withRouteBuilder Service.RouteBuilder
        |> Remoting.fromValue service
        |> Remoting.withErrorHandler (Remoting.errorHandler logger)
        |> Remoting.buildHttpHandler
    choose [
        route "/api/prefs/confirm" >=> Notifications.emailVerificationHandler
        route "/api/prefs/unsubscribe" >=> Notifications.unsubscribeHandler
        Require.services<ILogger<_>> remoting
        htmlFile "public/index.html"
    ]