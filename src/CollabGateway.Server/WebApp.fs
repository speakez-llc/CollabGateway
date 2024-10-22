module CollabGateway.Server.WebApp

open System
open System.Net
open System.Net.Http
open System.Threading.Tasks
open Giraffe
open Giraffe.GoodRead
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Microsoft.Extensions.Logging
open CollabGateway.Shared.API
open CollabGateway.Shared.Events
open CollabGateway.Shared.Errors
open SendGrid
open SendGrid.Helpers.Mail
open Newtonsoft.Json
open Newtonsoft.Json.Linq

type EventProcessingMessage =
    | ProcessStreamToken of StreamToken * EventDateTime
    | ProcessUserClientIP of StreamToken * EventDateTime * ClientIP
    | ProcessPageVisited of StreamToken * EventDateTime * PageName
    | ProcessButtonClicked of StreamToken * EventDateTime * ButtonName
    | ProcessSessionClose of StreamToken * EventDateTime
    | ProcessContactForm of StreamToken * EventDateTime * ContactForm

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
        | ProcessStreamToken (streamToken, timeStamp) ->
            use session = Database.store.LightweightSession()
            let streamState = session.Events.FetchStreamState(streamToken)
            let event =
                if streamState = null then
                    UserStreamInitiated { Id = Guid.NewGuid(); TimeStamp = timeStamp; StreamID = streamToken }
                else
                    UserStreamResumed { Id = Guid.NewGuid(); TimeStamp = timeStamp; StreamID = streamToken }
            session.Events.Append(streamToken, [| event :> obj |]) |> ignore
            do! session.SaveChangesAsync() |> Async.AwaitTask
        | ProcessUserClientIP (streamToken, timeStamp, clientIP) ->
            use session = Database.store.LightweightSession()
            let! allEvents = session.Events.FetchStream(streamToken) |> Task.FromResult |> Async.AwaitTask
            let unwrappedEvents =
                allEvents
                |> Seq.map (_.Data )
            let sessionEvents = unwrappedEvents |> Seq.choose (function | :? SessionEventCase as e -> Some e | _ -> None)
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
                    if sessionEvents |> Seq.exists (function | UserClientIPDetected _ | UserClientIPUpdated _ -> true | _ -> false) then
                        UserClientIPUpdated { Id = Guid.NewGuid(); TimeStamp = timeStamp; UserClientIP = clientIP; UserGeoInfo = userGeoInfo }
                    else
                        UserClientIPDetected { Id = Guid.NewGuid(); TimeStamp = timeStamp; UserClientIP = clientIP; UserGeoInfo = userGeoInfo }
                session.Events.Append(streamToken, [| event :> obj |]) |> ignore
                do! session.SaveChangesAsync() |> Async.AwaitTask
        | ProcessPageVisited (streamToken, timeStamp, pageName) ->
            use session = Database.store.LightweightSession()
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
            session.Events.Append(streamToken, [| pageCase :> obj |]) |> ignore
            do! session.SaveChangesAsync() |> Async.AwaitTask
        | ProcessButtonClicked (streamToken, timeStamp, buttonName) ->
            use session = Database.store.LightweightSession()
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
            session.Events.Append(streamToken, [| buttonCase :> obj |]) |> ignore
            do! session.SaveChangesAsync() |> Async.AwaitTask
        | ProcessSessionClose (streamToken, timeStamp) ->
            use session = Database.store.LightweightSession()
            let event = UserStreamClosed { Id = Guid.NewGuid(); TimeStamp = timeStamp; StreamID = streamToken }
            session.Events.Append(streamToken, [| event :> obj |]) |> ignore
            do! session.SaveChangesAsync() |> Async.AwaitTask
        | ProcessContactForm (streamToken, timeStamp, contactForm) ->
            use session = Database.store.LightweightSession()
            let event = ContactFormSubmitted { Id = Guid.NewGuid(); TimeStamp = timeStamp; Form = contactForm }
            session.Events.Append(streamToken, [| event :> obj |]) |> ignore
            do! session.SaveChangesAsync() |> Async.AwaitTask
        return! loop ()
    }
    loop ()
)

let formatJson string =
    let jObject = JObject.Parse(string)
    JsonConvert.SerializeObject(jObject, Formatting.Indented)
        .Replace(" ", "&nbsp;")
        .Replace("\n", "<br>")

let transmitContactForm (contactForm: ContactForm) =
    task {
        try
            let apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY")
            if String.IsNullOrEmpty(apiKey) then
                return! ServerError.failwith (ServerError.Exception "SendGrid API key is not set.")
            else
                let client = SendGridClient(apiKey)
                let from = EmailAddress("engineering@speakez.net", "Engineering Team")
                let subject = "New Contact Form Submission"
                let toAddress = EmailAddress("engineering@speakez.net", "Engineering Team")
                let plainTextContent = $"Name: %s{contactForm.Name}\nEmail: %s{contactForm.Email}\nMessage: %s{contactForm.MessageBody}"
                let htmlContent = $"<strong>Name:</strong> %s{contactForm.Name}<br><strong>Email:</strong> %s{contactForm.Email}<br><strong>Message:</strong> %s{contactForm.MessageBody}"
                let msg = MailHelper.CreateSingleEmail(from, toAddress, subject, plainTextContent, htmlContent)
                let! response = client.SendEmailAsync(msg)
                if response.StatusCode = HttpStatusCode.OK || response.StatusCode = HttpStatusCode.Accepted then
                    return "Email sent successfully"
                else
                    return! ServerError.failwith (ServerError.Exception $"Failed to send email: {response.StatusCode}")
        with
        | ex ->
            return! ServerError.failwith (ServerError.Exception $"Failed to send email: {ex.Message}")
    }
    |> Async.AwaitTask

