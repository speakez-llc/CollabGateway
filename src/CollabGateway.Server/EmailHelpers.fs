module CollabGateway.Server.EmailHelpers

open System
open System.IO
open CollabGateway.Shared.API
open CollabGateway.Shared.Errors
open MailKit.Net.Smtp
open MimeKit

let private logEnvironmentVariable name value =
    if String.IsNullOrEmpty(value) then
        printfn $"Environment variable %s{name} is not set."
    else
        printfn $"Environment variable %s{name} is set to %s{value}."

let private logTemplateRead filePath content =
    if String.IsNullOrEmpty(content) then
        printfn $"Failed to read template from %s{filePath}."
    else
        printfn $"Successfully read template from %s{filePath}."

let private serverName =
    let value = Environment.GetEnvironmentVariable("VITE_BACKEND_URL")
    printfn $"Raw VITE_BACKEND_URL environment variable: %s{value}"
    match value with
    | null | "" -> failwith "VITE_BACKEND_URL environment variable is not set."
    | value -> value

let private webServerName =
    let value = Environment.GetEnvironmentVariable("VITE_WEB_URL")
    printfn $"Raw VITE_WEB_URL environment variable: %s{value}"
    match value with
    | null | "" -> failwith "VITE_WEB_URL environment variable is not set."
    | value -> value

let private notificationKey =
    let value = Environment.GetEnvironmentVariable("NOTIFICATION_KEY")
    logEnvironmentVariable "NOTIFICATION_KEY" value
    match value with
    | null | "" -> failwith "NOTIFICATION_KEY environment variable is not set."
    | value -> value

let private outlookUserName =
    let value = Environment.GetEnvironmentVariable("OUTLOOK_USER_NAME")
    logEnvironmentVariable "OUTLOOK_USER_NAME" value
    match value with
    | null | "" -> failwith "OUTLOOK_USER_NAME environment variable is not set."
    | value -> value

let private outlookPassword =
    let value = Environment.GetEnvironmentVariable("OUTLOOK_PASSWORD")
    logEnvironmentVariable "OUTLOOK_PASSWORD" value
    match value with
    | null | "" -> failwith "OUTLOOK_PASSWORD environment variable is not set."
    | value -> value

let private substituteParameters (template: string) (parameters: Map<string, string>) =
    parameters |> Map.fold (fun (acc: string) key value -> acc.Replace(key, value)) template

let svgToDataUri (svgPath: string) =
    let svgContent = File.ReadAllText(svgPath)
    let base64Svg = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(svgContent))
    $"data:image/svg+xml;base64,%s{base64Svg}"

let private readTemplate (filePath: string) =
    let content = File.ReadAllText(filePath)
    logTemplateRead filePath content
    content

let private getFirstName (fullName: string) =
    let parts = fullName.Split(' ')
    if parts.Length > 0 then parts[0] else fullName

let verifyLink (verificationToken: VerificationToken) =
    $"{serverName}/api/prefs/confirm?token={verificationToken}&api-key={notificationKey}"

let unsubscribeLink (unsubscribeToken: SubscriptionToken) =
    $"{serverName}/api/prefs/unsubscribe?token={unsubscribeToken}&api-key={notificationKey}"

let activityPageLink (streamToken: StreamToken) = $"{webServerName}/activity?ref={streamToken}"

let verifyEmailParameters (name: UserName, verificationToken: VerificationToken, unsubscribeToken: SubscriptionToken) =
    let firstName = getFirstName name
    let emailVerifyLink = verifyLink verificationToken
    let unsubscribeLink = unsubscribeLink unsubscribeToken
    let svgDataUri = svgToDataUri "./Templates/CollabNarrow.svg"

    let parameters = Map.ofList [
        ("{{first_name}}", firstName)
        ("{{email_verify_link}}", emailVerifyLink)
        ("{{unsubscribe_link}}", unsubscribeLink)
        ("{{svg_data_uri}}", svgDataUri)
    ]
    parameters

let confirmEmailParameters (name: UserName, streamToken: StreamToken, unsubscribeToken: SubscriptionToken) =
    let firstName = getFirstName name
    let unsubscribeLink = unsubscribeLink unsubscribeToken
    let activityPageLink = activityPageLink streamToken
    let svgDataUri = svgToDataUri "./Templates/CollabNarrow.svg"

    let parameters = Map.ofList [
        ("{{first_name}}", firstName)
        ("{{activity_page_link}}", activityPageLink)
        ("{{unsubscribe_link}}", unsubscribeLink)
        ("{{svg_data_uri}}", svgDataUri)
    ]
    parameters

