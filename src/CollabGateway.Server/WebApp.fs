namespace CollabGateway.Server

open System
open System.Net
open System.Net.Http
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open CollabGateway.Shared.API
open CollabGateway.Shared.Errors
open SendGrid
open SendGrid.Helpers.Mail
open Newtonsoft.Json
open Newtonsoft.Json.Linq

module WebApp =

    let parseAndPrintMessage json =
        let jObject = JObject.Parse(json)
        let messageContent = jObject.SelectToken("choices[0].message.content").ToString()
        let formattedMessage = JObject.Parse(messageContent).ToString(Formatting.Indented)
        Console.WriteLine(formattedMessage)

    let formatJson (json: string) =
        let jObject = JObject.Parse(json)
        JsonConvert.SerializeObject(jObject, Formatting.Indented)
            .Replace(" ", "&nbsp;")
            .Replace("\n", "<br>")

    let sendEmail (contactForm: ContactForm) (geoInfo: string) =
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
                    let formattedGeoInfo = formatJson geoInfo
                    let plainTextContent = sprintf "Name: %s\nEmail: %s\nMessage: %s\nClient IP: %s\nGeo Info: %s" contactForm.Name contactForm.Email contactForm.Message contactForm.ClientIP geoInfo
                    let htmlContent = sprintf "<strong>Name:</strong> %s<br><strong>Email:</strong> %s<br><strong>Message:</strong> %s<br><strong>Client IP:</strong> %s<br><strong>Geo Info:</strong><br>%s" contactForm.Name contactForm.Email contactForm.Message contactForm.ClientIP formattedGeoInfo
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

    let getGeoInfo (clientIP: string) =
        task {
            let geoipifyToken = Environment.GetEnvironmentVariable("GEOIPIFY_TOKEN")
            if String.IsNullOrEmpty(geoipifyToken) then
                return! ServerError.failwith (ServerError.Exception "GeoApify API key is not set.")
            else
                let url = sprintf "https://geo.ipify.org/api/v2/country,city?apiKey=%s&ipAddress=%s" geoipifyToken clientIP
                use httpClient = new HttpClient()
                let! response = httpClient.GetStringAsync(url)
                return Some response
        }
        |> Async.AwaitTask

    let sendEmailHandler : HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let requestToken = Environment.GetEnvironmentVariable("REQUEST_TOKEN")
                let authHeader = ctx.Request.Headers.["Authorization"].ToString()
                let token = if authHeader.StartsWith("Bearer ") then authHeader.Substring(7) else ""

                if String.IsNullOrEmpty(requestToken) || requestToken <> token then
                    return! ServerError.failwith (ServerError.Authentication "Unauthorized")
                else
                    let! contactForm = ctx.BindJsonAsync<ContactForm>()
                    let! geoInfo = getGeoInfo contactForm.ClientIP
                    let geoInfoStr = match geoInfo with
                                     | Some info -> info
                                     | None -> "Geo information not available"
                    let! result = sendEmail contactForm geoInfoStr
                    return! json result next ctx
            }

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
                        { role = "system"; content = "Extract the following information from the text and respond with a JSON object with ONLY the following keys: name, email, title, phone, address1, address2, city, state, zip, country. For each key, infer a value from inputs. For fields without any corresponding information in inputs, use the value null." }
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
                    parseAndPrintMessage responseBody
                    return responseBody
                else
                    return! ServerError.failwith (ServerError.Exception $"Failed to call Azure OpenAI API: {response.StatusCode} - {responseBody}")
        }
        |> Async.AwaitTask

    let processClipboardTextHandler : HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let logger = ctx.GetLogger("processClipboardTextHandler")
                try
                    let! clipboardText = ctx.BindJsonAsync<{| text: string |}>()
                    if not (validateClipboardText clipboardText.text) then
                        logger.LogError("Invalid clipboard text.")
                        return! ServerError.failwith (ServerError.Exception "Invalid clipboard text.")
                    else
                        let! result = callAzureOpenAI clipboardText.text
                        return! json result next ctx
                with
                | ex ->
                    logger.LogError(ex, "An internal server error occurred.")
                    return! ServerError.failwith (ServerError.Exception $"An internal server error occurred: {ex.Message}")
            }

    let webApp : HttpHandler =
        choose [
            POST >=> route "/send-email" >=> sendEmailHandler
            POST >=> route "/process-clipboard-text" >=> processClipboardTextHandler
            // Add other routes here
        ]