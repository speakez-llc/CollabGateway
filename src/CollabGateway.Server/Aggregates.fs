module CollabGateway.Server.Aggregates

open System.Threading.Tasks
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
        | DataPolicyPageVisited { TimeStamp = ts } -> ts
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
        | DataPolicyResetButtonClicked { TimeStamp = ts } -> ts
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
        let! allEvents = session.Events.FetchStream(streamToken) |> Task.FromResult |> Async.AwaitTask
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

let getLatestDataPolicyDecision (streamToken: StreamToken): Async<DataPolicyChoice * EventDateTime option> =
    async {
        use session = Database.store.LightweightSession()
        let! allEvents = session.Events.FetchStream(streamToken) |> Task.FromResult |> Async.AwaitTask
        let allDataPolicyEvents =
            allEvents
            |> Seq.choose (fun e ->
                match e.Data with
                | :? DataPolicyEventCase as eventCase -> Some eventCase
                | _ -> None)
            |> Seq.sortByDescending (fun e -> unwrapEventTimeStamp e)

        match Seq.tryHead allDataPolicyEvents with
        | Some eventCase ->
            let choice =
                match eventCase with
                | DataPolicyAccepted _ -> Accepted
                | DataPolicyDeclined _ -> Declined
                | DataPolicyReset _ -> Unknown
            return (choice, Some (unwrapEventTimeStamp eventCase))
        | None -> return (Unknown, None)
    }

let getLatestContactFormSubmitted (streamToken: StreamToken): Async<EventDateTime option> =
    async {
        use session = Database.store.LightweightSession()
        let! allEvents = session.Events.FetchStream(streamToken) |> Task.FromResult |> Async.AwaitTask
        let allFormEvents =
            allEvents
            |> Seq.choose (fun e ->
                match e.Data with
                | :? FormEventCase as eventCase -> Some eventCase
                | _ -> None)
            |> Seq.sortByDescending (fun e -> unwrapEventTimeStamp e)

        match Seq.tryHead allFormEvents with
        | Some eventCase ->
            match eventCase with
            | ContactFormSubmitted _ -> return Some (unwrapEventTimeStamp eventCase)
            | _ -> return None
        | None -> return None
    }

let getLatestContactForm (streamToken: StreamToken): Async<ContactForm option> =
    async {
        use session = Database.store.LightweightSession()
        let! allEvents = session.Events.FetchStream(streamToken) |> Task.FromResult |> Async.AwaitTask
        let allFormEvents =
            allEvents
            |> Seq.choose (fun e ->
                match e.Data with
                | :? FormEventCase as eventCase -> Some eventCase
                | _ -> None)
            |> Seq.sortByDescending (fun e -> unwrapEventTimeStamp e)

        match Seq.tryHead allFormEvents with
        | Some eventCase ->
            match eventCase with
            | ContactFormSubmitted { Form = form } -> return Some form
            | _ -> return None
        | None -> return None
    }

let getLatestSignUpFormSubmitted (streamToken: StreamToken): Async<EventDateTime option> =
    async {
        use session = Database.store.LightweightSession()
        let! allEvents = session.Events.FetchStream(streamToken) |> Task.FromResult |> Async.AwaitTask
        let allFormEvents =
            allEvents
            |> Seq.choose (fun e ->
                match e.Data with
                | :? FormEventCase as eventCase -> Some eventCase
                | _ -> None)
            |> Seq.sortByDescending (fun e -> unwrapEventTimeStamp e)

        match Seq.tryHead allFormEvents with
        | Some eventCase ->
            match eventCase with
            | SignUpFormSubmitted _ -> return Some (unwrapEventTimeStamp eventCase)
            | _ -> return None
        | None -> return None
    }

let getLatestSignUpForm (streamToken: StreamToken): Async<SignUpForm option> =
    async {
        use session = Database.store.LightweightSession()
        let! allEvents = session.Events.FetchStream(streamToken) |> Task.FromResult |> Async.AwaitTask
        let allFormEvents =
            allEvents
            |> Seq.choose (fun e ->
                match e.Data with
                | :? FormEventCase as eventCase -> Some eventCase
                | _ -> None)
            |> Seq.sortByDescending (fun e -> unwrapEventTimeStamp e)

        match Seq.tryHead allFormEvents with
        | Some eventCase ->
            match eventCase with
            | SignUpFormSubmitted { Form = form } -> return Some form
            | _ -> return None
        | None -> return None
    }

let getUserNameAndEmail (streamToken: StreamToken): Async<string option * string option> =
    async {
        use session = Database.store.LightweightSession()
        let! allEvents = session.Events.FetchStream(streamToken) |> Task.FromResult |> Async.AwaitTask
        let allFormEvents =
            allEvents
            |> Seq.choose (fun e ->
                match e.Data with
                | :? FormEventCase as eventCase -> Some eventCase
                | _ -> None)
            |> Seq.sortByDescending (fun e -> unwrapEventTimeStamp e)

        match Seq.tryHead allFormEvents with
        | Some eventCase ->
            match eventCase with
            | ContactFormSubmitted { Form = { Name = name; Email = email } }
            | SignUpFormSubmitted { Form = { Name = name; Email = email } } -> return (Some name, Some email)
            | _ -> return (None, None)
        | None -> return (None, None)
    }



let retrieveUserSummaryAggregate (streamToken: StreamToken): Async<UserSummaryAggregate> =
    async {
        let! dateInitiated = getDateInitiated streamToken
        let! userName, userEmail = getUserNameAndEmail streamToken
        let! dataPolicyDecision = getLatestDataPolicyDecision streamToken
        let! contactFormSubmitted = getLatestContactFormSubmitted streamToken
        let! contactForm = getLatestContactForm streamToken
        let! signUpFormSubmitted = getLatestSignUpFormSubmitted streamToken
        let! signUpForm = getLatestSignUpForm streamToken
        return {
            UserName = userName
            UserEmail = userEmail
            StreamInitiated = dateInitiated
            DataPolicyDecision = dataPolicyDecision
            ContactFormSubmitted = contactFormSubmitted
            ContactForm = contactForm
            SignUpFormSubmitted = signUpFormSubmitted
            SignUpForm = signUpForm
        }
    }