let unsubscribeParameters (name: UserName) =
    Map.ofList [
        ("{{first_name}}", getFirstName name)
        ("{{svg_data_uri}}", svgToDataUri "./Templates/CollabNarrow.svg")
    ]

let verifyEmailBody (name: UserName, verificationToken: VerificationToken, unsubscribeToken: SubscriptionToken) =
    printfn $"Generating email body for {name} with verification token {verificationToken} and unsubscribe token {unsubscribeToken}"
    let parameters = verifyEmailParameters(name, verificationToken, unsubscribeToken)
    let verifyEmailTemplate = readTemplate "./Templates/VerifyEmail.html"
    let body = substituteParameters verifyEmailTemplate parameters
    body

let confirmThankYouBody (name: UserName, streamToken: StreamToken, unsubscribeToken: SubscriptionToken) =
    let parameters = confirmEmailParameters(name, streamToken, unsubscribeToken)
    let confirmThankYouTemplate = readTemplate "./Templates/ConfirmationThankYou.html"
    substituteParameters confirmThankYouTemplate parameters

let unsubscribeConfirmationBody (name: UserName) =
    let parameters = unsubscribeParameters(name)
    let confirmUnsubscribeTemplate = readTemplate "./Templates/UnsubscribeConfirmation.html"
    substituteParameters confirmUnsubscribeTemplate parameters

let createAndSendEmail (toAddress: EmailAddress) (name: UserName) (subject: string) (body: string) =
    printfn $"Sending email to %s{toAddress}..."
    let message = new MimeMessage()
    message.From.Add(MailboxAddress("SpeakEZ Collab", "collab@speakez.net"))
    message.To.Add(MailboxAddress(name, toAddress))
    message.Subject <- subject

    let bodyBuilder = BodyBuilder()
    bodyBuilder.HtmlBody <- body

    message.Body <- bodyBuilder.ToMessageBody()

    use client = new SmtpClient()
    try
        printfn "Connecting to SMTP server..."
        client.Connect("mail.privateemail.com", 465, MailKit.Security.SecureSocketOptions.SslOnConnect)
        printfn "Authenticating..."
        client.Authenticate(outlookUserName, outlookPassword)
        printfn "Sending email..."
        client.Send(message) |> ignore
        printfn "Disconnecting from SMTP server..."
        client.Disconnect(true)
        printfn $"Email sent successfully to %s{toAddress}"
    with
    | :? SmtpCommandException as ex ->
        printfn $"SMTP Command Error: %s{ex.Message}"
        printfn $"StatusCode: %A{ex.StatusCode}"
        raise ex
    | :? SmtpProtocolException as ex ->
        printfn $"SMTP Protocol Error: %s{ex.Message}"
        raise ex
    | ex ->
        printfn $"Unexpected Error: %s{ex.Message}"
        raise ex

let sendEmailVerification (name: UserName, email: EmailAddress, verificationToken: VerificationToken, unsubscribeToken: SubscriptionToken) =
    task {
        try
            let body = verifyEmailBody(name, verificationToken, unsubscribeToken)
            createAndSendEmail email name "Rower Collab Gateway: Verify Your Email Address" body
        with
        | ex ->
            return! ServerError.failwith (ServerError.Exception $"Failed to send email: {ex.Message}")
    }
    |> Async.AwaitTask

let sendEmailConfirmation (name: UserName, email: EmailAddress, streamToken: StreamToken, unsubscribeToken: SubscriptionToken) =
    task {
        try
            let body = confirmThankYouBody(name, streamToken, unsubscribeToken)
            createAndSendEmail email name "Rower Collab Gateway: Thank You for Confirming Your Email" body
        with
        | ex ->
            return! ServerError.failwith (ServerError.Exception $"Failed to send email: {ex.Message}")
    }
    |> Async.AwaitTask

let sendUnsubscribeConfirmation (name: UserName, email: EmailAddress) =
    task {
        try
            let body = unsubscribeConfirmationBody(name)
            createAndSendEmail email name "Rower Collab Gateway: You Have Been Unsubscribed" body
        with
        | ex ->
            return! ServerError.failwith (ServerError.Exception $"Failed to send email: {ex.Message}")
    }
    |> Async.AwaitTask