module CollabGateway.Server.Notifications

open System
open CollabGateway.Shared.API
open CollabGateway.Shared.Errors
open CollabGateway.Shared.Events
open Giraffe
open Microsoft.AspNetCore.Http

let private apiKey = Environment.GetEnvironmentVariable("NOTIFICATION_KEY")

let getEmailStatus (streamToken: StreamToken, email: EmailAddress): Async<EventToken * EmailStatus> =
    async {
        use session = Database.store.LightweightSession()
        let! allEvents = session.Events.FetchStreamAsync(streamToken) |> Async.AwaitTask
        let userStreamInitiatedEvent =
            allEvents
            |> Seq.pick (fun e ->
                match e.Data with
                | :? FormEventCase as eventCase ->
                    match eventCase with
                    | EmailStatusAppended { EventToken = et; Status = status } -> Some (et, status)
                    | _ -> None
                | _ -> None)
        return userStreamInitiatedEvent
    }

let processEmailVerificationCompletion (streamToken: StreamToken, eventToken: EventToken, timeStamp: EventDateTime) = async {
    Database.eventProcessor.Post(ProcessEmailStatus (streamToken, eventToken, timeStamp))
    }

let private authenticateApiKey (next: HttpFunc) (ctx: HttpContext) =
    task {
        let apiKeyParam = ctx.Request.Query["api_key"] |> string
        if apiKeyParam = apiKey then
            return! next ctx
        else
            ctx.Response.StatusCode <- 401
            return! text "Unauthorized" next ctx
    }

let confirmEmailHandler (next: HttpFunc) (ctx: HttpContext) =
    task {
        let verificationToken : EventToken = ctx.Request.Query["token"] |> string |> Guid.Parse
        let streamToken = ctx.Request.Query["entity"] |> string |> Guid.Parse
        let! eventToken = getEmailVerificationToken streamToken
        if verificationToken = eventToken then
            let timeStamp = DateTime.Now
            processEmailVerificationCompletion (streamToken, verificationToken, timeStamp) |> ignore
            ctx.Response.StatusCode <- 200
            return! text $"Email confirmed with token: {verificationToken}" next ctx
        else
            return! ServerError.failwith (ServerError.Exception "Invalid verification token.") next ctx
    }

let unsubscribeHandler (next: HttpFunc) (ctx: HttpContext) =
    task {
        let unsubscribeToken : EventToken = ctx.Request.Query["token"] |> string |> Guid.Parse
        let streamToken = ctx.Request.Query["entity"] |> string |> Guid.Parse
        let! eventToken = getEmailVerificationToken streamToken
        if verificationToken = eventToken then
            let timeStamp = DateTime.Now
            processUnsubscribeFromNotifications (streamToken, verificationToken, timeStamp) |> ignore
            ctx.Response.StatusCode <- 200
            return! text $"You have been unsubscribed from notifications - token: {verificationToken}" next ctx
        else
            return! ServerError.failwith (ServerError.Exception "Invalid verification token.") next ctx
    }

let notificationManager : HttpHandler  =
    choose [
        route "/api/prefs/confirm" >=> authenticateApiKey >=> confirmEmailHandler
        route "/api/prefs/unsubscribe" >=> authenticateApiKey >=> unsubscribeHandler
    ]