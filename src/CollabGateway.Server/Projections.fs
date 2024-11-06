module CollabGateway.Server.Projections

open System
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
                            | EmailStatusAppended { Status = status } -> ("Email Status Appended", Some (box status))
                            | SubscribeStatusAppended { Status = status } -> ("Subscribe Status Appended", Some (box status))
                        (name, content)
                    | :? ClientIPEventCase as eventCase ->
                        let name, content =
                            match eventCase with
                            | UserClientIPDetected { UserGeoInfo = geoInfo } -> ("User ClientIP Detected", Some (box geoInfo))
                            | UserClientIPUpdated { UserGeoInfo = geoInfo } -> ("User ClientIP Updated", Some (box geoInfo))
                        (name, content)
                    | _ -> ("Unknown Event", None)
                let timeStamp = Aggregates.unwrapEventTimeStamp e.Data
                (eventName, timeStamp, formContent))
            |> Seq.distinctBy (fun (eventName, timeStamp, _) -> (eventName, timeStamp)) // Ensure unique events
            |> Seq.sortBy (fun (_, ts, _) -> ts)
            |> Seq.toList
        let projection = allStreamEvents
        return projection
    }

let retrieveAllUserStreams (): Async<StreamToken list> =
    async {
        use session = Database.store.LightweightSession()
        let! allEvents = session.Events.QueryAllRawEvents() |> Task.FromResult |> Async.AwaitTask
        let streamTokens =
            allEvents
            |> Seq.sortByDescending (_.Timestamp)
            |> Seq.map (fun (e: Marten.Events.IEvent) -> e.StreamId)
            |> Seq.distinct
            |> Seq.toList
        return streamTokens
    }

let extractUserInfo (events: obj seq): string option * string option * int =
    let mutable userName = None
    let mutable email = None
    let eventCount = Seq.length events

    let sortedEvents =
        events
        |> Seq.sortByDescending (fun e ->
            match e with
            | :? Marten.Events.IEvent as event -> event.Timestamp
            | _ -> DateTime.MinValue)

    for event in sortedEvents do
        match event with
        | :? FormEventCase as formEvent ->
            match formEvent with
            | SignUpFormSubmitted { Form = form } ->
                if userName.IsNone then userName <- Some form.Name
                if email.IsNone then email <- Some form.Email
            | ContactFormSubmitted { Form = form } ->
                if userName.IsNone then userName <- Some form.Name
                if email.IsNone then email <- Some form.Email
            | _ -> ()
        | _ -> ()

    userName, email, eventCount

let retrieveUserStreamInfo (streamToken: StreamToken): Async<UserTopLineSummary> =
    async {
        use session = Database.store.LightweightSession()
        let! events = session.Events.FetchStreamAsync(streamToken) |> Async.AwaitTask
        let userName, email, eventCount = extractUserInfo (events |> Seq.map (_.Data))
        return { StreamToken = streamToken; UserName = userName; Email = email; EventCount = eventCount }
    }

let retrieveUserNameProjection (): Async<UserStreamProjection> =
    async {
        let! streamTokens = retrieveAllUserStreams()
        let! userStreamInfos = streamTokens |> List.map retrieveUserStreamInfo |> Async.Parallel
        return userStreamInfos |> List.ofArray
    }

let filterEventsByInterval (interval: (IntervalStart * IntervalEnd) option): Async<seq<Marten.Events.IEvent>> =
    async {
        use session = Database.store.LightweightSession()
        let! allEvents = session.Events.QueryAllRawEvents() |> Task.FromResult |> Async.AwaitTask
        let filteredEvents =
            match interval with
            | Some (start, ``end``) ->
                allEvents |> Seq.filter (fun e -> e.Timestamp > start && e.Timestamp <= ``end``)
            | None -> allEvents
        return filteredEvents
    }

let retrieveTotalUserStreamCount (interval: (IntervalStart * IntervalEnd) option): Async<int> =
    async {
        let! filteredEvents = filterEventsByInterval interval

        let userStreamInitiatedEvents =
            filteredEvents
            |> Seq.filter (fun e ->
                match e.Data with
                | :? StreamEventCase as eventCase ->
                    match eventCase with
                    | UserStreamInitiated _ -> true
                    | _ -> false
                | _ -> false)
            |> Seq.toList

        return List.length userStreamInitiatedEvents
    }


