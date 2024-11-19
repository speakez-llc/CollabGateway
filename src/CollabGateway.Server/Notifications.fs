module CollabGateway.Server.Notifications

open System
open CollabGateway.Server.Aggregates
open CollabGateway.Shared.API
open CollabGateway.Shared.Events
open Giraffe
open Microsoft.AspNetCore.Http

let private notificationKey =
    let value = Environment.GetEnvironmentVariable("NOTIFICATION_KEY")
    printfn $"Notifications: Raw NOTIFICATION_KEY environment variable: %s{value}"
    match value with
    | null | "" -> failwith "NOTIFICATION_KEY environment variable is not set."
    | value -> value

let private webServerName =
    let value = Environment.GetEnvironmentVariable("VITE_WEB_URL")
    printfn $"Notifications: Raw VITE_WEB_URL environment variable: %s{value}"
    match value with
    | null | "" -> failwith "VITE_WEB_URL environment variable is not set."
    | value -> value

let getEmailStatus (verificationToken: VerificationToken): Async<StreamToken * EmailAddress * EmailStatus * VerificationToken> =
    async {
        try
            use session = Database.store.LightweightSession()
            let allEvents =
                session.Events.QueryAllRawEvents()
                |> Seq.map (fun e -> { StreamId = e.StreamId; Timestamp = e.Timestamp; Data = e.Data })
                |> Seq.toList

            let rec findLatestEmailStatus (events: EventEnvelope list) =
                match events with
                | [] -> None
                | e :: rest ->
                    match e.Data with
                    | :? FormEventCase as eventCase ->
                        match eventCase with
                        | EmailStatusAppended { VerificationToken = vt; EmailAddress = email; Status = status } when vt = verificationToken ->
                            Some (e.StreamId, email, status, vt)
                        | _ -> findLatestEmailStatus rest
                    | _ -> findLatestEmailStatus rest

            let sortedEvents = allEvents |> List.sortByDescending (_.Timestamp)
            match findLatestEmailStatus sortedEvents with
            | Some (streamToken, email, status, verificationToken) -> return (streamToken, email, status, verificationToken)
            | None -> return failwith "No available state for the given token. Verification may already be complete."
        with
        | ex ->
            Console.WriteLine $"Exception in getEmailStatus: {ex.Message}"
            return failwith "An error occurred while retrieving email status."
    }

let getUnsubscribeStatus (subToken: SubscriptionToken): Async<StreamToken * EmailAddress * SubscribeStatus> =
    async {
        use session = Database.store.LightweightSession()
        let allEvents =
            session.Events.QueryAllRawEvents()
            |> Seq.map (fun e -> { StreamId = e.StreamId; Timestamp = e.Timestamp; Data = e.Data })
            |> Seq.toList

        let rec findLatestSubscribeStatus (events: EventEnvelope list) =
            match events with
            | [] -> None
            | e :: rest ->
                match e.Data with
                | :? FormEventCase as eventCase ->
                    match eventCase with
                    | SubscribeStatusAppended { SubscriptionToken = st; EmailAddress = email; Status = status } when st = subToken ->
                        Some (e.StreamId, email, status)
                    | _ -> findLatestSubscribeStatus rest
                | _ -> findLatestSubscribeStatus rest

        let sortedEvents = allEvents |> List.sortByDescending (_.Timestamp)
        match findLatestSubscribeStatus sortedEvents with
        | Some (streamToken, email, status) -> return (streamToken, email, status)
        | None -> return failwith "No available state for the given token. Verification may already be complete."
    }


let processEmailVerificationCompletion (timeStamp: EventDateTime, streamToken: StreamToken, subToken: SubscriptionToken, emailAddress: EmailAddress, status: EmailStatus) = async {
    try
        Database.eventProcessor.Post(ProcessEmailStatus (timeStamp, streamToken, subToken, emailAddress, status))
        let! nameOption = getUserName streamToken
        let name = nameOption |> Option.defaultValue "User"
        do EmailHelpers.sendEmailConfirmation (name, emailAddress, streamToken, subToken) |> ignore
    with
    | ex ->
        Console.WriteLine $"Exception in processEmailVerificationCompletion: {ex.Message}"
}

