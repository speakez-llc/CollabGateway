module CollabGateway.Client.Pages.UserSummary

open System
open Browser.Dom
open Browser.Types
open Feliz
open Fable.FontAwesome
open Elmish
open CollabGateway.Client.Server
open CollabGateway.Shared.API
open CollabGateway.Shared.Events
open CollabGateway.Client.ViewMsg
open UseElmish

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

let renderContactForm (form: ContactForm) =
    let calculateRows (text: string) =
        let lines = text.Split('\n').Length
        let additionalRows = text.Length / 60
        lines + additionalRows

    Html.div [
        Html.div [
            prop.className "mb-2"
            prop.children [
                Html.label [
                    prop.className "text-xs font-bold block"
                    prop.text "Name"
                ]
                Html.input [
                    prop.className "form-control bg-base-200 input-s p-2 rounded-xl w-1/2 ml-auto"
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
                    prop.className "form-control bg-base-200 input-s p-2 rounded-xl w-1/2 ml-auto"
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
                    prop.className "form-control bg-base-200 input-bordered input-ghost input-s p-2 rounded-xl w-full"
                    prop.readOnly true
                    prop.value form.MessageBody
                    prop.rows (calculateRows form.MessageBody)
                ]
            ]
        ]
    ]

let renderSignUpForm (form: SignUpForm) =
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
        renderRow "Name" form.Name "Email" form.Email
        renderRow "Job Title" form.JobTitle "Phone" form.Phone
        renderRow "Department" form.Department "Company" form.Company
        renderRow "Street Address 1" form.StreetAddress1 "Street Address 2" form.StreetAddress2
        renderRow "City" form.City "State or Province" form.StateProvince
        renderRow "Postal Code" form.PostCode "Country" form.Country
    ]

let renderGeoInfo (geoInfo: GeoInfo) =
    Html.div [
        Html.p [ prop.text (sprintf "IP: %s" geoInfo.ip) ]
        Html.p [ prop.text (sprintf "ISP: %s" geoInfo.isp) ]
        Html.p [ prop.text (sprintf "Location: %s, %s, %s" geoInfo.location.city geoInfo.location.region geoInfo.location.country) ]
        Html.p [ prop.text (sprintf "Latitude: %f, Longitude: %f" geoInfo.location.lat geoInfo.location.lng) ]
        Html.p [ prop.text (sprintf "ASN: %d, Name: %s, Type: %s, Route: %s, Domain: %s" geoInfo.``as``.asn geoInfo.``as``.name geoInfo.``as``.``type`` geoInfo.``as``.route geoInfo.``as``.domain) ]
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

let renderTimeline (fullStream: FullUserStreamProjection) (dispatch: Msg -> unit) =
    let events =
        fullStream |> List.map (fun (eventName, date, content) ->
            let description =
                match eventName, content with
                | "User ClientIP Detected", Some (:? GeoInfo as geoInfo) -> renderGeoInfo geoInfo
                | "User ClientIP Updated", Some (:? GeoInfo as geoInfo) -> renderGeoInfo geoInfo
                | "Contact Form Submitted", Some (:? ContactForm as form) -> renderContactForm form
                | "SignUp Form Submitted", Some (:? SignUpForm as form) -> renderSignUpForm form
                | "Smart Form Submitted", Some (:? string as input) -> Html.text input
                | "Smart Form Result Returned", Some (:? SignUpForm as form) -> renderSignUpForm form
                | "Subscribe Status Appended", Some (:? SubscribeStatus as status) -> Html.text (sprintf "%A" status)
                | "Email Status Appended", Some (:? EmailStatus as status) -> Html.text (sprintf "%A" status)
                | _, None -> Html.none
                | _ -> Html.text (sprintf "%A" content)
            (date, eventName, description)
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
                                        prop.className "loading loading-ring loading-md text-warning animate-spin"
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
                                            prop.className "loading loading-ring loading-md text-warning animate-spin"
                                        ]
                                        Html.span [
                                            prop.className "text-warning text-xl"
                                            prop.text "Select a user..."
                                        ]
                                    ]
                                ]
                ]
            ]
        ]
    ]