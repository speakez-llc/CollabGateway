module CollabGateway.Server.Aggregates

open CollabGateway.Shared.API
open CollabGateway.Shared.Events

let unwrapEventTimeStamp (eventCase: obj) : EventDateTime =
    match eventCase with
    | :? StreamEventCase as e ->
        match e with
        | UserStreamInitiated { TimeStamp = ts }
        | UserStreamResumed { TimeStamp = ts }
        | UserStreamClosed { TimeStamp = ts }
        | UserStreamEnded { TimeStamp = ts } -> ts
    | :? DataPolicyEventCase as e ->
        match e with
        | DataPolicyAccepted { TimeStamp = ts }
        | DataPolicyDeclined { TimeStamp = ts }
        | DataPolicyReset { TimeStamp = ts } -> ts
    | :? PageEventCase as e ->
        match e with
        | HomePageVisited { TimeStamp = ts }
        | ProjectPageVisited { TimeStamp = ts }
        | DataPageVisited { TimeStamp = ts }
        | SignupPageVisited { TimeStamp = ts }
        | RowerPageVisited { TimeStamp = ts }
        | SpeakEZPageVisited { TimeStamp = ts }
        | ContactPageVisited { TimeStamp = ts }
        | PartnersPageVisited { TimeStamp = ts }
        | DataPolicyPageVisited { TimeStamp = ts }
        | SummaryActivityPageVisited { TimeStamp = ts }
        | OverviewPageVisited { TimeStamp = ts }
        | UserSummaryPageVisited { TimeStamp = ts } -> ts
    | :? ButtonEventCase as e ->
        match e with
        | HomeButtonClicked { TimeStamp = ts }
        | HomeProjectButtonClicked { TimeStamp = ts }
        | HomeSignUpButtonClicked { TimeStamp = ts }
        | ProjectButtonClicked { TimeStamp = ts }
        | ProjectDataButtonClicked { TimeStamp = ts }
        | ProjectSignUpButtonClicked { TimeStamp = ts }
        | DataButtonClicked { TimeStamp = ts }
        | DataSignUpButtonClicked { TimeStamp = ts }
        | SignUpButtonClicked { TimeStamp = ts }
        | SmartFormButtonClicked { TimeStamp = ts }
        | SmartFormSubmittedButtonClicked { TimeStamp = ts }
        | RowerButtonClicked { TimeStamp = ts }
        | RowerSignUpButtonClicked { TimeStamp = ts }
        | SpeakEZButtonClicked { TimeStamp = ts }
        | SpeakEZSignUpButtonClicked { TimeStamp = ts }
        | ContactButtonClicked { TimeStamp = ts }
        | PartnersButtonClicked { TimeStamp = ts }
        | RowerSiteButtonClicked { TimeStamp = ts }
        | CuratorSiteButtonClicked { TimeStamp = ts }
        | TableauSiteButtonClicked { TimeStamp = ts }
        | PowerBISiteButtonClicked { TimeStamp = ts }
        | ThoughtSpotSiteButtonClicked { TimeStamp = ts }
        | SpeakEZSiteButtonClicked { TimeStamp = ts }
        | DataPolicyAcceptButtonClicked { TimeStamp = ts }
        | DataPolicyDeclineButtonClicked { TimeStamp = ts }
        | DataPolicyResetButtonClicked { TimeStamp = ts }
        | SummaryActivityButtonClicked { TimeStamp = ts }
        | ContactActivityButtonClicked { TimeStamp = ts }
        | SignUpActivityButtonClicked { TimeStamp = ts }
        | OverviewButtonClicked { TimeStamp = ts }
        | UserSummaryButtonClicked { TimeStamp = ts } -> ts
    | :? FormEventCase as e ->
        match e with
        | ContactFormSubmitted { TimeStamp = ts }
        | SignUpFormSubmitted { TimeStamp = ts }
        | SmartFormSubmitted { TimeStamp = ts }
        | SmartFormResultReturned { TimeStamp = ts }
        | EmailStatusAppended { TimeStamp = ts }
        | SubscribeStatusAppended { TimeStamp = ts } -> ts
    | :? ClientIPEventCase as e ->
        match e with
        | UserClientIPDetected { TimeStamp = ts }
        | UserClientIPUpdated { TimeStamp = ts } -> ts
    | _ -> failwith "Unknown event type"

