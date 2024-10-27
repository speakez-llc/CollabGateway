module CollabGateway.Server.Projections

open System.Threading.Tasks
open CollabGateway.Shared.API
open CollabGateway.Shared.Events


let retrieveFullUserStreamProjection (streamToken: StreamToken): Async<FullUserStreamProjection> =
    async {
        use session = Database.store.LightweightSession()
        let! allEvents = session.Events.FetchStreamAsync(streamToken) |> Async.AwaitTask
        let allStreamEvents =
            allEvents
            |> Seq.map (fun e ->
                let eventName, formContent =
                    match e.Data with
                    | :? StreamEventCase as eventCase ->
                        let name =
                            match eventCase with
                            | UserStreamInitiated _ -> "UserStreamInitiated"
                            | UserStreamResumed _ -> "UserStreamResumed"
                            | UserStreamClosed _ -> "UserStreamClosed"
                            | UserStreamEnded _ -> "UserStreamEnded"
                        (name, None)
                    | :? DataPolicyEventCase as eventCase ->
                        let name =
                            match eventCase with
                            | DataPolicyAccepted _ -> "DataPolicyAccepted"
                            | DataPolicyDeclined _ -> "DataPolicyDeclined"
                            | DataPolicyReset _ -> "DataPolicyReset"
                        (name, None)
                    | :? PageEventCase as eventCase ->
                        let name =
                            match eventCase with
                            | HomePageVisited _ -> "HomePageVisited"
                            | ProjectPageVisited _ -> "ProjectPageVisited"
                            | DataPageVisited _ -> "DataPageVisited"
                            | SignupPageVisited _ -> "SignupPageVisited"
                            | RowerPageVisited _ -> "RowerPageVisited"
                            | SpeakEZPageVisited _ -> "SpeakEZPageVisited"
                            | ContactPageVisited _ -> "ContactPageVisited"
                            | PartnersPageVisited _ -> "PartnersPageVisited"
                            | DataPolicyPageVisited _ -> "DataPolicyPageVisited"
                        (name, None)
                    | :? ButtonEventCase as eventCase ->
                        let name =
                            match eventCase with
                            | HomeButtonClicked _ -> "HomeButtonClicked"
                            | HomeProjectButtonClicked _ -> "HomeProjectButtonClicked"
                            | HomeSignUpButtonClicked _ -> "HomeSignUpButtonClicked"
                            | ProjectButtonClicked _ -> "ProjectButtonClicked"
                            | ProjectDataButtonClicked _ -> "ProjectDataButtonClicked"
                            | ProjectSignUpButtonClicked _ -> "ProjectSignUpButtonClicked"
                            | DataButtonClicked _ -> "DataButtonClicked"
                            | DataSignUpButtonClicked _ -> "DataSignUpButtonClicked"
                            | SignUpButtonClicked _ -> "SignUpButtonClicked"
                            | SmartFormButtonClicked _ -> "SmartFormButtonClicked"
                            | SmartFormSubmittedButtonClicked _ -> "SmartFormSubmittedButtonClicked"
                            | RowerButtonClicked _ -> "RowerButtonClicked"
                            | RowerSignUpButtonClicked _ -> "RowerSignUpButtonClicked"
                            | SpeakEZButtonClicked _ -> "SpeakEZButtonClicked"
                            | SpeakEZSignUpButtonClicked _ -> "SpeakEZSignUpButtonClicked"
                            | ContactButtonClicked _ -> "ContactButtonClicked"
                            | PartnersButtonClicked _ -> "PartnersButtonClicked"
                            | RowerSiteButtonClicked _ -> "RowerSiteButtonClicked"
                            | CuratorSiteButtonClicked _ -> "CuratorSiteButtonClicked"
                            | TableauSiteButtonClicked _ -> "TableauSiteButtonClicked"
                            | PowerBISiteButtonClicked _ -> "PowerBISiteButtonClicked"
                            | ThoughtSpotSiteButtonClicked _ -> "ThoughtSpotSiteButtonClicked"
                            | SpeakEZSiteButtonClicked _ -> "SpeakEZSiteButtonClicked"
                            | DataPolicyAcceptButtonClicked _ -> "DataPolicyAcceptButtonClicked"
                            | DataPolicyDeclineButtonClicked _ -> "DataPolicyDeclineButtonClicked"
                            | DataPolicyResetButtonClicked _ -> "DataPolicyResetButtonClicked"
                        (name, None)
                    | :? FormEventCase as eventCase ->
                        let name, content =
                            match eventCase with
                            | ContactFormSubmitted { Form = form } -> ("ContactFormSubmitted", Some (box form))
                            | SignUpFormSubmitted { Form = form } -> ("SignUpFormSubmitted", Some (box form))
                            | SmartFormSubmitted { ClipboardInput = input } -> ("SmartFormSubmitted", Some (box input))
                            | SmartFormResultReturned { Form = form } -> ("SmartFormResultReturned", Some (box form))
                        (name, content)
                    | :? ClientIPEventCase as eventCase ->
                        let name =
                            match eventCase with
                            | UserClientIPDetected _ -> "UserClientIPDetected"
                            | UserClientIPUpdated _ -> "UserClientIPUpdated"
                        (name, None)
                    | _ -> ("UnknownEvent", None)
                let timeStamp = Aggregates.unwrapEventTimeStamp e.Data
                (eventName, timeStamp, formContent))
            |> Seq.sortBy (fun (_, ts, _) -> ts)
            |> Seq.toList
        let projection = FullUserStreamProjection()
        projection.State <- allStreamEvents
        return projection
    }


let retrieveUserStreamProjection (): Async<UserNameProjection> =
    async {
        use session = Database.store.LightweightSession()
        let! allStreams = session.Events.QueryAllRawEvents() |> Task.FromResult |> Async.AwaitTask

        // Create a list of tuples containing StreamToken and string option
        let userNames =
            allStreams
            |> Seq.map (fun e ->
                let name =
                    match e.Data with
                    | :? FormEventCase as formEvent ->
                        match formEvent with
                        | SignUpFormSubmitted { Form = form } -> Some form.Name
                        | ContactFormSubmitted { Form = form } -> Some form.Name
                        | _ -> None
                    | _ -> None
                (name, e.StreamId))
            |> Seq.toList

        let projection = UserNameProjection()
        projection.State <- userNames
        return projection
    }

