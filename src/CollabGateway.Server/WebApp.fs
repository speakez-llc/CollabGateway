namespace CollabGateway.Server

open System
open System.Net
open Giraffe
open Microsoft.AspNetCore.Http
open CollabGateway.Shared.API
open CollabGateway.Shared.Errors
open SendGrid
open SendGrid.Helpers.Mail

module WebApp =

    let sendEmail (contactForm: ContactForm) =
        task {
            try
                let apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY")
                if String.IsNullOrEmpty(apiKey) then
                    return "SendGrid API key is not set."
                else
                    let client = SendGridClient(apiKey)
                    let from = EmailAddress("engineering@speakez.net", "Engineering Team")
                    let subject = "New Contact Form Submission"
                    let toAddress = EmailAddress("engineering@speakez.net", "Engineering Team")
                    let plainTextContent = sprintf "Name: %s\nEmail: %s\nMessage: %s" contactForm.Name contactForm.Email contactForm.Message
                    let htmlContent = sprintf "<strong>Name:</strong> %s<br><strong>Email:</strong> %s<br><strong>Message:</strong> %s" contactForm.Name contactForm.Email contactForm.Message
                    let msg = MailHelper.CreateSingleEmail(from, toAddress, subject, plainTextContent, htmlContent)

                    let! response = client.SendEmailAsync(msg)
                    if response.StatusCode = HttpStatusCode.OK || response.StatusCode = HttpStatusCode.Accepted then
                        return "Email sent successfully"
                    else
                        return $"Failed to send email: {response.StatusCode}"
            with
            | ex ->
                return $"Failed to send email: {ex.Message}"
        }
        |> Async.AwaitTask

    let service = {
        GetMessage = fun success ->
            task {
                if success then return "Hi from Server!"
                else return ServerError.failwith (ServerError.Exception "OMG, something terrible happened")
            }
            |> Async.AwaitTask
        SendEmailMessage = sendEmail
    }

    let sendEmailHandler : HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let requestToken = Environment.GetEnvironmentVariable("REQUEST_TOKEN")
                // echo REQUEST_TOKEN to console for debugging
                Console.WriteLine("REQUEST_TOKEN: " + requestToken)
                let authHeader = ctx.Request.Headers.["Authorization"].ToString()
                let token = if authHeader.StartsWith("Bearer ") then authHeader.Substring(7) else ""

                if String.IsNullOrEmpty(requestToken) || requestToken <> token then
                    return! json "Unauthorized" next ctx
                else
                    let! contactForm = ctx.BindJsonAsync<ContactForm>()
                    let! result = sendEmail contactForm
                    return! json result next ctx
            }

    let webApp : HttpHandler =
        choose [
            POST >=> route "/send-email" >=> sendEmailHandler
            // Add other routes here
        ]