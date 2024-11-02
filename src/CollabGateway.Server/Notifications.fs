﻿module CollabGateway.Server.Notifications

open System
open CollabGateway.Shared.API
open CollabGateway.Shared.Errors
open CollabGateway.Shared.Events
open Giraffe
open Microsoft.AspNetCore.Http

let private apiKey = Environment.GetEnvironmentVariable("NOTIFICATION_KEY")

let getEmailStatus (eventToken: EventToken): Async<StreamToken * EmailAddress * EmailStatus> =
    async {
        use session = Database.store.LightweightSession()
        let allEvents = session.Events.QueryAllRawEvents()
        let streamTokenOption =
            allEvents
            |> Seq.tryPick (fun e ->
                match e.Data with
                | :? FormEventCase as eventCase ->
                    match eventCase with
                    | EmailStatusAppended { EventToken = et; EmailAddress = _; Status = _ } when et = eventToken -> Some e.StreamId
                    | _ -> None
                | _ -> None)

        match streamTokenOption with
        | Some streamToken ->
            let! streamEvents = session.Events.FetchStreamAsync(streamToken) |> Async.AwaitTask
            let latestEmailStatus =
                streamEvents
                |> Seq.choose (fun e ->
                    match e.Data with
                    | :? FormEventCase as eventCase ->
                        match eventCase with
                        | EmailStatusAppended { EventToken = et; EmailAddress = email; Status = status } when et = eventToken-> Some (streamToken, email, status)
                        | _ -> None
                    | _ -> None)
                |> Seq.tryHead

            match latestEmailStatus with
            | Some (streamToken, email, status) -> return (streamToken, email, status)
            | None -> return failwith "No available state for the given token. Verification may already be complete."
        | None -> return failwith "No match found for the given token."
    }

let processEmailVerificationCompletion (timeStamp: EventDateTime, streamToken: StreamToken, eventToken: EventToken, emailAddress: EmailAddress, status: EmailStatus) = async {
    Database.eventProcessor.Post(ProcessEmailStatus (timeStamp, streamToken, eventToken, emailAddress, status))
    }

let getUnsubscribeStatus (eventToken: EventToken): Async<StreamToken * EmailAddress * SubscribeStatus> =
    async {
        use session = Database.store.LightweightSession()
        let allEvents = session.Events.QueryAllRawEvents()
        let streamTokenOption =
            allEvents
            |> Seq.tryPick (fun e ->
                match e.Data with
                | :? FormEventCase as eventCase ->
                    match eventCase with
                    | EmailStatusAppended { EventToken = et; EmailAddress = _; Status = _ } when et = eventToken -> Some e.StreamId
                    | _ -> None
                | _ -> None)

        match streamTokenOption with
        | Some streamToken ->
            let! streamEvents = session.Events.FetchStreamAsync(streamToken) |> Async.AwaitTask
            let latestEmailStatus =
                streamEvents
                |> Seq.choose (fun e ->
                    match e.Data with
                    | :? FormEventCase as eventCase ->
                        match eventCase with
                        | SubscribeStatusAppended { EventToken = et; EmailAddress = email; Status = status } when et = eventToken-> Some (streamToken, email, status)
                        | _ -> None
                    | _ -> None)
                |> Seq.tryHead

            match latestEmailStatus with
            | Some (streamToken, email, status) -> return (streamToken, email, status)
            | None -> return failwith "No available state for the given token. Verification may already be complete."
        | None -> return failwith "No match found for the given token."
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
        let tokenParam : EventToken = ctx.Request.Query["token"] |> string |> Guid.Parse
        let! streamToken, email, emailStatus = getEmailStatus tokenParam
        if emailStatus = EmailStatus.Open then
            let timeStamp = DateTime.Now
            let status = EmailStatus.Verified
            processEmailVerificationCompletion (timeStamp, streamToken, tokenParam, email, status ) |> ignore
            ctx.Response.StatusCode <- 200
            return! text $"Email {email} verified with token: {tokenParam}" next ctx
        else
            return! ServerError.failwith (ServerError.Exception "Token not available for verification.") next ctx
    }

let unsubscribeHandler (next: HttpFunc) (ctx: HttpContext) =
    task {
        let unsubscribeToken : EventToken = ctx.Request.Query["token"] |> string |> Guid.Parse
        let! streamToken, email, subscribeStatus = getUnsubscribeStatus unsubscribeToken
        if subscribeStatus = SubscribeStatus.Open then
            let newStatus = SubscribeStatus.Unsubscribed
            let timeStamp = DateTime.Now
            ProcessUnsubscribeStatus (timeStamp, streamToken, unsubscribeToken,  email, newStatus) |> ignore
            ctx.Response.StatusCode <- 200
            return! text $"You have been unsubscribed from marketing mails - email: {email} token: {unsubscribeToken}. Be advised if you use another email for this site you will still verify and manage that email separately." next ctx
        else
            return! ServerError.failwith (ServerError.Exception "Token not available for verification. Email may already be unsubscribed.") next ctx
    }

let notificationManager : HttpHandler  =
    choose [
        route "/api/prefs/confirm" >=> authenticateApiKey >=> confirmEmailHandler
        route "/api/prefs/unsubscribe" >=> authenticateApiKey >=> unsubscribeHandler
    ]