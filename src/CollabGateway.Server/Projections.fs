module CollabGateway.Server.Projections

open System
open System.Threading.Tasks
open CollabGateway.Shared.API
open CollabGateway.Shared.Events

let retrieveFullUserStreamProjection (streamToken: StreamToken): Async<FullUserStreamEvent list> =
    async {
        use session = Database.store.LightweightSession()
        let! allEvents = session.Events.FetchStreamAsync(streamToken) |> Async.AwaitTask
        let allStreamEvents =
            allEvents
            |> Seq.map (fun e ->
                match e.Data with
                | :? StreamEventCase as eventCase ->
                    match eventCase with
                    | UserStreamInitiated _ -> FullUserStreamEvent.UserStreamInitiated (Aggregates.unwrapEventTimeStamp e.Data)
                    | UserStreamResumed _ -> FullUserStreamEvent.UserStreamResumed (Aggregates.unwrapEventTimeStamp e.Data)
                    | UserStreamClosed _ -> FullUserStreamEvent.UserStreamClosed (Aggregates.unwrapEventTimeStamp e.Data)
                    | UserStreamEnded _ -> FullUserStreamEvent.UserStreamEnded (Aggregates.unwrapEventTimeStamp e.Data)
                | :? DataPolicyEventCase as eventCase ->
                    match eventCase with
                    | DataPolicyAccepted _ -> FullUserStreamEvent.DataPolicyAccepted (Aggregates.unwrapEventTimeStamp e.Data)
                    | DataPolicyDeclined _ -> FullUserStreamEvent.DataPolicyDeclined (Aggregates.unwrapEventTimeStamp e.Data)
                    | DataPolicyReset _ -> FullUserStreamEvent.DataPolicyReset (Aggregates.unwrapEventTimeStamp e.Data)
                | :? PageEventCase as eventCase ->
                    match eventCase with
                    | HomePageVisited _ -> FullUserStreamEvent.HomePageVisited (Aggregates.unwrapEventTimeStamp e.Data)
                    | ProjectPageVisited _ -> FullUserStreamEvent.ProjectPageVisited (Aggregates.unwrapEventTimeStamp e.Data)
                    | DataPageVisited _ -> FullUserStreamEvent.DataPageVisited (Aggregates.unwrapEventTimeStamp e.Data)
                    | SignupPageVisited _ -> FullUserStreamEvent.SignupPageVisited (Aggregates.unwrapEventTimeStamp e.Data)
                    | RowerPageVisited _ -> FullUserStreamEvent.RowerPageVisited (Aggregates.unwrapEventTimeStamp e.Data)
                    | SpeakEZPageVisited _ -> FullUserStreamEvent.SpeakEZPageVisited (Aggregates.unwrapEventTimeStamp e.Data)
                    | ContactPageVisited _ -> FullUserStreamEvent.ContactPageVisited (Aggregates.unwrapEventTimeStamp e.Data)
                    | PartnersPageVisited _ -> FullUserStreamEvent.PartnersPageVisited (Aggregates.unwrapEventTimeStamp e.Data)
                    | DataPolicyPageVisited _ -> FullUserStreamEvent.DataPolicyPageVisited (Aggregates.unwrapEventTimeStamp e.Data)
                    | SummaryActivityPageVisited _ -> FullUserStreamEvent.SummaryActivityPageVisited (Aggregates.unwrapEventTimeStamp e.Data)
                | :? ButtonEventCase as eventCase ->
                    match eventCase with
                    | HomeButtonClicked _ -> FullUserStreamEvent.HomeButtonClicked (Aggregates.unwrapEventTimeStamp e.Data)
                    | HomeProjectButtonClicked _ -> FullUserStreamEvent.HomeProjectButtonClicked (Aggregates.unwrapEventTimeStamp e.Data)
                    | HomeSignUpButtonClicked _ -> FullUserStreamEvent.HomeSignUpButtonClicked (Aggregates.unwrapEventTimeStamp e.Data)
                    | ProjectButtonClicked _ -> FullUserStreamEvent.ProjectButtonClicked (Aggregates.unwrapEventTimeStamp e.Data)
                    | ProjectDataButtonClicked _ -> FullUserStreamEvent.ProjectDataButtonClicked (Aggregates.unwrapEventTimeStamp e.Data)
                    | ProjectSignUpButtonClicked _ -> FullUserStreamEvent.ProjectSignUpButtonClicked (Aggregates.unwrapEventTimeStamp e.Data)
                    | DataButtonClicked _ -> FullUserStreamEvent.DataButtonClicked (Aggregates.unwrapEventTimeStamp e.Data)
                    | DataSignUpButtonClicked _ -> FullUserStreamEvent.DataSignUpButtonClicked (Aggregates.unwrapEventTimeStamp e.Data)
                    | SignUpButtonClicked _ -> FullUserStreamEvent.SignUpButtonClicked (Aggregates.unwrapEventTimeStamp e.Data)
                    | SmartFormButtonClicked _ -> FullUserStreamEvent.SmartFormButtonClicked (Aggregates.unwrapEventTimeStamp e.Data)
                    | SmartFormSubmittedButtonClicked _ -> FullUserStreamEvent.SmartFormSubmittedButtonClicked (Aggregates.unwrapEventTimeStamp e.Data)
                    | RowerButtonClicked _ -> FullUserStreamEvent.RowerButtonClicked (Aggregates.unwrapEventTimeStamp e.Data)
                    | RowerSignUpButtonClicked _ -> FullUserStreamEvent.RowerSignUpButtonClicked (Aggregates.unwrapEventTimeStamp e.Data)
                    | SpeakEZButtonClicked _ -> FullUserStreamEvent.SpeakEZButtonClicked (Aggregates.unwrapEventTimeStamp e.Data)
                    | SpeakEZSignUpButtonClicked _ -> FullUserStreamEvent.SpeakEZSignUpButtonClicked (Aggregates.unwrapEventTimeStamp e.Data)
                    | ContactButtonClicked _ -> FullUserStreamEvent.ContactButtonClicked (Aggregates.unwrapEventTimeStamp e.Data)
                    | PartnersButtonClicked _ -> FullUserStreamEvent.PartnersButtonClicked (Aggregates.unwrapEventTimeStamp e.Data)
                    | RowerSiteButtonClicked _ -> FullUserStreamEvent.RowerSiteButtonClicked (Aggregates.unwrapEventTimeStamp e.Data)
                    | CuratorSiteButtonClicked _ -> FullUserStreamEvent.CuratorSiteButtonClicked (Aggregates.unwrapEventTimeStamp e.Data)
                    | TableauSiteButtonClicked _ -> FullUserStreamEvent.TableauSiteButtonClicked (Aggregates.unwrapEventTimeStamp e.Data)
                    | PowerBISiteButtonClicked _ -> FullUserStreamEvent.PowerBISiteButtonClicked (Aggregates.unwrapEventTimeStamp e.Data)
                    | ThoughtSpotSiteButtonClicked _ -> FullUserStreamEvent.ThoughtSpotSiteButtonClicked (Aggregates.unwrapEventTimeStamp e.Data)
                    | SpeakEZSiteButtonClicked _ -> FullUserStreamEvent.SpeakEZSiteButtonClicked (Aggregates.unwrapEventTimeStamp e.Data)
                    | DataPolicyAcceptButtonClicked _ -> FullUserStreamEvent.DataPolicyAcceptButtonClicked (Aggregates.unwrapEventTimeStamp e.Data)
                    | DataPolicyDeclineButtonClicked _ -> FullUserStreamEvent.DataPolicyDeclineButtonClicked (Aggregates.unwrapEventTimeStamp e.Data)
                    | DataPolicyResetButtonClicked _ -> FullUserStreamEvent.DataPolicyResetButtonClicked (Aggregates.unwrapEventTimeStamp e.Data)
                    | SummaryActivityButtonClicked _ -> FullUserStreamEvent.SummaryActivityButtonClicked (Aggregates.unwrapEventTimeStamp e.Data)
                | :? FormEventCase as eventCase ->
                    match eventCase with
                    | ContactFormSubmitted { Form = form } -> FullUserStreamEvent.ContactFormSubmitted (Aggregates.unwrapEventTimeStamp e.Data, form)
                    | SignUpFormSubmitted { Form = form } -> FullUserStreamEvent.SignUpFormSubmitted (Aggregates.unwrapEventTimeStamp e.Data, form)
                    | SmartFormSubmitted { ClipboardInput = input } -> FullUserStreamEvent.SmartFormSubmitted (Aggregates.unwrapEventTimeStamp e.Data, input)
                    | SmartFormResultReturned { Form = form } -> FullUserStreamEvent.SmartFormResultReturned (Aggregates.unwrapEventTimeStamp e.Data, form)
                    | EmailStatusAppended { Status = status } -> FullUserStreamEvent.EmailStatusAppended (Aggregates.unwrapEventTimeStamp e.Data, status)
                    | SubscribeStatusAppended { Status = status } -> FullUserStreamEvent.SubscribeStatusAppended (Aggregates.unwrapEventTimeStamp e.Data, status)
                | :? ClientIPEventCase as eventCase ->
                    match eventCase with
                    | UserClientIPDetected { UserGeoInfo = geoInfo } -> FullUserStreamEvent.UserClientIPDetected (Aggregates.unwrapEventTimeStamp e.Data, geoInfo)
                    | UserClientIPUpdated { UserGeoInfo = geoInfo } -> FullUserStreamEvent.UserClientIPUpdated (Aggregates.unwrapEventTimeStamp e.Data, geoInfo)
                | _ -> failwith "Unknown event type"
            )
            |> Seq.toList
        return allStreamEvents
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

let retrieveTotalNewUserStreamCount (interval: (IntervalStart * IntervalEnd) option): Async<int> =
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

let retrieveUsersWhoReachedSmartFormLimit (interval: (IntervalStart * IntervalEnd) option): Async<int> =
    async {
        let! filteredEvents = filterEventsByInterval interval

        let smartFormEventCounts =
            filteredEvents
            |> Seq.choose (fun e ->
                match e.Data with
                | :? FormEventCase as eventCase ->
                    match eventCase with
                    | SmartFormSubmitted _ -> Some e.StreamId
                    | _ -> None
                | _ -> None)
            |> Seq.groupBy id
            |> Seq.map (fun (streamId, events) -> streamId, Seq.length events)
            |> Seq.filter (fun (_, count) -> count >= 5)
            |> Seq.toList

        return List.length smartFormEventCounts
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
        let! totalNewUserStreams = retrieveTotalNewUserStreamCount interval
        let! totalDataPolicyDeclined = retrieveTotalDataPolicyDeclined interval
        let! totalContactFormsSubmitted = retrieveTotalContactFormsSubmitted interval
        let! totalSmartFormUsers = retrieveTotalSmartFormUsers interval
        let! totalSignUpFormsSubmitted = retrieveTotalSignUpFormsSubmitted interval
        let! totalEmailVerifications = retrieveTotalEmailVerifications interval
        let! totalEmailUnsubscribes = retrieveTotalEmailUnsubscribes interval
        let! totalUsersWhoReachedSmartFormLimit = retrieveUsersWhoReachedSmartFormLimit interval
        return {
            TotalNewUserStreams = totalNewUserStreams
            TotalDataPolicyDeclines = totalDataPolicyDeclined
            TotalContactFormsUsed = totalContactFormsSubmitted
            TotalSmartFormUsers = totalSmartFormUsers
            TotalSignUpFormsUsed = totalSignUpFormsSubmitted
            TotalEmailVerifications = totalEmailVerifications
            TotalEmailUnsubscribes = totalEmailUnsubscribes
            TotalUsersWhoReachedSmartFormLimit = totalUsersWhoReachedSmartFormLimit
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