let validateClipboardText (text: string) =
    if String.IsNullOrWhiteSpace(text) then
        false
    else
        // Add more validation logic if needed
        true

let callAzureOpenAI (text: string) =
    task {
        let apiKey = Environment.GetEnvironmentVariable("AZUREOPENAI_API_KEY")
        if String.IsNullOrEmpty(apiKey) then
            return! ServerError.failwith (ServerError.Exception "Azure OpenAI API key is not set.")
        else
            let url = "https://addreslookup.openai.azure.com/openai/deployments/gpt-35-turbo/chat/completions?api-version=2024-02-15-preview"
            use httpClient = new HttpClient()
            httpClient.DefaultRequestHeaders.Add("api-key", apiKey)
            let requestPayload = {
                messages = [
                    { role = "system"; content = "Extract the following information from the text and respond with a JSON object with ONLY the following keys: name, email, title, phone, address1, address2, city, state, zip, country. For each key, infer a value from inputs. Only return one value for each key. For fields without any corresponding information in inputs, use the value null." }
                    { role = "user"; content = text }
                ]
                max_tokens = 800
                temperature = 0.7
                frequency_penalty = 0.0
                presence_penalty = 0.0
                top_p = 0.95
                stop = None
            }
            let content = new StringContent(JsonConvert.SerializeObject(requestPayload), System.Text.Encoding.UTF8, "application/json")
            let! response = httpClient.PostAsync(url, content)
            let! responseBody = response.Content.ReadAsStringAsync()
            if response.StatusCode = HttpStatusCode.OK then
                return responseBody
            else
                return! ServerError.failwith (ServerError.Exception $"Failed to call Azure OpenAI API: {response.StatusCode} - {responseBody}")
    }
    |> Async.AwaitTask


let retrieveDataPolicyChoice (streamToken: StreamToken) = async {
    use session = Database.store.LightweightSession()
    let! allEvents = session.Events.FetchStream(streamToken) |> Task.FromResult |> Async.AwaitTask
    let eventsWithTimestamps =
        allEvents
        |> Seq.map (fun e -> e.Timestamp, e.Data)
    let dataPolicyEvents =
        eventsWithTimestamps
        |> Seq.choose (fun (timestamp, data) ->
            match data with
            | :? BaseEventCase as e ->
                match e with
                | DataPolicyAcceptButtonClicked _ -> Some (timestamp, e)
                | DataPolicyDeclineButtonClicked _ -> Some (timestamp, e)
                | _ -> None
            | _ -> None)
        |> Seq.sortByDescending fst

    match Seq.tryHead dataPolicyEvents with
    | Some (_, DataPolicyAcceptButtonClicked _) -> return Accepted
    | Some (_, DataPolicyDeclineButtonClicked _) -> return Declined
    | _ -> return Unknown
}
let processStreamToken (streamToken: StreamToken, timeStamp: EventDateTime) = async {
    eventProcessor.Post(ProcessStreamToken (streamToken, timeStamp))
    }

let processStreamClose (streamToken: StreamToken, timeStamp: EventDateTime) = async {
    eventProcessor.Post(ProcessSessionClose (streamToken, timeStamp))
    }

let processPageVisited (streamToken: StreamToken, timeStamp: EventDateTime, pageName: PageName) = async {
    eventProcessor.Post(ProcessPageVisited (streamToken, timeStamp, pageName))
    }

let processButtonClicked (streamToken: StreamToken, timeStamp: EventDateTime, buttonName: ButtonName) = async {
    Console.WriteLine $"Button handler: {buttonName}"
    eventProcessor.Post(ProcessButtonClicked (streamToken, timeStamp, buttonName))
    }

let processUserClientIP (streamToken: StreamToken, timeStamp: EventDateTime, clientIP: ClientIP) = async {
    eventProcessor.Post(ProcessUserClientIP (streamToken, timeStamp, clientIP))
}

let processContactForm (streamToken: StreamToken, timeStamp: EventDateTime, form: ContactForm) = async {
    eventProcessor.Post(ProcessContactForm (streamToken, timeStamp, form))
    let! result = transmitContactForm form
    return result
}

let service = {
    GetMessage = fun success ->
        task {
            if success then return "Hi from Server!"
            else return ServerError.failwith (ServerError.Exception "OMG, something terrible happened")
        }
        |> Async.AwaitTask
    RetrieveDataPolicyChoice = retrieveDataPolicyChoice
    ProcessContactForm = processContactForm
    ProcessStreamToken = processStreamToken
    ProcessStreamClose = processStreamClose
    ProcessPageVisited = processPageVisited
    ProcessButtonClicked = processButtonClicked
    ProcessUserClientIP = processUserClientIP
}

let webApp : HttpHandler =
    let remoting logger =
        Remoting.createApi()
        |> Remoting.withRouteBuilder Service.RouteBuilder
        |> Remoting.fromValue service
        |> Remoting.withErrorHandler (Remoting.errorHandler logger)
        |> Remoting.buildHttpHandler
    choose [
        Require.services<ILogger<_>> remoting
        htmlFile "public/index.html"
    ]