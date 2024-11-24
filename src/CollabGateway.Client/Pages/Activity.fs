module CollabGateway.Client.Pages.Activity

open System
open Browser.Dom
open Feliz
open Fable.FontAwesome
open Elmish
open CollabGateway.Client.Server
open CollabGateway.Shared.API
open CollabGateway.Client.ViewMsg
open UseElmish

type State = {
    UserSummary: UserSummaryAggregate option
    TimelineStartEnd: int
}

type Msg =
    | FetchUserSummary
    | UserSummaryReceived of UserSummaryAggregate
    | FetchUserSummaryFailed of exn
    | IncrementTimelineStartEnd
    | ResetTimelineStartEnd

let init () =
    let streamToken = Guid.Parse (window.localStorage.getItem("UserStreamToken"))

    let initialState = {
        UserSummary = None
        TimelineStartEnd = 0
    }

    let fetchUserSummaryCmd =
        Cmd.OfAsync.perform (fun () -> service.RetrieveUserSummary streamToken) () UserSummaryReceived

    initialState, fetchUserSummaryCmd

let update (msg: Msg) (model: State) : State * Cmd<Msg> =
    match msg with
    | FetchUserSummary ->
        let streamToken = Guid.Parse (window.localStorage.getItem("UserStreamToken"))
        model, Cmd.OfAsync.perform (fun () -> service.RetrieveUserSummary streamToken) () UserSummaryReceived
    | UserSummaryReceived summary ->
        console.log summary
        { model with UserSummary = Some summary }, Cmd.none
    | FetchUserSummaryFailed _ ->
        model, Cmd.none
    | IncrementTimelineStartEnd ->
        { model with TimelineStartEnd = model.TimelineStartEnd + 1 }, Cmd.none
    | ResetTimelineStartEnd ->
        { model with TimelineStartEnd = 0 }, Cmd.none

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

let timelineItem (index: int) (time: string) (title: string) (content: ReactElement) =
    let isStart = (index % 2) = 0
    Html.li [
        Html.div [
            prop.className "timeline-middle"
            prop.children [
                Fa.i [ Fa.Solid.CheckCircle ] []
            ]
        ]
        Html.div [
            prop.className (if isStart then "timeline-start mb-10 md:text-end p-1 w-full" else "timeline-end mb-10 p-1 w-full")
            prop.children [
                Html.time [
                    prop.className "font-mono italic"
                    prop.text time
                ]
                Html.div [
                    prop.className "text-lg font-black"
                    prop.text title
                ]
                content
            ]
        ]
        Html.hr []
    ]

let renderTimeline (userSummary: UserSummaryAggregate) (dispatch: Msg -> unit) =
    dispatch ResetTimelineStartEnd

    let events = [
        yield (userSummary.StreamInitiated, 0, "Your First Visit", Html.none)

        match userSummary.DataPolicyDecision with
        | Some (date, choice) ->
            yield (date, 1, "Data Policy Agreement", Html.text (choice.ToString()))
        | None -> ()

        match userSummary.ContactFormSubmitted with
        | Some (date, form) ->
            yield (date, 0, "Latest Contact Form Sent", renderContactForm form)
        | None -> ()

        match userSummary.SignUpFormSubmitted with
        | Some (date, form) ->
            yield (date, 1, "Latest Sign Up Form Sent", renderSignUpForm form)
        | None -> ()

        match userSummary.EmailStatus with
        | Some (date, email, status) ->
            yield (date, 0, $"Email Status: %s{status.ToString()}", Html.text $"email: %s{email}")
        | None -> ()

        match userSummary.SubscribeStatus with
        | Some (date, email, status) ->
            yield (date, 1, $"Marketing Email Status: %s{status.ToString()}", Html.text $"email: %s{email}")
        | None -> ()
    ]

    let sortedEvents = events |> List.sortBy (fun (date, _, _, _) -> date)

    let items = sortedEvents |> List.map (fun (date, index, title, description) ->
        timelineItem index (date.ToString("yyyy-MM-dd HH:mm:ss")) title description
    )

    Html.ul [
        prop.className "timeline timeline-snap-icon max-md:timeline timeline-vertical mx-auto"
        prop.children items
    ]

[<ReactComponent>]
let IndexView (parentDispatch: ViewMsg -> unit) =
    let state, dispatch = React.useElmish(init, update, [| |])

    React.useEffectOnce(fun () ->
        parentDispatch (ProcessPageVisited ActivityPage)
    )

    Html.div [
        prop.className "flex justify-center items-center"
        prop.children [
            Html.div [
                prop.className "activity-page w-full max-w-4xl"
                prop.children [
                    Html.h1 [
                        prop.className "page-title text-center text-2xl font-bold my-4"
                        prop.text "User Activity Summary"
                    ]
                    match state.UserSummary with
                    | Some summary -> renderTimeline summary dispatch
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
                ]
            ]
        ]
    ]