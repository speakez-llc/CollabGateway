namespace CollabGateway.Server

open System
open System.Net
open System.Net.Http
open Giraffe
open Microsoft.AspNetCore.Http
open CollabGateway.Shared.API
open CollabGateway.Shared.Errors
open SendGrid
open SendGrid.Helpers.Mail
open Newtonsoft.Json
open Newtonsoft.Json.Linq

module WebApp =

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
                return None
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

    let webApp : HttpHandler =
        choose [
            POST >=> route "/send-email" >=> sendEmailHandler
            // Add other routes here
        ]