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
        | SummaryActivityPageVisited { TimeStamp = ts } -> ts
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
        | SummaryActivityButtonClicked { TimeStamp = ts } -> ts
    | :? FormEventCase as e ->
        match e with
        | ContactFormSubmitted { TimeStamp = ts }
        | SignUpFormSubmitted { TimeStamp = ts }
        | SmartFormSubmitted { TimeStamp = ts }
        | SmartFormResultReturned { TimeStamp = ts } -> ts
    | :? ClientIPEventCase as e ->
        match e with
        | UserClientIPDetected { TimeStamp = ts }
        | UserClientIPUpdated { TimeStamp = ts } -> ts
    | _ -> failwith "Unknown event case type"

let getDateInitiated (streamToken: StreamToken): Async<EventDateTime> =
    async {
        use session = Database.store.LightweightSession()
        let! allEvents = session.Events.FetchStreamAsync(streamToken) |> Async.AwaitTask
        let userStreamInitiatedEvent =
            allEvents
            |> Seq.pick (fun e ->
                match e.Data with
                | :? StreamEventCase as eventCase ->
                    match eventCase with
                    | UserStreamInitiated { TimeStamp = ts } -> Some ts
                    | _ -> None
                | _ -> None)
        return userStreamInitiatedEvent
    }

let getLatestDataPolicyDecision (streamToken: StreamToken): Async<(DataPolicyChoice * EventDateTime) option> =
    async {
        use session = Database.store.LightweightSession()
        let! allEvents = session.Events.FetchStreamAsync(streamToken) |> Async.AwaitTask
        let allDataPolicyEvents =
            allEvents
            |> Seq.choose (fun e ->
                match e.Data with
                | :? ButtonEventCase as eventCase -> Some eventCase
                | _ -> None)
            |> Seq.sortByDescending (fun e -> unwrapEventTimeStamp e)

        let rec findDecision events =
            match Seq.tryHead events with
            | Some eventCase ->
                let choice =
                    match eventCase with
                    | DataPolicyAcceptButtonClicked _ -> Some Accepted
                    | DataPolicyDeclineButtonClicked _ -> Some Declined
                    | _ -> None
                match choice with
                | Some c -> Some (c, unwrapEventTimeStamp eventCase)
                | None -> findDecision (Seq.tail events)
            | None -> None

        return findDecision allDataPolicyEvents
    }

let getLatestContactFormSubmitted (streamToken: StreamToken): Async<(ContactForm * EventDateTime) option> =
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
                | ContactFormSubmitted { Form = form } -> Some (form, unwrapEventTimeStamp eventCase)
                | _ -> findLatestForm (Seq.tail events)
            | None -> None

        return findLatestForm allFormEvents
    }

let getLatestSignUpFormSubmitted (streamToken: StreamToken): Async<(SignUpForm * EventDateTime) option> =
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
                | SignUpFormSubmitted { Form = form } -> Some (form, unwrapEventTimeStamp eventCase)
                | _ -> findLatestForm (Seq.tail events)
            | None -> None

        return findLatestForm allFormEvents
    }

let retrieveUserSummaryAggregate (streamToken: StreamToken): Async<UserSummaryAggregate> =
    async {
        let! dateInitiated = getDateInitiated streamToken
        let! dataPolicyDecision = getLatestDataPolicyDecision streamToken
        let! contactFormSubmitted = getLatestContactFormSubmitted streamToken
        let! signUpFormSubmitted = getLatestSignUpFormSubmitted streamToken
        return {
            StreamInitiated = dateInitiated
            DataPolicyDecision = dataPolicyDecision
            ContactFormSubmitted = contactFormSubmitted
            SignUpFormSubmitted = signUpFormSubmitted
        }
    }