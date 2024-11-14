﻿module CollabGateway.Server.WebApp

open System
open System.Net
open System.Net.Http
open Giraffe
open Giraffe.GoodRead
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Microsoft.Extensions.Logging
open CollabGateway.Shared.API
open CollabGateway.Shared.Events
open CollabGateway.Shared.Errors
open CollabGateway.Server.SendGridHelpers
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open Npgsql

let private apiKey = Environment.GetEnvironmentVariable("NOTIFICATION_KEY")
let serverName =
    match Environment.GetEnvironmentVariable("SERVER_NAME") with
    | null | "" -> "http://localhost:5000"
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

let processSmartForm (timeStamp: EventDateTime, streamToken: StreamToken, text: SmartFormRawContent) : Async<SignUpForm> =
    task {
        let apiKey = Environment.GetEnvironmentVariable("AZUREOPENAI_API_KEY")
        if String.IsNullOrEmpty(apiKey) then
            return! ServerError.failwith (ServerError.Exception "Azure OpenAI API key is not set.")
        else
            Database.eventProcessor.Post(ProcessSmartFormInput (timeStamp, streamToken, text))
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
                Database.eventProcessor.Post(ProcessSmartFormResult (timeStamp, streamToken, form))
                return form
            else
                return! ServerError.failwith (ServerError.Exception $"Call failed to inference: {response.StatusCode} - {responseBody}")
    }
    |> Async.AwaitTask

let establishStreamToken (timeStamp: EventDateTime, streamToken: StreamToken) = async {
    Database.eventProcessor.Post(EstablishStreamToken (timeStamp, streamToken))
    }

let appendUnsubscribeStatus (timeStamp: EventDateTime, streamToken: StreamToken, subscribeToken: SubscriptionToken, emailAddress: EmailAddress, status: SubscribeStatus) = async {
    Database.eventProcessor.Post(ProcessUnsubscribeStatus (timeStamp, streamToken, subscribeToken, emailAddress, status))
    }

let appendEmailStatus (timeStamp: EventDateTime, streamToken: StreamToken, verificationToken: VerificationToken, email: EmailAddress, status: EmailStatus) = async {
    Database.eventProcessor.Post(ProcessEmailStatus (timeStamp, streamToken, verificationToken, email, status))
    Console.WriteLine $"Event Store verification link: {serverName}/api/prefs/confirm?token={verificationToken}&api-key={apiKey}"
    }

let processStreamClose (timeStamp: EventDateTime, streamToken: StreamToken) = async {
    Database.eventProcessor.Post(ProcessStreamClose (timeStamp, streamToken))
    }

let processPageVisited (timeStamp: EventDateTime, streamToken: StreamToken, pageName: PageName) = async {
    Database.eventProcessor.Post(ProcessPageVisited (timeStamp, streamToken, pageName))
    }

let processButtonClicked (timeStamp: EventDateTime, streamToken: StreamToken, buttonName: ButtonName) = async {
    Database.eventProcessor.Post(ProcessButtonClicked (timeStamp, streamToken, buttonName))
    }

let processUserClientIP (timeStamp: EventDateTime,streamToken: StreamToken,  clientIP: ClientIP) = async {
    Database.eventProcessor.Post(EstablishUserClientIP (timeStamp, streamToken, clientIP))
}

let processContactForm (timeStamp: EventDateTime,streamToken: StreamToken,  form: ContactForm) = async {
    Database.eventProcessor.Post(ProcessContactForm (timeStamp, streamToken, form))
    return "Ok"
}

let processSignUpForm (timeStamp: EventDateTime, streamToken: StreamToken, form: SignUpForm) = async {
    Database.eventProcessor.Post(ProcessSignUpForm (timeStamp, streamToken, form))
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
    RetrieveEmailStatus = Aggregates.retrieveEmailStatus
    RetrieveUnsubscribeStatus = Aggregates.retrieveSubscribeStatus
    RetrieveContactFormSubmitted = Aggregates.retrieveContactFormSubmitted
    RetrieveSignUpFormSubmitted = Aggregates.retrieveSignUpFormSubmitted
    RetrieveUserSummary = Aggregates.retrieveUserSummaryAggregate
    RetrieveFullUserStream = Projections.retrieveFullUserStreamProjection
    RetrieveAllUserNames = Projections.retrieveUserNameProjection
    RetrieveOverviewTotals = Projections.retrieveOverviewTotals
    RetrieveClientIPLocations = Projections.retrieveClientIPLocations
    RetrieveVerifiedEmailDomains = Projections.retrieveVerifiedEmailDomains
    SendEmailVerification = sendEmailVerification
}

let webApp : HttpHandler =
    let remoting logger =
        Remoting.createApi()
        |> Remoting.withRouteBuilder Service.RouteBuilder
        |> Remoting.fromValue service
        |> Remoting.withErrorHandler (Remoting.errorHandler logger)
        |> Remoting.buildHttpHandler
    choose [
        route "/api/prefs/confirm" >=> Notifications.verificationEmailHandler
        route "/api/prefs/unsubscribe" >=> Notifications.unsubscribeHandler
        Require.services<ILogger<_>> remoting
        htmlFile "public/index.html"
    ]