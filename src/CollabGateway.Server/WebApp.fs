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

let transmitSignUpForm (contactForm: SignUpForm) =
    task {
        try
            let apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY")
            if String.IsNullOrEmpty(apiKey) then
                return! ServerError.failwith (ServerError.Exception "SendGrid API key is not set.")
            else
                let client = SendGridClient(apiKey)
                let from = EmailAddress("engineering@speakez.net", "Engineering Team")
                let subject = "New SignUp Form Submission"
                let toAddress = EmailAddress("engineering@speakez.net", "Engineering Team")
                let plainTextContent = $"Name: %s{contactForm.Name}\n
                                            Email: %s{contactForm.Email}\n
                                            Job Title: %s{contactForm.JobTitle}\n
                                            Phone: %s{contactForm.Phone}\n
                                            Department: %s{contactForm.Department}\n
                                            Company: %s{contactForm.Company}\n
                                            Street Address 1: %s{contactForm.StreetAddress1}\n
                                            Street Address 2: %s{contactForm.StreetAddress2}\n
                                            City: %s{contactForm.City}\n
                                            State/Province: %s{contactForm.StateProvince}\n
                                            Post Code: %s{contactForm.PostCode}\n
                                            Country: %s{contactForm.Country}"
                let htmlContent = $"<strong>Name:</strong> %s{contactForm.Name}<br>
                                    <strong>Email:</strong> %s{contactForm.Email}<br>
                                    <strong>Job Title:</strong> %s{contactForm.JobTitle}<br>
                                    <strong>Phone:</strong> %s{contactForm.Phone}<br>
                                    <strong>Department:</strong> %s{contactForm.Department}<br>
                                    <strong>Company:</strong> %s{contactForm.Company}<br>
                                    <strong>Street Address 1:</strong> %s{contactForm.StreetAddress1}<br>
                                    <strong>Street Address 2:</strong> %s{contactForm.StreetAddress2}<br>
                                    <strong>City:</strong> %s{contactForm.City}<br>
                                    <strong>State/Province:</strong> %s{contactForm.StateProvince}<br>
                                    <strong>Post Code:</strong> %s{contactForm.PostCode}<br>
                                    <strong>Country:</strong> %s{contactForm.Country}"
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

let processSmartForm (streamToken: StreamToken, timeStamp: EventDateTime, text: SmartFormRawContent) : Async<SignUpForm> =
    task {
        let apiKey = Environment.GetEnvironmentVariable("AZUREOPENAI_API_KEY")
        if String.IsNullOrEmpty(apiKey) then
            return! ServerError.failwith (ServerError.Exception "Azure OpenAI API key is not set.")
        else
            Database.eventProcessor.Post(ProcessSmartFormInput (streamToken, timeStamp, text))
            let url = "https://addreslookup.openai.azure.com/openai/deployments/gpt-35-turbo/chat/completions?api-version=2024-02-15-preview"
            use httpClient = new HttpClient()
            httpClient.DefaultRequestHeaders.Add("api-key", apiKey)
            let requestPayload = {
                messages = [
                    {
                        role = "system"
                        content = "Extract the following information from the text and respond with a JSON object with ONLY the following keys: Name, Email, JobTitle, Phone, StreetAddress1, StreetAddress2, City, StateProvince, PostCode, Country. For each key, infer a value from inputs. For fields without any corresponding information in inputs, use the value null."
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
                        role = "user";
                        content = text
                    }
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
                let jsonResponse = JObject.Parse(responseBody)
                let messageContent = jsonResponse.SelectToken("choices[0].message.content").ToString()
                let form = JsonConvert.DeserializeObject<SignUpForm>(messageContent)
                Database.eventProcessor.Post(ProcessSmartFormResult (streamToken, timeStamp, form))
                return form
            else
                return! ServerError.failwith (ServerError.Exception $"Call failed to inference: {response.StatusCode} - {responseBody}")
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
            | :? ButtonEventCase as e ->
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
    Database.eventProcessor.Post(ProcessStreamToken (streamToken, timeStamp))
    }

let processStreamClose (streamToken: StreamToken, timeStamp: EventDateTime) = async {
    Database.eventProcessor.Post(ProcessSessionClose (streamToken, timeStamp))
    }

let processPageVisited (streamToken: StreamToken, timeStamp: EventDateTime, pageName: PageName) = async {
    Database.eventProcessor.Post(ProcessPageVisited (streamToken, timeStamp, pageName))
    }

let processButtonClicked (streamToken: StreamToken, timeStamp: EventDateTime, buttonName: ButtonName) = async {
    Console.WriteLine $"Button handler: {buttonName}"
    Database.eventProcessor.Post(ProcessButtonClicked (streamToken, timeStamp, buttonName))
    }

let processUserClientIP (streamToken: StreamToken, timeStamp: EventDateTime, clientIP: ClientIP) = async {
    Database.eventProcessor.Post(ProcessUserClientIP (streamToken, timeStamp, clientIP))
}

let processContactForm (streamToken: StreamToken, timeStamp: EventDateTime, form: ContactForm) = async {
    Database.eventProcessor.Post(ProcessContactForm (streamToken, timeStamp, form))
    let! result = transmitContactForm form
    return result
}

let processSignUpForm (streamToken: StreamToken, timeStamp: EventDateTime, form: SignUpForm) = async {
    Database.eventProcessor.Post(ProcessSignUpForm (streamToken, timeStamp, form))
    let! result = transmitSignUpForm form
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
    ProcessSmartForm = processSmartForm
    ProcessSignUpForm = processSignUpForm
    RetrieveUserSummary = Aggregates.retrieveUserSummaryAggregate
    RetrieveFullUserStream = Projections.retrieveFullUserStreamProjection
    RetrieveAllUserNames = Projections.retrieveUserStreamProjection
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