let getUnsubscribeToken (streamToken: StreamToken): Async<SubscriptionToken option> =
    async {
        use session = Database.store.LightweightSession()
        let! streamEvents = session.Events.FetchStreamAsync(streamToken) |> Async.AwaitTask
        let unsubscribeToken =
            streamEvents
            |> Seq.choose (fun e ->
                match e.Data with
                | :? FormEventCase as eventCase ->
                    match eventCase with
                    | SubscribeStatusAppended { SubscriptionToken = st } -> Some st
                    | _ -> None
                | _ -> None)
            |> Seq.tryHead
        return unsubscribeToken
    }

let processUnsubscribeCompletion (timeStamp: EventDateTime, streamToken: StreamToken, subToken: SubscriptionToken, emailAddress: EmailAddress, status: SubscribeStatus) = async {
    try
        Database.eventProcessor.Post(ProcessUnsubscribeStatus (timeStamp, streamToken, subToken, emailAddress, status))
        let! nameOption = getUserName streamToken
        let name = nameOption |> Option.defaultValue "Unknown"
        do EmailHelpers.sendUnsubscribeConfirmation (name, emailAddress) |> ignore
    with
    | ex ->
        Console.WriteLine $"Exception in processEmailVerificationCompletion: {ex.Message}"
}

let private authenticateApiKey (next: HttpFunc) (ctx: HttpContext) =
    task {
        let apiKeyParam = ctx.Request.Query["api-key"] |> string
        if apiKeyParam = notificationKey then
            return! next ctx
        else
            ctx.Response.StatusCode <- 401
            return! text "Unauthorized" next ctx
    }

let emailVerificationHandler (next: HttpFunc) (ctx: HttpContext) =
    task {
        let verificationParam : VerificationToken = ctx.Request.Query["token"] |> string |> Guid.Parse
        let! streamToken, email, emailStatus, verificationToken = getEmailStatus verificationParam
        if emailStatus = EmailStatus.Open then
            let timeStamp = DateTime.Now
            let status = EmailStatus.Verified
            do! processEmailVerificationCompletion (timeStamp, streamToken, verificationToken, email, status)
            ctx.Response.StatusCode <- 302
            ctx.Response.Headers["Location"] <- $"{webServerName}/activity?ref={streamToken}"
            return! next ctx
        else
            ctx.Response.StatusCode <- 302
            ctx.Response.Headers["Location"] <- $"{webServerName}/activity?ref={streamToken}"
            return! next ctx
    }

let unsubscribeHandler (next: HttpFunc) (ctx: HttpContext) =
    task {
        let unsubscribeToken : SubscriptionToken = ctx.Request.Query["token"] |> string |> Guid.Parse
        let! streamToken, email, subscribeStatus = getUnsubscribeStatus unsubscribeToken
        if subscribeStatus = SubscribeStatus.Open then
            let newStatus = SubscribeStatus.Unsubscribed
            let timeStamp = DateTime.Now
            do! processUnsubscribeCompletion (timeStamp, streamToken, unsubscribeToken, email, newStatus)
            ctx.Response.StatusCode <- 302
            ctx.Response.Headers["Location"] <- $"{webServerName}/activity?ref={streamToken}"
            return! next ctx
        else
            ctx.Response.StatusCode <- 302
            ctx.Response.Headers["Location"] <- $"{webServerName}/activity?ref={streamToken}"
            return! next ctx
    }

let notificationManager : HttpHandler  =
    choose [
        route "/api/prefs/confirm" >=> emailVerificationHandler
        route "/api/prefs/unsubscribe" >=> unsubscribeHandler
    ]