module CollabGateway.Server.SendGridHelpers

open System
open System.Net
open CollabGateway.Shared.API
open CollabGateway.Shared.Errors
open SendGrid
open SendGrid.Helpers.Mail

let private getFirstName (fullName: string) =
    let parts = fullName.Split(' ')
    if parts.Length > 0 then parts[0] else fullName

let serverName =
    match Environment.GetEnvironmentVariable("SERVER_NAME") with
    | null | "" -> "http://localhost:5000"
    | value -> value

let webServerName =
    match Environment.GetEnvironmentVariable("WEB_SERVER_NAME") with
    | null | "" -> "http://localhost:8080"
    | value -> value

let apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY")
let notificationKey = Environment.GetEnvironmentVariable("NOTIFICATION_KEY")

let sendEmailVerification (name: UserName, email,  verificationToken: VerificationToken, unsubscribeToken: SubscriptionToken) =
    task {
        try
            if String.IsNullOrEmpty(apiKey) then
                return! ServerError.failwith (ServerError.Exception "SendGrid API key is not set.")
            else
                let client = SendGridClient(apiKey)
                let from = EmailAddress("engineering@speakez.net", "SpeakEZ Engineering")
                let toAddress = EmailAddress(email.ToString(), name.ToString())
                let firstName = getFirstName name
                let verifyLink = $"{serverName}/api/prefs/confirm?token={verificationToken}&api-key={notificationKey}"
                let unsubscribeLink = $"{serverName}/api/prefs/unsubscribe?token={unsubscribeToken}&api-key={notificationKey}"
                let msg = MailHelper.CreateSingleTemplateEmail(from, toAddress, "d-6e266c53f5744695bc61c146d2d124b2",
                    dict [
                        "first_name", firstName
                        "email_verify_link", verifyLink
                        "unsubscribe_link", unsubscribeLink
                    ])
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

let sendEmailConfirmation (name: UserName, email, streamToken: StreamToken, unsubscribeToken: SubscriptionToken) =
    task {
        try
            let apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY")
            if String.IsNullOrEmpty(apiKey) then
                return! ServerError.failwith (ServerError.Exception "SendGrid API key is not set.")
            else
                let client = SendGridClient(apiKey)
                let from = EmailAddress("engineering@speakez.net", "SpeakEZ Engineering")
                let toAddress = EmailAddress(email, name)
                let firstName = getFirstName name
                let activityLink = $"{webServerName}/activity?ref={streamToken}"
                let unsubscribeLink = $"{serverName}/api/prefs/unsubscribe?token={unsubscribeToken}&api-key={notificationKey}"
                let msg = MailHelper.CreateSingleTemplateEmail(from, toAddress, "d-06d1530260ae4d21aeb31e6c1cab221a",
                    dict [
                        "first_name", firstName
                        "activity_page_link", activityLink
                        "unsubscribe_link", unsubscribeLink
                    ])
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

let sendUnsubscribeConfirmation (name: string, email: string) =
    task {
        try
            let apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY")
            if String.IsNullOrEmpty(apiKey) then
                return! ServerError.failwith (ServerError.Exception "SendGrid API key is not set.")
            else
                let client = SendGridClient(apiKey)
                let from = EmailAddress("engineering@speakez.net", "SpeakEZ Engineering")
                let toAddress = EmailAddress(email, name)
                let firstName = getFirstName name
                let msg = MailHelper.CreateSingleTemplateEmail(from, toAddress, "d-e422a18cce1441ac97b9d52b7c048191",
                    dict [
                        "first_name", firstName
                    ])
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