let retrieveTotalDataPolicyDeclined (interval: (IntervalStart * IntervalEnd) option): Async<int> =
    async {
        let! filteredEvents = filterEventsByInterval interval

        let declinedEvents =
            filteredEvents
            |> Seq.choose (fun e ->
                match e.Data with
                | :? ButtonEventCase as eventCase -> Some (e.StreamId, eventCase, e.Timestamp)
                | _ -> None)
            |> Seq.groupBy (fun (streamId, _, _) -> streamId)
            |> Seq.choose (fun (_, events) ->
                events
                |> Seq.sortByDescending (fun (_, _, timestamp) -> timestamp)
                |> Seq.tryHead
                |> function
                    | Some (_, DataPolicyDeclineButtonClicked _, _) -> Some ()
                    | _ -> None)
            |> Seq.length
        return declinedEvents
    }

let retrieveTotalContactFormsSubmitted (interval: (IntervalStart * IntervalEnd) option): Async<int> =
    async {
        let! filteredEvents = filterEventsByInterval interval

        let contactFormEvents =
            filteredEvents
            |> Seq.choose (fun e ->
                match e.Data with
                | :? FormEventCase as eventCase ->
                    match eventCase with
                    | ContactFormSubmitted _ -> Some e.StreamId
                    | _ -> None
                | _ -> None)
            |> Seq.distinct
            |> Seq.toList

        return List.length contactFormEvents
    }

let retrieveTotalSmartFormUsers (interval: (IntervalStart * IntervalEnd) option): Async<int> =
    async {
        let! filteredEvents = filterEventsByInterval interval

        let smartFormEvents =
            filteredEvents
            |> Seq.choose (fun e ->
                match e.Data with
                | :? FormEventCase as eventCase ->
                    match eventCase with
                    | SmartFormSubmitted _ -> Some e.StreamId
                    | _ -> None
                | _ -> None)
            |> Seq.distinct
            |> Seq.toList

        return List.length smartFormEvents
    }

let retrieveTotalSignUpFormsSubmitted (interval: (IntervalStart * IntervalEnd) option): Async<int> =
    async {
        let! filteredEvents = filterEventsByInterval interval

        let signUpFormEvents =
            filteredEvents
            |> Seq.choose (fun e ->
                match e.Data with
                | :? FormEventCase as eventCase ->
                    match eventCase with
                    | SignUpFormSubmitted _ -> Some e.StreamId
                    | _ -> None
                | _ -> None)
            |> Seq.distinct
            |> Seq.toList

        return List.length signUpFormEvents
    }

let retrieveTotalEmailVerifications (interval: (IntervalStart * IntervalEnd) option): Async<int> =
    async {
        let! filteredEvents = filterEventsByInterval interval

        let emailStatusEvents =
            filteredEvents
            |> Seq.filter (fun e ->
                match e.Data with
                | :? FormEventCase as eventCase ->
                    match eventCase with
                    | EmailStatusAppended { Status = Verified } -> true
                    | _ -> false
                | _ -> false)
            |> Seq.toList
        return List.length emailStatusEvents
    }

let retrieveTotalEmailUnsubscribes (interval: (IntervalStart * IntervalEnd) option): Async<int> =
    async {
        let! filteredEvents = filterEventsByInterval interval

        let unsubscribeStatusEvents =
            filteredEvents
            |> Seq.filter (fun e ->
                match e.Data with
                | :? FormEventCase as eventCase ->
                    match eventCase with
                    | SubscribeStatusAppended { Status = Unsubscribed } -> true
                    | _ -> false
                | _ -> false)
            |> Seq.toList
        return List.length unsubscribeStatusEvents
    }

let retrieveUniqueClientIPDomains (): Async<(string * int) list> =
    async {
        use session = Database.store.LightweightSession()
        let! allEvents = session.Events.QueryAllRawEvents() |> Task.FromResult |> Async.AwaitTask
        let domainCounts =
            allEvents
            |> Seq.choose (fun e ->
                match e.Data with
                | :? ClientIPEventCase as eventCase ->
                    match eventCase with
                    | UserClientIPUpdated { UserGeoInfo = geoInfo } -> Some geoInfo.``as``.domain
                    | UserClientIPDetected { UserGeoInfo = geoInfo } -> Some geoInfo.``as``.domain
                | _ -> None)
            |> Seq.groupBy id
            |> Seq.map (fun (domain, occurrences) -> (domain, Seq.length occurrences))
            |> Seq.toList
        return domainCounts
    }

