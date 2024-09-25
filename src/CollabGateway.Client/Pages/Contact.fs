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

let private init () = { Message = "Feel free to send us a message" }, Cmd.none

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
            prop.className "flex flex-col p-4 space-y-4 transition-opacity duration-900 ease-in-out w-4/5 mx-auto max-w-screen-xl"
            prop.children [
                // Header with the message
                Html.h1 [
                    prop.className "text-2xl font-bold mb-4 mx-auto"
                    prop.text state.Message
                ]
                Html.div [
                    prop.className "card mx-auto bg-base-200 w-4/5 mx-auto"
                    prop.children [
                        Html.div [
                            prop.className "p-4 m-2 card-body mx-auto"
                            prop.text "There's a lot of ways to get in touch with us. You can send us a message using the form below, and we'll reach out via email as soon as we can. We're always happy to hear from you."
                        ]
                    ]
                ]
                // Name field
                Html.input [
                    prop.className "rounded-lg h-10 w-2/3 md:w-1/3 shadow bg-base-200 pl-2"
                    prop.placeholder "Name"
                    prop.autoComplete "Name"
                ]
                // Email field
                Html.input [
                    prop.className "rounded-lg h-10 w-2/3 md:w-1/3 shadow bg-base-200 pl-2"
                    prop.placeholder "Email"
                    prop.autoComplete "Email"
                ]
                // Message field
                Html.textarea [
                    prop.className "rounded-lg h-32 w-full md:w-1/2 shadow bg-base-200 p-2"
                    prop.placeholder "Your Message"
                ]
                // Submit button
                Html.button [
                    prop.className "btn btn-primary h-10 w-1/2 md:w-1/4"
                    prop.text "Get In Touch"
                ]
            ]
        ]
    ]