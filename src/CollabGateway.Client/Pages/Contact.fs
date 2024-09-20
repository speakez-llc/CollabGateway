module CollabGateway.Client.Pages.Contact

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

let private init () = { Message = "This is the Contact Form page" }, Cmd.none

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
            prop.className "flex flex-col p-4 space-y-4 transition-opacity duration-900 ease-in-out w-4/5 mx-auto"
            prop.children [
                // Header with the message
                Html.h1 [
                    prop.className "text-2xl font-bold mb-4 mx-auto"
                    prop.text state.Message
                ]
                // Skeleton placeholder for Name field
                Html.div [
                    prop.className "skeleton rounded-lg h-10 w-full"
                ]
                // Skeleton placeholder for Email field
                Html.div [
                    prop.className "skeleton rounded-lg h-10 w-full"
                ]
                // Skeleton placeholder for Message field
                Html.div [
                    prop.className "skeleton rounded-lg h-32 w-full"
                ]
                // Skeleton placeholder for Submit button
                Html.div [
                    prop.className "skeleton rounded-lg h-10 w-1/4"
                ]
            ]
        ]
    ]