﻿module CollabGateway.Client.Pages.Index

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

let private init () = { Message = "Experience How We Built the Next Evolution of Decision Support" }, Cmd.none

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
            prop.className "flex flex-col items-center justify-center space-y-4 md:w-4/5 mx-auto"
            prop.children [
                // Header with the message
                Html.h1 [
                    prop.className "text-2xl mt-8 font-bold mx-auto"
                    prop.text state.Message
                ]
                // Animated SVG
                Html.embed [
                    prop.src "/img/Animated_SVG_small.svg"
                    prop.type' "image/svg+xml"
                    prop.className "w-full h-auto"
                ]
            ]
        ]
    ]