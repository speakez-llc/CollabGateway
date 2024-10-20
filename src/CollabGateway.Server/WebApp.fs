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
    | ProcessSessionToken of SessionToken
    | ProcessUserClientIP of SessionToken * ClientIP
    | ProcessPageVisited of SessionToken * PageName
    | ProcessButtonClicked of SessionToken * ButtonName
    | ProcessSessionClose of SessionToken

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
        | ProcessSessionToken sessionToken ->
            use session = Database.store.LightweightSession()
            let streamState = session.Events.FetchStreamState(sessionToken)
            let event =
                if streamState = null then
                    UserSessionInitiated { Id = Guid.NewGuid(); SessionID = sessionToken }
                else
                    UserSessionResumed { Id = Guid.NewGuid(); SessionID = sessionToken }
            session.Events.Append(sessionToken, [| event :> obj |]) |> ignore
            do! session.SaveChangesAsync() |> Async.AwaitTask
        | ProcessUserClientIP (sessionToken, clientIP) ->
            use session = Database.store.LightweightSession()
            let! allEvents = session.Events.FetchStream(sessionToken) |> Task.FromResult |> Async.AwaitTask
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
                Console.WriteLine $"No new event needed for IP: %s{clientIP} as it matches an existing event."
            | None ->
                Console.WriteLine $"Creating event for new IP: %s{clientIP}"
                let! userGeoInfo = getGeoInfo clientIP
                let event =
                    if sessionEvents |> Seq.exists (function | UserClientIPDetected _ | UserClientIPUpdated _ -> true | _ -> false) then
                        UserClientIPUpdated { Id = Guid.NewGuid(); UserClientIP = clientIP; UserGeoInfo = userGeoInfo }
                    else
                        UserClientIPDetected { Id = Guid.NewGuid(); UserClientIP = clientIP; UserGeoInfo = userGeoInfo }
                session.Events.Append(sessionToken, [| event :> obj |]) |> ignore
                do! session.SaveChangesAsync() |> Async.AwaitTask
        | ProcessPageVisited (sessionToken, pageName) ->
            use session = Database.store.LightweightSession()
            let pageCase =
                match pageName with
                | HomePage -> HomePageVisited { Id = Guid.NewGuid() }
                | ProjectPage -> ProjectPageVisited { Id = Guid.NewGuid() }
                | CMSDataPage -> DataPageVisited { Id = Guid.NewGuid() }
                | SignUpPage -> SignupPageVisited { Id = Guid.NewGuid() }
                | RowerPage -> RowerPageVisited { Id = Guid.NewGuid() }
                | SpeakEZPage -> SpeakEZPageVisited { Id = Guid.NewGuid() }
                | ContactPage -> ContactPageVisited { Id = Guid.NewGuid() }
                | PartnersPage -> PartnersPageVisited { Id = Guid.NewGuid() }
            session.Events.Append(sessionToken, [| pageCase :> obj |]) |> ignore
            do! session.SaveChangesAsync() |> Async.AwaitTask
        | ProcessButtonClicked (sessionToken, buttonName) ->
            use session = Database.store.LightweightSession()
            let buttonCase =
                match buttonName with
                | HomeButton -> HomeButtonClicked { Id = Guid.NewGuid() }
                | HomeProjectButton -> HomeProjectButtonClicked { Id = Guid.NewGuid() }
                | HomeSignUpButton -> HomeSignUpButtonClicked { Id = Guid.NewGuid() }
                | ProjectButton -> ProjectButtonClicked { Id = Guid.NewGuid() }
                | ProjectDataButton -> ProjectDataButtonClicked { Id = Guid.NewGuid() }
                | ProjectSignUpButton -> ProjectSignUpButtonClicked { Id = Guid.NewGuid() }
                | CMSDataButton -> DataButtonClicked { Id = Guid.NewGuid() }
                | CMSDataSignUpButton -> DataSignUpButtonClicked { Id = Guid.NewGuid() }
                | SignUpButton -> SignUpButtonClicked { Id = Guid.NewGuid() }
                | RowerButton -> RowerButtonClicked { Id = Guid.NewGuid() }
                | RowerSignUpButton -> RowerSignUpButtonClicked { Id = Guid.NewGuid() }
                | SpeakEZButton -> SpeakEZButtonClicked { Id = Guid.NewGuid() }
                | SpeakEZSignUpButton -> SpeakEZSignUpButtonClicked { Id = Guid.NewGuid() }
                | ContactButton -> ContactButtonClicked { Id = Guid.NewGuid() }
                | PartnersButton -> PartnersButtonClicked { Id = Guid.NewGuid() }
                | RowerSiteButton -> RowerSiteButtonClicked { Id = Guid.NewGuid() }
                | CuratorSiteButton -> CuratorSiteButtonClicked { Id = Guid.NewGuid() }
                | TableauSiteButton -> TableauSiteButtonClicked { Id = Guid.NewGuid() }
                | PowerBISiteButton -> PowerBISiteButtonClicked { Id = Guid.NewGuid() }
                | ThoughtSpotSiteButton -> ThoughtSpotSiteButtonClicked { Id = Guid.NewGuid() }
                | SpeakEZSiteButton -> SpeakEZSiteButtonClicked { Id = Guid.NewGuid() }
            session.Events.Append(sessionToken, [| buttonCase :> obj |]) |> ignore
            do! session.SaveChangesAsync() |> Async.AwaitTask
        | ProcessSessionClose sessionToken ->
            use session = Database.store.LightweightSession()
            let event = UserSessionClosed { Id = Guid.NewGuid(); SessionID = sessionToken }
            session.Events.Append(sessionToken, [| event :> obj |]) |> ignore
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

let processContactForm (contactForm: ContactForm) =
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

let processSessionToken (sessionToken: SessionToken) = async {
    eventProcessor.Post(ProcessSessionToken sessionToken)
    }

let processSessionClose (sessionToken: SessionToken) = async {
    eventProcessor.Post(ProcessSessionClose sessionToken)
    }

let processPageVisited (sessionToken: SessionToken, pageName: PageName) = async {
    eventProcessor.Post(ProcessPageVisited (sessionToken, pageName))
    }

let processButtonClicked (sessionToken: SessionToken, buttonName: ButtonName) = async {
    eventProcessor.Post(ProcessButtonClicked (sessionToken, buttonName))
    }

let processUserClientIP (sessionToken: SessionToken, clientIP: ClientIP) = async {
    eventProcessor.Post(ProcessUserClientIP (sessionToken, clientIP))
}

let service = {
    GetMessage = fun success ->
        task {
            if success then return "Hi from Server!"
            else return ServerError.failwith (ServerError.Exception "OMG, something terrible happened")
        }
        |> Async.AwaitTask
    ProcessContactForm = processContactForm
    ProcessSessionToken = processSessionToken
    ProcessSessionClose = processSessionClose
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