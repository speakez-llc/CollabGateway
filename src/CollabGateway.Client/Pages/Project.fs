module CollabGateway.Client.Pages.Project

open Feliz
open Elmish
open CollabGateway.Client.Server
open UseElmish

type private State = {
    Message : string
}

type private Msg =
    | AskForMessage of bool
    | MessageReceived of ServerResult<string>

let private init () = { Message = "This is the Project page" }, Cmd.none

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
                // Top card spanning 80% width
                Html.div [
                    prop.className "card w-4/5 mx-auto bg-base-300"
                    prop.children [
                        Html.h1 [
                            prop.className "p-2 mx-auto card-title mt-4"
                            prop.text "Bringing Together the Tried-and-True with Leading-Edge Solutions"
                        ]
                        Html.div [
                            prop.className "p-4 m-4 card-body mx-auto"
                            prop.text "Rower's Analytics Portal showcases the power of data-driven decision-making, demonstrating how to leverage existing investments while adding new capabilities in a single responsive application. Our platform integrates seamlessly with your existing systems, providing real-time insights and actionable recommendations. With Rower, you can harness the full potential of your data and drive your business forward."
                        ]
                    ]
                ]
                // Bottom row with three cards
                Html.div [
                    prop.className "flex justify-between w-4/5 mx-auto"
                    prop.children [
                        Html.div [
                            prop.className "card m-4 w-80 shadow bg-base-300"
                            prop.style [ style.flexGrow 1 ]
                            prop.children [
                                Html.figure [
                                    Html.img [
                                        prop.src "https://picsum.photos/id/103/500/250"
                                    ]
                                ]
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "DaisyUI Card 1"
                                        ]
                                        Html.p [
                                            prop.text "Rerum reiciendis beatae tenetur excepturi aut pariatur est eos. Sit sit necessitatibus."
                                        ]
                                    ]
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "card m-4 w-80 shadow bg-base-300"
                            prop.style [ style.flexGrow 1 ]
                            prop.children [
                                Html.figure [
                                    Html.img [
                                        prop.src "https://picsum.photos/id/103/500/250"
                                    ]
                                ]
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "DaisyUI Card 2"
                                        ]
                                        Html.p [
                                            prop.text "Rerum reiciendis beatae tenetur excepturi aut pariatur est eos. Sit sit necessitatibus."
                                        ]
                                    ]
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "card m-4 w-80 shadow bg-base-300"
                            prop.style [ style.flexGrow 1 ]
                            prop.children [
                                Html.figure [
                                    Html.img [
                                        prop.src "https://picsum.photos/id/103/500/250"
                                    ]
                                ]
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "DaisyUI Card 3"
                                        ]
                                        Html.p [
                                            prop.text "Rerum reiciendis beatae tenetur excepturi aut pariatur est eos. Sit sit necessitatibus."
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
        Html.div [
            prop.className "flex flex-col p-4 space-y-4 transition-all duration-300 ease-in-out"
            prop.children [
                // Top card spanning 80% width
                Html.div [
                    prop.className "card w-4/5 mx-auto bg-base-300"
                    prop.children [
                        Html.h1 [
                            prop.className "p-2 mx-auto card-title"
                            prop.text "Delivering Business Solutions You Need With Tools You Trust"
                        ]
                        Html.div [
                            prop.className "p-4 m-4 card-body mx-auto"
                            prop.text "Inspired by the Results Only Work Environment (ROWE) philosophy, Rower Consulting draws its strength from teamwork and performance. Our diverse team of experts, boasting over 100 years of combined experience, collaborates with companies across various industries to accelerate digital transformation and unlock their full potential."
                        ]
                    ]
                ]
                // Bottom row with three cards
                Html.div [
                    prop.className "flex justify-between w-4/5 mx-auto"
                    prop.children [
                        Html.div [
                            prop.className "card m-4 w-80 shadow bg-base-300"
                            prop.style [ style.flexGrow 1 ]
                            prop.children [
                                Html.figure [
                                    Html.img [
                                        prop.src "https://picsum.photos/id/103/500/250"
                                    ]
                                ]
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "DaisyUI Card 1"
                                        ]
                                        Html.p [
                                            prop.text "Rerum reiciendis beatae tenetur excepturi aut pariatur est eos. Sit sit necessitatibus."
                                        ]
                                    ]
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "card m-4 w-80 shadow bg-base-300"
                            prop.style [ style.flexGrow 1 ]
                            prop.children [
                                Html.figure [
                                    Html.img [
                                        prop.src "https://picsum.photos/id/103/500/250"
                                    ]
                                ]
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "DaisyUI Card 2"
                                        ]
                                        Html.p [
                                            prop.text "Rerum reiciendis beatae tenetur excepturi aut pariatur est eos. Sit sit necessitatibus."
                                        ]
                                    ]
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "card m-4 w-80 shadow bg-base-300"
                            prop.style [ style.flexGrow 1 ]
                            prop.children [
                                Html.figure [
                                    Html.img [
                                        prop.src "https://picsum.photos/id/103/500/250"
                                    ]
                                ]
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "DaisyUI Card 3"
                                        ]
                                        Html.p [
                                            prop.text "Rerum reiciendis beatae tenetur excepturi aut pariatur est eos. Sit sit necessitatibus."
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