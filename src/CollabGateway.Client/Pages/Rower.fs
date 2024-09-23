﻿module CollabGateway.Client.Pages.Rower

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

let private init () = { Message = "This is the Rower page" }, Cmd.none

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
                // All-text card in its own row
                Html.div [
                    prop.className "card w-4/5 mx-auto bg-base-300 shadow-3xl"
                    prop.children [
                        Html.div [
                            prop.className "card-body"
                            prop.children [
                                Html.h1 [
                                    prop.className "card-title mx-auto"
                                    prop.text "Delivering Business Solutions You Need With Tools You Trust"
                                ]
                                Html.p [
                                    prop.text "Inspired by the Results Only Work Environment (ROWE) philosophy, Rower Consulting succeeds through teamwork and performance. Our diverse team of experts, with over 100 years of combined experience, collaborate across industries and disciplines. We rely on a seasoned view and modern tooling to go beyond the latest hype cycle and deliver solutions that unlock an organization's unique potential."
                                ]
                            ]
                        ]
                    ]
                ]
                // Image at the top with rounded corners
                Html.div [
                    prop.className "w-4/5 p-2 mx-auto"
                    prop.children [
                        Html.img [
                            prop.src "/img/Rower_Logo_grad.svg"
                            prop.className "h-full w-full object-cover rounded-3xl" // Ensure the image height adjusts automatically
                            prop.alt "Rower Logo"
                        ]
                    ]
                ]
                // Bottom row with three cards
                Html.div [
                    prop.className "flex flex-col md:flex-row justify-between w-4/5 mx-auto"
                    prop.children [
                        Html.div [
                            prop.className "card m-2 w-full md:w-80 shadow bg-base-300"
                            prop.children [
                                Html.figure [
                                    Html.img [
                                        prop.src "img/hospital.png"
                                    ]
                                ]
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "A Variety of Industries"
                                        ]
                                        Html.p [
                                            prop.text "From healthcare to finance, retail to manufacturing, we have the depth and breadth of expertise to help your organization craft solutions to return real business value."
                                        ]
                                    ]
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "card m-2 w-full md:w-80 shadow bg-base-300"
                            prop.children [
                                Html.figure [
                                    Html.img [
                                        prop.src "/img/team-meeting.png"
                                    ]
                                ]
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "Flexibility & Experience"
                                        ]
                                        Html.p [
                                            prop.text "Whether a highly regulated workplace or a fast-moving startup, we work with teams to develop strategies that help them adapt to today's technology landscape."
                                        ]
                                    ]
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "card m-2 w-full md:w-80 shadow bg-base-300"
                            prop.children [
                                Html.figure [
                                    Html.img [
                                        prop.src "/img/contact.png"
                                    ]
                                ]
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "The Speed of Business"
                                        ]
                                        Html.p [
                                            prop.text "While we're known for enterprise analytics, we also can help you bring operational app experiences to the field and on the go via phone, tablet or embedded devices."
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]