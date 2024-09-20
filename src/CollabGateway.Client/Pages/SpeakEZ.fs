module CollabGateway.Client.Pages.SpeakEZ

open Feliz
open Feliz.DaisyUI
open Elmish
open CollabGateway.Client.Server
open UseElmish
open Fable.Core.JsInterop

// Workaround to have React-refresh working
// I need to open an issue on react-refresh to see if they can improve the detection
emitJsStatement () "import React from \"react\""

type private State = {
    Message : string
}

type private Msg =
    | AskForMessage of bool
    | MessageReceived of ServerResult<string>

let private init () = { Message = "This is the About SpeakEZ page" }, Cmd.none

let private update (msg:Msg) (model:State) : State * Cmd<Msg> =
    match msg with
    | AskForMessage success -> model, Cmd.OfAsync.eitherAsResult (fun _ -> service.GetMessage success) MessageReceived
    | MessageReceived (Ok msg) -> { model with Message = $"Got success response: {msg}" }, Cmd.none
    | MessageReceived (Error error) -> { model with Message = $"Got server error: {error}" }, Cmd.none

[<ReactComponent>]
let IndexView () =
    let state, dispatch = React.useElmish(init, update, [| |])

    React.fragment [
        Html.div [
            prop.className "flex flex-col p-4 space-y-4 transition-all duration-300 ease-in-out"
            prop.children [
                // Header with the message
                Html.h1 [
                    prop.className "text-2xl font-bold mb-4 mx-auto"
                    prop.text state.Message
                ]
                // First row with one skeleton box
                Html.div [
                    prop.className "skeleton rounded-lg h-32 w-4/5 mx-auto"
                ]
                // Second row with three skeleton boxes
                Html.div [
                    prop.className "flex justify-between space-x-4 w-4/5 mx-auto"
                    prop.children [
                        Html.div [
                            prop.className "skeleton rounded-lg h-32 w-1/3"
                        ]
                        Html.div [
                            prop.className "skeleton rounded-lg h-32 w-1/3"
                        ]
                        Html.div [
                            prop.className "skeleton rounded-lg h-32 w-1/3"
                        ]
                    ]
                ]
            ]
        ]
    ]

