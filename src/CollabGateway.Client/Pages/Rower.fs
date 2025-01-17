﻿module CollabGateway.Client.Pages.Rower

open Feliz
open Elmish
open CollabGateway.Client.Server
open CollabGateway.Client.Router
open CollabGateway.Client.ViewMsg
open CollabGateway.Shared.API
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
    | AskForMessage success -> model, Cmd.OfAsync.eitherAsResult (fun _ -> service.GetMessage (if success then "true" else "false")) MessageReceived
    | MessageReceived (Ok msg) -> { model with Message = $"Got success response: {msg}" }, Cmd.none
    | MessageReceived (Result.Error error) -> { model with Message = $"Got server error: {error}" }, Cmd.none

[<ReactComponent>]
let IndexView (parentDispatch : ViewMsg -> unit) =
    let state, dispatch = React.useElmish(init, update, [| |])

    React.useEffectOnce(fun () ->
        parentDispatch (ProcessPageVisited RowerPage)
    )

    React.fragment [
        Html.div [
            prop.className "flex flex-col p-4 space-y-4 transition-all duration-300 ease-in-out mx-auto max-w-screen-xl"
            prop.children [
                // All-text card in its own row
                Html.div [
                    prop.className "card w-full bg-base-200 shadow-3xl"
                    prop.children [
                        Html.div [
                            prop.className "card-body"
                            prop.children [
                                Html.h1 [
                                    prop.className "card-title mx-auto"
                                    prop.text "Delivering Business Solutions You Need With Tools You Trust"
                                ]
                                Html.p [
                                    prop.text "Inspired by the Results Only Work Environment (ROWE) philosophy, Rower Consulting delivers innovation through insight and performance. Our diverse team of experts, with over 100 years of combined experience, collaborate across industries and disciplines. We rely on collective expertise along with advanced tooling to deliver exceptional products where user success is the central metric. Through collaboration we uncover opportunities that support your organization, beyond the latest hype cycle, and deliver tailored analytic and operational solutions that unlock your unique potential."
                                ]
                            ]
                        ]
                    ]
                ]
                // Image with rounded corners
                Html.div [
                    prop.className "w-full md:w-2/3 mx-auto"
                    prop.children [
                        Html.img [
                            prop.src "/img/Rower_Logo_solid.svg"
                            prop.className "h-full w-full object-cover rounded-3xl"
                            prop.alt "Rower Logo"
                        ]
                    ]
                ]
                // Bottom row with three cards
                Html.div [
                    prop.className "flex flex-col md:flex-row gap-4 w-full"
                    prop.children [
                        Html.div [
                            prop.className "card w-full shadow bg-base-200 rounded-3xl"
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
                                            prop.text "From healthcare to finance, entertainment, automotive, retail and manufacturing logistics, we have the depth and breadth of expertise to help you craft solutions to gain real business value from your technology investments."
                                        ]
                                    ]
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "card w-full shadow bg-base-200 rounded-3xl"
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
                                            prop.text "Whether a highly regulated workplace or a fast-moving startup, we work with you to develop strategies that help adapt the many dimensions of today's technology landscape to maximize each organization's unique potential."
                                        ]
                                    ]
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "card w-full shadow bg-base-200 rounded-3xl"
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
                                            prop.text "While Rower is known for analytics, we also can help bring robust operational app experiences to the field and on the go via phone, tablet and even embedded devices. Modern work is not limited to web or desktop, and neither are we."
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
                Html.div [
                    prop.className "flex flex-col md:flex-row gap-4 justify-end"
                    prop.children [
                        Html.button [
                            prop.className "btn btn-primary text-lg text-gray-200 text-gray-200"
                            prop.onClick (fun e -> Router.goToUrl(e); parentDispatch (ProcessButtonClicked RowerSignUpButton))
                            prop.href "/signup"
                            prop.text "Join Our Waitlist"
                        ]
                    ]
                ]
            ]
        ]
    ]