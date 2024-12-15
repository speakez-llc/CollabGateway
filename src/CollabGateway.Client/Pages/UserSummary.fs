module CollabGateway.Client.Pages.UserSummary

open System
open Browser.Types
open Feliz
open Fable.FontAwesome
open Elmish
open CollabGateway.Client.Server
open CollabGateway.Shared.API
open CollabGateway.Client.ViewMsg
open UseElmish

let generateBreadcrumbPaths (taxonomy: GicsTaxonomy[]) =
    taxonomy
    |> Array.collect (fun g ->
        let sectorPath = [g.SectorName]
        let industryGroupPath = if String.IsNullOrWhiteSpace(g.IndustryGroupName) then [] else [g.IndustryGroupName]
        let industryPath = if String.IsNullOrWhiteSpace(g.IndustryName) then [] else [g.IndustryName]
        let subIndustryPath = if String.IsNullOrWhiteSpace(g.SubIndustryName) then [] else [g.SubIndustryName]
        let fullPath = sectorPath @ industryGroupPath @ industryPath @ subIndustryPath
        [|
            (g.SectorCode, String.concat " > " sectorPath)
            (g.IndustryGroupCode, String.concat " > " (sectorPath @ industryGroupPath))
            (g.IndustryCode, String.concat " > " (sectorPath @ industryGroupPath @ industryPath))
            (g.SubIndustryCode, String.concat " > " fullPath)
        |]
    )
    |> Array.distinctBy snd
    |> Array.sortBy fst
    |> Map.ofArray

type State = {
    FullUserStream: FullUserStreamProjection option
    TimelineStartEnd: int
    SelectedUser: StreamToken option
    UserStreams: UserStreamProjection option
}

type Msg =
    | FetchUserSummary
    | UserSummaryReceived of FullUserStreamProjection
    | FetchUserSummaryFailed of exn
    | IncrementTimelineStartEnd
    | ResetTimelineStartEnd
    | FetchUserStreams
    | UserStreamsReceived of UserStreamProjection
    | UserStreamsFetchFailed of exn
    | UserSelected of StreamToken
    | FullUserStreamReceived of FullUserStreamProjection

let init () =
    let initialState = {
        FullUserStream = None
        TimelineStartEnd = 0
        SelectedUser = None
        UserStreams = None
    }

    let fetchUserStreamsCmd =
        Cmd.OfAsync.perform service.RetrieveAllUserNames () UserStreamsReceived

    initialState, fetchUserStreamsCmd

