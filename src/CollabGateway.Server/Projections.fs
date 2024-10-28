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
                            | UserStreamInitiated _ -> "User Stream Initiated"
                            | UserStreamResumed _ -> "User Stream Resumed"
                            | UserStreamClosed _ -> "User Stream Closed"
                            | UserStreamEnded _ -> "User Stream Ended"
                        (name, None)
                    | :? DataPolicyEventCase as eventCase ->
                        let name =
                            match eventCase with
                            | DataPolicyAccepted _ -> "Data Policy Accepted"
                            | DataPolicyDeclined _ -> "Data Policy Declined"
                            | DataPolicyReset _ -> "Data Policy Reset"
                        (name, None)
                    | :? PageEventCase as eventCase ->
                        let name =
                            match eventCase with
                            | HomePageVisited _ -> "Home Page Visited"
                            | ProjectPageVisited _ -> "Project Page Visited"
                            | DataPageVisited _ -> "Data Page Visited"
                            | SignupPageVisited _ -> "Signup Page Visited"
                            | RowerPageVisited _ -> "Rower Page Visited"
                            | SpeakEZPageVisited _ -> "SpeakEZ Page Visited"
                            | ContactPageVisited _ -> "Contact Page Visited"
                            | PartnersPageVisited _ -> "Partners Page Visited"
                            | DataPolicyPageVisited _ -> "Data Policy Page Visited"
                            | SummaryActivityPageVisited _ -> "Summary Activity Page Visited"
                        (name, None)
                    | :? ButtonEventCase as eventCase ->
                        let name =
                            match eventCase with
                            | HomeButtonClicked _ -> "Home Button Clicked"
                            | HomeProjectButtonClicked _ -> "Home Project Button Clicked"
                            | HomeSignUpButtonClicked _ -> "Home SignUp Button Clicked"
                            | ProjectButtonClicked _ -> "Project Button Clicked"
                            | ProjectDataButtonClicked _ -> "Project Data Button Clicked"
                            | ProjectSignUpButtonClicked _ -> "Project SignUp Button Clicked"
                            | DataButtonClicked _ -> "Data Button Clicked"
                            | DataSignUpButtonClicked _ -> "Data SignUp Button Clicked"
                            | SignUpButtonClicked _ -> "SignUp Button Clicked"
                            | SmartFormButtonClicked _ -> "Smart Form Button Clicked"
                            | SmartFormSubmittedButtonClicked _ -> "Smart Form Submitted Button Clicked"
                            | RowerButtonClicked _ -> "Rower Button Clicked"
                            | RowerSignUpButtonClicked _ -> "Rower SignUp Button Clicked"
                            | SpeakEZButtonClicked _ -> "SpeakEZ Button Clicked"
                            | SpeakEZSignUpButtonClicked _ -> "SpeakEZ SignUp Button Clicked"
                            | ContactButtonClicked _ -> "Contact Button Clicked"
                            | PartnersButtonClicked _ -> "Partners Button Clicked"
                            | RowerSiteButtonClicked _ -> "Rower Site Button Clicked"
                            | CuratorSiteButtonClicked _ -> "Curator Site Button Clicked"
                            | TableauSiteButtonClicked _ -> "Tableau Site Button Clicked"
                            | PowerBISiteButtonClicked _ -> "PowerBI Site Button Clicked"
                            | ThoughtSpotSiteButtonClicked _ -> "ThoughtSpot Site Button Clicked"
                            | SpeakEZSiteButtonClicked _ -> "SpeakEZ Site Button Clicked"
                            | DataPolicyAcceptButtonClicked _ -> "Data Policy Accept Button Clicked"
                            | DataPolicyDeclineButtonClicked _ -> "Data Policy Decline Button Clicked"
                            | DataPolicyResetButtonClicked _ -> "Data Policy Reset Button Clicked"
                            | SummaryActivityButtonClicked _ -> "Summary Activity Button Clicked"
                        (name, None)
                    | :? FormEventCase as eventCase ->
                        let name, content =
                            match eventCase with
                            | ContactFormSubmitted { Form = form } -> ("Contact Form Submitted", Some (box form))
                            | SignUpFormSubmitted { Form = form } -> ("SignUp Form Submitted", Some (box form))
                            | SmartFormSubmitted { ClipboardInput = input } -> ("Smart Form Submitted", Some (box input))
                            | SmartFormResultReturned { Form = form } -> ("Smart Form Result Returned", Some (box form))
                        (name, content)
                    | :? ClientIPEventCase as eventCase ->
                        let name =
                            match eventCase with
                            | UserClientIPDetected _ -> "User ClientIP Detected"
                            | UserClientIPUpdated _ -> "User ClientIP Updated"
                        (name, None)
                    | _ -> ("Unknown Event", None)
                let timeStamp = Aggregates.unwrapEventTimeStamp e.Data
                (eventName, timeStamp, formContent))
            |> Seq.sortBy (fun (_, ts, _) -> ts)
            |> Seq.toList
        let projection = allStreamEvents
        return projection
    }


let retrieveUserStreamProjection (): Async<UserNameProjection> =
    async {
        use session = Database.store.LightweightSession()
        let! allStreams = session.Events.QueryAllRawEvents() |> Task.FromResult |> Async.AwaitTask

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

        let projection = userNames
        return projection
    }

