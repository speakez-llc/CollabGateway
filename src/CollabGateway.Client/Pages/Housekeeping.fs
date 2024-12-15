module CollabGateway.Client.Pages.Housekeeping

open Feliz
open Elmish
open CollabGateway.Client.Server
open CollabGateway.Shared.API
open CollabGateway.Client.ViewMsg
open UseElmish

type private State = {
    Message : string
    EmptyStreams : int
}

type private Msg =
    | AskForMessage of bool
    | MessageReceived of ServerResult<string>
    | FetchEmptyStreams
    | EmptyStreamsFetched of int
    | ArchiveEmptyStreams
    | ArchiveEmptyStreamsCompleted

let private init () = { Message = "This is Housekeeping!"; EmptyStreams = 0 }, Cmd.ofMsg FetchEmptyStreams

let private update (msg:Msg) (model:State) : State * Cmd<Msg> =
    match msg with
    | AskForMessage success -> model, Cmd.OfAsync.eitherAsResult (fun _ -> service.GetMessage (if success then "true" else "false")) MessageReceived
    | MessageReceived (Ok msg) -> { model with Message = $"Got success response: {msg}" }, Cmd.none
    | MessageReceived (Result.Error error) -> { model with Message = $"Got server error: {error}" }, Cmd.none
    | FetchEmptyStreams ->
        model, Cmd.OfAsync.perform service.RetrieveCountOfEmptyStreams () EmptyStreamsFetched
    | EmptyStreamsFetched count ->
        { model with EmptyStreams = count }, Cmd.none
    | ArchiveEmptyStreams ->
        model, Cmd.OfAsync.perform service.ArchiveEmptyStreams () (fun _ -> FetchEmptyStreams)

[<ReactComponent>]
let IndexView (isAdmin: bool, parentDispatch: ViewMsg -> unit) =
    let state, dispatch = React.useElmish(init, update, [| |])


    Html.div [
        prop.className "flex justify-center items-center"
        prop.children [
            Html.div [
                prop.className "overview-page w-full max-w-4xl"
                prop.children [
                    Html.h1 [
                        prop.className "page-title text-center text-2xl font-bold my-4"
                        prop.text "Housekeeping"
                    ]
                    if isAdmin then
                        React.fragment [
                            Html.div [
                                prop.className "grid md:grid-cols-2 md:grid-rows-2 gap-4 md:w-5/6 w-full justify-self-center"
                                prop.children [
                                    Html.div [
                                        prop.className "shadow-xl flex items-center space-x-4 justify-center aspect-w-3 aspect-h-2"
                                        prop.children [
                                            Html.text $"Archive Empty Streams ({state.EmptyStreams})"
                                            Html.button [
                                                prop.className "btn btn-sm btn-primary ml-2"
                                                prop.text "Archive"
                                                prop.onClick (fun _ -> dispatch ArchiveEmptyStreams)
                                            ]
                                        ]
                                    ]
                                    Html.div [
                                        prop.className "card shadow-xl flex items-center space-x-4 justify-center aspect-w-3 aspect-h-2"
                                        prop.text "Quadrant 2"
                                    ]
                                    Html.div [
                                        prop.className "card shadow-xl flex items-center space-x-4 justify-center aspect-w-3 aspect-h-2"
                                        prop.text "Quadrant 3"
                                    ]
                                    Html.div [
                                        prop.className "card shadow-xl flex items-center space-x-4 justify-center aspect-w-3 aspect-h-2"
                                        prop.text "Quadrant 4"
                                    ]
                                ]
                            ]
                        ]
                    else
                        Html.div [
                            prop.className "flex items-center space-x-2 justify-center"
                            prop.children [
                                Html.span [
                                    prop.className "text-warning text-xl"
                                    prop.text "You do not have permission to view this page."
                                ]
                            ]
                        ]
                ]
            ]
        ]
    ]