let renderContactForm (form: ContactForm) =
    let calculateRows (text: string) =
        let lines = text.Split('\n').Length
        let additionalRows = text.Length / 60
        lines + additionalRows

    Html.div [
        prop.className "collapse collapse-arrow border border-base-300 bg-base-100 rounded-box"
        prop.children [
            Html.input [
                prop.type' "checkbox"
                prop.defaultChecked true
            ]
            Html.div [
                prop.className "collapse-title bg-base-200 font-medium text-gold"
                prop.text "Contact Form"
            ]
            Html.div [
                prop.className "collapse-content"
                prop.children [
                    Html.div [
                        prop.className "mb-2"
                        prop.children [
                            Html.label [
                                prop.className "text-xs font-bold"
                                prop.text "Name"
                            ]
                            Html.input [
                                prop.className "form-control bg-base-200 input-s p-2 rounded-xl w-full"
                                prop.readOnly true
                                prop.value form.Name
                            ]
                        ]
                    ]
                    Html.div [
                        prop.className "mb-2"
                        prop.children [
                            Html.label [
                                prop.className "text-xs font-bold block"
                                prop.text "Email"
                            ]
                            Html.input [
                                prop.className "form-control bg-base-200 input input-ghost input-s p-2 rounded-xl w-full"
                                prop.readOnly true
                                prop.value form.Email
                            ]
                        ]
                    ]
                    Html.div [
                        prop.className "mb-2"
                        prop.children [
                            Html.label [
                                prop.className "text-xs font-bold block"
                                prop.text "Message"
                            ]
                            Html.textarea [
                                prop.className "form-control bg-base-200 input input-ghost input-s p-2 rounded-xl w-full"
                                prop.readOnly true
                                prop.value form.MessageBody
                                prop.rows (calculateRows form.MessageBody)
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

let renderSmartFormReturn (form: SignUpForm) =
    let renderRow (label1: string) (value1: string) (label2: string) (value2: string) =
        Html.div [
            prop.className "flex space-x-4"
            prop.children [
                Html.div [
                    prop.className "flex-1"
                    prop.children [
                        Html.label [
                            prop.className "text-xs font-bold block mb-1"
                            prop.text label1
                        ]
                        Html.input [
                            prop.className "form-control bg-base-200 input-bordered input-ghost input-s p-2 rounded-xl w-full"
                            prop.readOnly true
                            prop.value value1
                        ]
                    ]
                ]
                Html.div [
                    prop.className "flex-1"
                    prop.children [
                        Html.label [
                            prop.className "text-xs font-bold block mb-1"
                            prop.text label2
                        ]
                        Html.input [
                            prop.className "form-control bg-base-200 input-bordered input-ghost input-s p-2 rounded-xl w-full"
                            prop.readOnly true
                            prop.value value2
                        ]
                    ]
                ]
            ]
        ]
    Html.div [
        prop.className "collapse collapse-arrow border border-base-300 bg-base-100 rounded-box"
        prop.children [
            Html.input [
                prop.type' "checkbox"
                prop.defaultChecked true
            ]
            Html.div [
                prop.className "collapse-title bg-base-200 font-medium text-gold"
                prop.text "Smart Form Returned Structure"
            ]
            Html.div [
                prop.className "collapse-content"
                prop.children [
                    renderRow "Name" form.Name "Email" form.Email
                    renderRow "Job Title" form.JobTitle "Phone" form.Phone
                    renderRow "Department" form.Department "Industry" form.Industry
                    renderRow "Street Address 1" form.StreetAddress1 "Street Address 2" form.StreetAddress2
                    renderRow "City" form.City "State or Province" form.StateProvince
                    renderRow "Postal Code" form.PostCode "Country" form.Country
                ]
            ]
        ]
    ]

let renderSignUpForm (form: SignUpForm) (taxonomy: GicsTaxonomy[]) =
    let breadcrumbPaths = generateBreadcrumbPaths taxonomy
    let industryPath =
        match Map.tryFind form.Industry breadcrumbPaths with
        | Some path -> path
        | None -> form.Industry

    let renderRow (label1: string) (value1: string) (label2: string) (value2: string) =
        Html.div [
            prop.className "flex space-x-4"
            prop.children [
                Html.div [
                    prop.className "flex-1"
                    prop.children [
                        Html.label [
                            prop.className "text-xs font-bold block mb-1"
                            prop.text label1
                        ]
                        Html.input [
                            prop.className "form-control bg-base-200 input-bordered input-ghost input-s p-2 rounded-xl w-full"
                            prop.readOnly true
                            prop.value value1
                        ]
                    ]
                ]
                Html.div [
                    prop.className "flex-1"
                    prop.children [
                        Html.label [
                            prop.className "text-xs font-bold block mb-1"
                            prop.text label2
                        ]
                        Html.input [
                            prop.className "form-control bg-base-200 input-bordered input-ghost input-s p-2 rounded-xl w-full"
                            prop.readOnly true
                            prop.value value2
                        ]
                    ]
                ]
            ]
        ]
    Html.div [
        prop.className "collapse collapse-arrow border border-base-300 bg-base-100 rounded-box"
        prop.children [
            Html.input [
                prop.type' "checkbox"
                prop.defaultChecked true
            ]
            Html.div [
                prop.className "collapse-title bg-base-200 font-medium text-gold"
                prop.text "Sign Up Information"
            ]
            Html.div [
                prop.className "collapse-content"
                prop.children [
                    renderRow "Name" form.Name "Email" form.Email
                    renderRow "Job Title" form.JobTitle "Phone" form.Phone
                    renderRow "Department" form.Department "Industry" industryPath
                    renderRow "Street Address 1" form.StreetAddress1 "Street Address 2" form.StreetAddress2
                    renderRow "City" form.City "State or Province" form.StateProvince
                    renderRow "Postal Code" form.PostCode "Country" form.Country
                ]
            ]
        ]
    ]

let renderUserStreamDropdown (userStreams: UserStreamProjection) (dispatch: Msg -> unit) =
    Html.select [
        prop.className "form-select"
        prop.onChange (fun (ev: Browser.Types.Event) ->
            let target = ev.target :?> HTMLSelectElement
            let value = target.value
            let streamToken = Guid.Parse value
            dispatch (UserSelected streamToken)
        )
        prop.children (
            userStreams |> List.map (fun userStream ->
                let displayText =
                    match userStream.UserName, userStream.Email with
                    | Some name, Some email -> sprintf "%s (%s) - %d events" name email userStream.EventCount
                    | _ -> sprintf "%s - %d events" (userStream.StreamToken.ToString()) userStream.EventCount
                Html.option [
                    prop.value (userStream.StreamToken.ToString())
                    prop.text displayText
                ]
            )
        )
    ]

let update (msg: Msg) (model: State) : State * Cmd<Msg> =
    match msg with
    | FetchUserSummary ->
        match model.SelectedUser with
        | Some streamToken ->
            model, Cmd.OfAsync.perform (fun () -> service.RetrieveFullUserStream streamToken) () UserSummaryReceived
        | None -> model, Cmd.none
    | UserSummaryReceived summary ->
        { model with FullUserStream = Some summary }, Cmd.none
    | FetchUserSummaryFailed _ ->
        model, Cmd.none
    | IncrementTimelineStartEnd ->
        { model with TimelineStartEnd = model.TimelineStartEnd + 1 }, Cmd.none
    | ResetTimelineStartEnd ->
        { model with TimelineStartEnd = 0 }, Cmd.none
    | FetchUserStreams ->
        model, Cmd.OfAsync.perform service.RetrieveAllUserNames () UserStreamsReceived
    | UserStreamsReceived streams ->
        { model with UserStreams = Some streams }, Cmd.none
    | UserStreamsFetchFailed _ ->
        model, Cmd.none
    | UserSelected streamToken ->
        { model with SelectedUser = Some streamToken; FullUserStream = None }, Cmd.OfAsync.perform (fun () -> service.RetrieveFullUserStream streamToken) () FullUserStreamReceived
    | FullUserStreamReceived fullStream ->
        { model with FullUserStream = Some fullStream }, Cmd.ofMsg ResetTimelineStartEnd

let renderGeoInfo (geoInfo: GeoInfo) =
    let renderRow (label1: string) (value1: string) (label2: string) (value2: string) =
        Html.div [
            prop.className "flex space-x-4"
            prop.children [
                Html.div [
                    prop.className "flex-1"
                    prop.children [
                        Html.label [
                            prop.className "text-xs font-bold block mb-1"
                            prop.text label1
                        ]
                        Html.input [
                            prop.className "form-control bg-base-200 input-bordered input-ghost input-s p-2 rounded-xl w-full"
                            prop.readOnly true
                            prop.value value1
                        ]
                    ]
                ]
                Html.div [
                    prop.className "flex-1"
                    prop.children [
                        Html.label [
                            prop.className "text-xs font-bold block mb-1"
                            prop.text label2
                        ]
                        Html.input [
                            prop.className "form-control bg-base-200 input-bordered input-ghost input-s p-2 rounded-xl w-full"
                            prop.readOnly true
                            prop.value value2
                        ]
                    ]
                ]
            ]
        ]
    Html.div [
        prop.className "collapse collapse-arrow border border-base-300 bg-base-100 rounded-box"
        prop.children [
            Html.input [
                prop.type' "checkbox"
                prop.defaultChecked true
            ]
            Html.div [
                prop.className "collapse-title bg-base-200 font-medium text-gold"
                prop.text "Geo Information"
            ]
            Html.div [
                prop.className "collapse-content"
                prop.children [
                    renderRow "Name" geoInfo.``as``.name "Type" geoInfo.``as``.``type``
                    renderRow "ISP" geoInfo.isp "Location" (sprintf "%s, %s, %s" geoInfo.location.city geoInfo.location.region geoInfo.location.country)
                    renderRow "Latitude" (sprintf "%f" geoInfo.location.lat) "Longitude" (sprintf "%f" geoInfo.location.lng)
                    renderRow "IP" geoInfo.ip "Route" geoInfo.``as``.route
                    renderRow "ASN" (sprintf "%d" geoInfo.``as``.asn) "Domain" geoInfo.``as``.domain
                ]
            ]
        ]
    ]

let timelineItem (time: string) (title: string) (content: ReactElement) =
    Html.li [
        Html.div [
            prop.className "timeline-start mr-2"
            prop.children [
                Html.time [
                    prop.className "font-mono italic"
                    prop.text time
                ]
            ]
        ]
        Html.div [
            prop.className "timeline-middle"
            prop.children [
                Fa.i [ Fa.Solid.ArrowCircleRight ] []
            ]
        ]
        Html.div [
            prop.className "timeline-end timeline-box"
            prop.children [
                Html.div [
                    prop.className "text-lg font-black"
                    prop.text title
                ]
                content
            ]
        ]
        Html.hr []
    ]

let renderTimeline (fullStream: FullUserStreamEvent list) (dispatch: Msg -> unit) =
    let events =
        fullStream |> List.map (fun event ->
            let date, title, description =
                match event with
                | FullUserStreamEvent.UserStreamInitiated date -> (date, "User Stream Initiated", Html.none)
                | FullUserStreamEvent.UserStreamResumed date -> (date, "User Stream Resumed", Html.none)
                | FullUserStreamEvent.UserStreamClosed date -> (date, "User Stream Closed", Html.none)
                | FullUserStreamEvent.UserStreamEnded date -> (date, "User Stream Ended", Html.none)
                | FullUserStreamEvent.DataPolicyAccepted date -> (date, "Data Policy Accepted", Html.none)
                | FullUserStreamEvent.DataPolicyDeclined date -> (date, "Data Policy Declined", Html.none)
                | FullUserStreamEvent.DataPolicyReset date -> (date, "Data Policy Reset", Html.none)
                | FullUserStreamEvent.HomePageVisited date -> (date, "Home Page Visited", Html.none)
                | FullUserStreamEvent.ProjectPageVisited date -> (date, "Project Page Visited", Html.none)
                | FullUserStreamEvent.DataPageVisited date -> (date, "Data Page Visited", Html.none)
                | FullUserStreamEvent.SignupPageVisited date -> (date, "Signup Page Visited", Html.none)
                | FullUserStreamEvent.RowerPageVisited date -> (date, "Rower Page Visited", Html.none)
                | FullUserStreamEvent.SpeakEZPageVisited date -> (date, "SpeakEZ Page Visited", Html.none)
                | FullUserStreamEvent.ContactPageVisited date -> (date, "Contact Page Visited", Html.none)
                | FullUserStreamEvent.PartnersPageVisited date -> (date, "Partners Page Visited", Html.none)
                | FullUserStreamEvent.DataPolicyPageVisited date -> (date, "Data Policy Page Visited", Html.none)
                | FullUserStreamEvent.SummaryActivityPageVisited date -> (date, "Summary Activity Page Visited", Html.none)
                | FullUserStreamEvent.HomeButtonClicked date -> (date, "Home Button Clicked", Html.none)
                | FullUserStreamEvent.HomeProjectButtonClicked date -> (date, "Home Project Button Clicked", Html.none)
                | FullUserStreamEvent.HomeSignUpButtonClicked date -> (date, "Home SignUp Button Clicked", Html.none)
                | FullUserStreamEvent.ProjectButtonClicked date -> (date, "Project Button Clicked", Html.none)
                | FullUserStreamEvent.ProjectDataButtonClicked date -> (date, "Project Data Button Clicked", Html.none)
                | FullUserStreamEvent.ProjectSignUpButtonClicked date -> (date, "Project SignUp Button Clicked", Html.none)
                | FullUserStreamEvent.DataButtonClicked date -> (date, "Data Button Clicked", Html.none)
                | FullUserStreamEvent.DataSignUpButtonClicked date -> (date, "Data SignUp Button Clicked", Html.none)
                | FullUserStreamEvent.SignUpButtonClicked date -> (date, "SignUp Button Clicked", Html.none)
                | FullUserStreamEvent.SmartFormButtonClicked date -> (date, "Smart Form Button Clicked", Html.none)
                | FullUserStreamEvent.SmartFormSubmittedButtonClicked date -> (date, "Smart Form Submitted Button Clicked", Html.none)
                | FullUserStreamEvent.RowerButtonClicked date -> (date, "Rower Button Clicked", Html.none)
                | FullUserStreamEvent.RowerSignUpButtonClicked date -> (date, "Rower SignUp Button Clicked", Html.none)
                | FullUserStreamEvent.SpeakEZButtonClicked date -> (date, "SpeakEZ Button Clicked", Html.none)
                | FullUserStreamEvent.SpeakEZSignUpButtonClicked date -> (date, "SpeakEZ SignUp Button Clicked", Html.none)
                | FullUserStreamEvent.ContactButtonClicked date -> (date, "Contact Button Clicked", Html.none)
                | FullUserStreamEvent.PartnersButtonClicked date -> (date, "Partners Button Clicked", Html.none)
                | FullUserStreamEvent.RowerSiteButtonClicked date -> (date, "Rower Site Button Clicked", Html.none)
                | FullUserStreamEvent.CuratorSiteButtonClicked date -> (date, "Curator Site Button Clicked", Html.none)
                | FullUserStreamEvent.TableauSiteButtonClicked date -> (date, "Tableau Site Button Clicked", Html.none)
                | FullUserStreamEvent.PowerBISiteButtonClicked date -> (date, "PowerBI Site Button Clicked", Html.none)
                | FullUserStreamEvent.ThoughtSpotSiteButtonClicked date -> (date, "ThoughtSpot Site Button Clicked", Html.none)
                | FullUserStreamEvent.SpeakEZSiteButtonClicked date -> (date, "SpeakEZ Site Button Clicked", Html.none)
                | FullUserStreamEvent.DataPolicyAcceptButtonClicked date -> (date, "Data Policy Accept Button Clicked", Html.none)
                | FullUserStreamEvent.DataPolicyDeclineButtonClicked date -> (date, "Data Policy Decline Button Clicked", Html.none)
                | FullUserStreamEvent.DataPolicyResetButtonClicked date -> (date, "Data Policy Reset Button Clicked", Html.none)
                | FullUserStreamEvent.SummaryActivityButtonClicked date -> (date, "Summary Activity Button Clicked", Html.none)
                | FullUserStreamEvent.ContactFormSubmitted (date, form) -> (date, "Contact Form Submitted", renderContactForm form)
                | FullUserStreamEvent.SignUpFormSubmitted (date, form) ->
                    let loadTaxonomyAsync () = async {
                        let! taxonomy = service.LoadGicsTaxonomy()
                        return taxonomy
                    }
                    let taxonomy = Async.RunSynchronously (loadTaxonomyAsync ())
                    (date, "SignUp Form Submitted", renderSignUpForm form taxonomy)
                | FullUserStreamEvent.SmartFormSubmitted (date, input) ->
                    let renderSmartFormSubmitted (input: string) =
                        Html.div [
                            prop.className "collapse collapse-arrow border border-base-300 bg-base-100 rounded-box"
                            prop.children [
                                Html.input [
                                    prop.type' "checkbox"
                                ]
                                Html.div [
                                    prop.className "collapse-title bg-base-200 font-medium text-gold"
                                    prop.text "Smart Form Text Input"
                                ]
                                Html.div [
                                    prop.className "collapse-content"
                                    prop.children [
                                        Html.text input
                                    ]
                                ]
                            ]
                        ]
                    (date, "Smart Form Submitted", renderSmartFormSubmitted input)
                | FullUserStreamEvent.SmartFormResultReturned (date, form) -> (date, "Smart Form Result Returned", renderSmartFormReturn form)
                | FullUserStreamEvent.EmailStatusAppended (date, status) -> (date, "Email Status Appended", Html.text (sprintf "%A" status))
                | FullUserStreamEvent.SubscribeStatusAppended (date, status) -> (date, "Subscribe Status Appended", Html.text (sprintf "%A" status))
                | FullUserStreamEvent.UserClientIPDetected (date, geoInfo) -> (date, "User ClientIP Detected", renderGeoInfo geoInfo)
                | FullUserStreamEvent.UserClientIPUpdated (date, geoInfo) -> (date, "User ClientIP Updated", renderGeoInfo geoInfo)
                | FullUserStreamEvent.ContactActivityButtonClicked date -> (date, "Contact Activity Button Clicked", Html.none)
                | FullUserStreamEvent.SignUpActivityButtonClicked date -> (date, "Sign Up Activity Button Clicked", Html.none)
            (date, title, description)
        )

    let sortedEvents = events |> List.sortBy (fun (date, _, _) -> date)

    let items = sortedEvents |> List.map (fun (date, title, description) ->
        timelineItem (date.ToString("yyyy-MM-dd HH:mm:ss")) title description
    )

    Html.ul [
        prop.className "timeline timeline-vertical mx-auto"
        prop.children items
    ]

[<ReactComponent>]
let IndexView (isAdmin: bool, parentDispatch: ViewMsg -> unit) =
    let state, dispatch = React.useElmish(init, update, [| |])

    React.useEffect(fun () ->
        if state.FullUserStream.IsSome then
            dispatch ResetTimelineStartEnd
    , [| state.FullUserStream :> obj |])

    Html.div [
        prop.className "flex justify-center items-center"
        prop.children [
            Html.div [
                prop.className "w-full max-w-4xl"
                prop.children [
                    Html.h1 [
                        prop.className "text-2xl font-bold mb-4"
                        prop.text "User Summary"
                    ]
                    match state.UserStreams with
                    | Some userStreams -> renderUserStreamDropdown userStreams dispatch
                    | None -> Html.div [
                                prop.className "flex items-center space-x-2 justify-center"
                                prop.children [
                                    Html.div [
                                        prop.className "loading loading-ring loading-lg text-warning animate-spin"
                                    ]
                                    Html.span [
                                        prop.className "text-warning text-xl"
                                        prop.text "Loading..."
                                    ]
                                ]
                           ]
                    match state.FullUserStream with
                    | Some summary -> renderTimeline summary dispatch
                    | None -> Html.div [
                                    prop.className "flex items-center space-x-2 justify-center"
                                    prop.children [
                                        Html.div [
                                            prop.className "loading loading-bars loading-sm text-info"
                                        ]
                                        Html.span [
                                            prop.className "text-info text-xl"
                                            prop.text "Select a user..."
                                        ]
                                    ]
                                ]
                ]
            ]
        ]
    ]