let getDateInitiated (streamToken: StreamToken): Async<EventDateTime> =
    async {
        use session = Database.store.LightweightSession()
        let! allEvents = session.Events.FetchStreamAsync(streamToken) |> Async.AwaitTask
        let dateInitiatedOption =
            allEvents
            |> Seq.tryPick (fun e ->
                match e.Data with
                | :? StreamEventCase as eventCase ->
                    match eventCase with
                    | UserStreamInitiated { TimeStamp = timeStamp } -> Some timeStamp
                    | _ -> None
                | _ -> None)

        match dateInitiatedOption with
        | Some dateInitiated -> return dateInitiated
        | None -> return failwith "No UserStreamInitiated event found for the given StreamToken."
    }

let retrieveDataPolicyChoice (streamToken: StreamToken) = async {
    use session = Database.store.LightweightSession()
    let! allEvents = session.Events.FetchStreamAsync(streamToken) |> Async.AwaitTask
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

let retrieveSmartFormSubmittedCount (streamToken: StreamToken) = async {
    use session = Database.store.LightweightSession()
    let! allEvents = session.Events.FetchStreamAsync(streamToken) |> Async.AwaitTask
    let smartFormSubmittedEvents =
        allEvents
        |> Seq.choose (fun e ->
            match e.Data with
            | :? FormEventCase as eventCase ->
                match eventCase with
                | SmartFormSubmitted _ -> Some eventCase
                | _ -> None
            | _ -> None)
        |> Seq.length

    return smartFormSubmittedEvents
}

let retrieveEmailStatus (streamToken: StreamToken): Async<(EventDateTime * EmailAddress * EmailStatus) list option> =
    async {
        use session = Database.store.LightweightSession()
        let! allEvents = session.Events.FetchStreamAsync(streamToken) |> Async.AwaitTask
        let emailStatusEvents =
            allEvents
            |> Seq.choose (fun e ->
                match e.Data with
                | :? FormEventCase as eventCase ->
                    match eventCase with
                    | EmailStatusAppended { TimeStamp = ts; EmailAddress = ea; Status = status} -> Some (ts, ea, status)
                    | _ -> None
                | _ -> None)
            |> Seq.toList

        return if List.isEmpty emailStatusEvents then None else Some emailStatusEvents
    }

let retrieveSubscribeStatus (streamToken: StreamToken): Async<(EventDateTime * EmailAddress * SubscribeStatus) list option> =
    async {
        use session = Database.store.LightweightSession()
        let! allEvents = session.Events.FetchStreamAsync(streamToken) |> Async.AwaitTask
        let unsubscribeStatusEvents =
            allEvents
            |> Seq.choose (fun e ->
                match e.Data with
                | :? FormEventCase as eventCase ->
                    match eventCase with
                    | SubscribeStatusAppended { TimeStamp = ts; EmailAddress = ea; Status = status} -> Some (ts, ea, status)
                    | _ -> None
                | _ -> None)
            |> Seq.groupBy (fun (_, ea, _) -> ea)
            |> Seq.map (fun (_, events) ->
                let latestEvent = events |> Seq.maxBy (fun (timestamp, _, _) -> timestamp)
                match latestEvent with
                | timestamp, ea, status -> (timestamp, ea, status))
            |> Seq.toList

        return if List.isEmpty unsubscribeStatusEvents then None else Some unsubscribeStatusEvents
    }

let retrieveContactFormSubmitted (streamToken: StreamToken): Async<bool> =
    async {
        use session = Database.store.LightweightSession()
        let! allEvents = session.Events.FetchStreamAsync(streamToken) |> Async.AwaitTask
        let contactFormSubmittedEvents =
            allEvents
            |> Seq.exists (fun e ->
                match e.Data with
                | :? FormEventCase as eventCase ->
                    match eventCase with
                    | ContactFormSubmitted _ -> true
                    | _ -> false
                | _ -> false)

        return contactFormSubmittedEvents
    }

let retrieveSignUpFormSubmitted (streamToken: StreamToken): Async<bool> =
    async {
        use session = Database.store.LightweightSession()
        let! allEvents = session.Events.FetchStreamAsync(streamToken) |> Async.AwaitTask
        let contactFormSubmittedEvents =
            allEvents
            |> Seq.exists (fun e ->
                match e.Data with
                | :? FormEventCase as eventCase ->
                    match eventCase with
                    | SignUpFormSubmitted _ -> true
                    | _ -> false
                | _ -> false)

        return contactFormSubmittedEvents
    }

let getLatestDataPolicyDecision (streamToken: StreamToken): Async<(EventDateTime * DataPolicyChoice) option> =
    async {
        use session = Database.store.LightweightSession()
        let! allEvents = session.Events.FetchStreamAsync(streamToken) |> Async.AwaitTask
        let allDataPolicyEvents =
            allEvents
            |> Seq.choose (fun e ->
                match e.Data with
                | :? ButtonEventCase as eventCase -> Some eventCase
                | _ -> None)
            |> Seq.sortByDescending unwrapEventTimeStamp

        let rec findDecision events =
            match Seq.tryHead events with
            | Some eventCase ->
                let choice =
                    match eventCase with
                    | DataPolicyAcceptButtonClicked _ -> Some Accepted
                    | DataPolicyDeclineButtonClicked _ -> Some Declined
                    | _ -> None
                match choice with
                | Some c -> Some (unwrapEventTimeStamp eventCase, c)
                | None -> findDecision (Seq.tail events)
            | None -> None

        return findDecision allDataPolicyEvents
    }

let getLatestContactFormSubmitted (streamToken: StreamToken): Async<(EventDateTime * ContactForm) option> =
    async {
        use session = Database.store.LightweightSession()
        let! allEvents = session.Events.FetchStreamAsync(streamToken) |> Async.AwaitTask
        let allFormEvents =
            allEvents
            |> Seq.choose (fun e ->
                match e.Data with
                | :? FormEventCase as eventCase -> Some eventCase
                | _ -> None)
            |> Seq.sortByDescending (fun e -> unwrapEventTimeStamp e)

        let rec findLatestForm events =
            match Seq.tryHead events with
            | Some eventCase ->
                match eventCase with
                | ContactFormSubmitted { Form = form } -> Some (unwrapEventTimeStamp eventCase, form)
                | _ -> findLatestForm (Seq.tail events)
            | None -> None

        return findLatestForm allFormEvents
    }

let getLatestSignUpFormSubmitted (streamToken: StreamToken): Async<(EventDateTime * SignUpForm) option> =
    async {
        use session = Database.store.LightweightSession()
        let! allEvents = session.Events.FetchStreamAsync(streamToken) |> Async.AwaitTask
        let allFormEvents =
            allEvents
            |> Seq.choose (fun e ->
                match e.Data with
                | :? FormEventCase as eventCase -> Some eventCase
                | _ -> None)
            |> Seq.sortByDescending (fun e -> unwrapEventTimeStamp e)

        let rec findLatestForm events =
            match Seq.tryHead events with
            | Some eventCase ->
                match eventCase with
                | SignUpFormSubmitted { Form = form } -> Some (unwrapEventTimeStamp eventCase, form)
                | _ -> findLatestForm (Seq.tail events)
            | None -> None

        return findLatestForm allFormEvents
    }

let retrieveUserSummaryAggregate (streamToken: StreamToken): Async<UserSummaryAggregate> =
    async {
        let! results =
            Async.Parallel [
                async { let! x = getDateInitiated streamToken in return x :> obj }
                async { let! x = getLatestDataPolicyDecision streamToken in return x :> obj }
                async { let! x = getLatestContactFormSubmitted streamToken in return x :> obj }
                async { let! x = getLatestSignUpFormSubmitted streamToken in return x :> obj }
                async { let! x = retrieveEmailStatus streamToken in return x :> obj }
                async { let! x = retrieveSubscribeStatus streamToken in return x :> obj }
            ]

        let dateInitiated = results[0] :?> EventDateTime
        let dataPolicyDecision = results[1] :?> (EventDateTime * DataPolicyChoice) option
        let contactFormSubmitted = results[2] :?> (EventDateTime * ContactForm) option
        let signUpFormSubmitted = results[3] :?> (EventDateTime * SignUpForm) option
        let emailStatus = results[4] :?> (EventDateTime * EmailAddress * EmailStatus) list option
        let subscribeStatus = results[5] :?> (EventDateTime * EmailAddress * SubscribeStatus) list option

        return {
            StreamInitiated = dateInitiated
            DataPolicyDecision = dataPolicyDecision
            ContactFormSubmitted = contactFormSubmitted
            SignUpFormSubmitted = signUpFormSubmitted
            EmailStatus = emailStatus
            SubscribeStatus = subscribeStatus
        }
    }