let retrieveClientIPLocations (): Async<(string * float * float * int) list> =
    async {
        use session = Database.store.LightweightSession()
        let! allEvents = session.Events.QueryAllRawEvents() |> Task.FromResult |> Async.AwaitTask
        let cityLatLons =
            allEvents
            |> Seq.choose (fun e ->
                match e.Data with
                | :? ClientIPEventCase as eventCase ->
                    match eventCase with
                    | UserClientIPUpdated { UserGeoInfo = geoInfo } -> Some (geoInfo.location.city, geoInfo.location.lat, geoInfo.location.lng)
                    | UserClientIPDetected { UserGeoInfo = geoInfo } -> Some (geoInfo.location.city, geoInfo.location.lat, geoInfo.location.lng)
                | _ -> None)
            |> Seq.groupBy id
            |> Seq.map (fun ((city, lat, lng), occurrences) -> (city, lat, lng, Seq.length occurrences))
            |> Seq.toList
        return cityLatLons
    }

let retrieveVerifiedEmailDomains (): Async<(string * int) list> =
    async {
        use session = Database.store.LightweightSession()
        let! allEvents = session.Events.QueryAllRawEvents() |> Task.FromResult |> Async.AwaitTask
        let emailDomainsWithCounts =
            allEvents
            |> Seq.choose (fun e ->
                match e.Data with
                | :? FormEventCase as eventCase ->
                    match eventCase with
                    | EmailStatusAppended { Status = Verified; EmailAddress = email } ->
                        let domain = email.Split('@').[1]
                        Some domain
                    | _ -> None
                | _ -> None)
            |> Seq.groupBy id
            |> Seq.map (fun (domain, occurrences) -> (domain, Seq.length occurrences))
            |> Seq.toList
        return emailDomainsWithCounts
    }

let retrieveOverviewStats (interval: (IntervalStart * IntervalEnd) option): Async<OverviewTotals> =
    async {
        let! totalUserStreams = retrieveTotalUserStreamCount interval
        let! totalDataPolicyDeclined = retrieveTotalDataPolicyDeclined interval
        let! totalContactFormsSubmitted = retrieveTotalContactFormsSubmitted interval
        let! totalSmartFormUsers = retrieveTotalSmartFormUsers interval
        let! totalSignUpFormsSubmitted = retrieveTotalSignUpFormsSubmitted interval
        let! totalEmailVerifications = retrieveTotalEmailVerifications interval
        let! totalEmailUnsubscribes = retrieveTotalEmailUnsubscribes interval
        return {
            TotalUserStreams = totalUserStreams
            TotalDataPolicyDeclines = totalDataPolicyDeclined
            TotalContactFormsUsed = totalContactFormsSubmitted
            TotalSmartFormUsers = totalSmartFormUsers
            TotalSignUpFormsUsed = totalSignUpFormsSubmitted
            TotalEmailVerifications = totalEmailVerifications
            TotalEmailUnsubscribes = totalEmailUnsubscribes
        }
    }

let retrieveOverviewTotals (intGrain: (int * Grain) option): Async<OverviewTotalsProjection list> =
    async {
        match intGrain with
        | None ->
            let! totals = retrieveOverviewStats None
            return [{ IntervalStart = None; IntervalEnd = None; OverviewTotals = totals }]
        | Some (count, grain) ->
            let now = DateTime.UtcNow
            let intervalLength =
                match grain with
                | Grain.Hour -> TimeSpan.FromHours(1.0)
                | Grain.Day -> TimeSpan.FromDays(1.0)
                | Grain.Week -> TimeSpan.FromDays(7.0)
                | Grain.Month -> TimeSpan.FromDays(30.0)

            let intervals =
                [for i in 0 .. count - 1 do
                    let endInterval = now - TimeSpan.FromTicks(intervalLength.Ticks * int64 i)
                    let startInterval = endInterval - intervalLength
                    yield (startInterval, endInterval)]
                |> List.sortBy fst

            let! projections =
                intervals
                |> List.map (fun (start, ``end``) ->
                    async {
                        let! totals = retrieveOverviewStats (Some (start, ``end``))
                        return { IntervalStart = Some start; IntervalEnd = Some ``end``; OverviewTotals = totals }
                    })
                |> Async.Parallel

            return projections |> List.ofArray
    }