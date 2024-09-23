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

let private init () = { Message = "About The Project" }, Cmd.none

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
                // Top card spanning 80% width
                Html.div [
                    prop.className "card w-4/5 mx-auto bg-base-300"
                    prop.children [
                        Html.h1 [
                            prop.className "p-2 mx-auto card-title mt-4"
                            prop.text "Tempering the Tried-and-True with Leading-Edge Engineering"
                        ]
                        Html.div [
                            prop.className "p-4 m-2 card-body mx-auto"
                            prop.text "Rower's Analytics Portal showcases the power of data-driven decision-making, taking you step-by-step through the journey to leverage existing investments while adding new capabilities in a single responsive application. Our platform integrates seamlessly with your existing systems, providing real-time insights and actionable recommendations. With Rower's expertise and SpeakEZ technologies, you can harness the full potential of your data and drive your business forward."
                        ]
                    ]
                ]
                // Bottom row with three cards
                Html.div [
                    prop.className "flex flex-col md:flex-row justify-between w-4/5 mx-auto"
                    prop.children [
                        Html.div [
                            prop.className "card m-2 w-full md:w-80 shadow bg-base-300"
                            prop.style [ style.flexGrow 1 ]
                            prop.children [
                                Html.figure [
                                    Html.img [
                                        prop.src "/img/Curator_dashboard.png"
                                    ]
                                ]
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "Step 1: Curator"
                                        ]
                                        Html.p [
                                            prop.text "Interworks Curator is a flexible portal that collates and coordinates all of your data. It provides a cohesive experience to place  existing and new decision support assets in one place."
                                        ]
                                    ]
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "card m-2 w-full md:w-80 shadow bg-base-300"
                            prop.style [ style.flexGrow 1 ]
                            prop.children [
                                Html.figure [
                                    Html.img [
                                        prop.src "/img/TableauPBI_splash.png"
                                    ]
                                ]
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "Step 2: Existing Work"
                                        ]
                                        Html.p [
                                            prop.text "The portal 'wraps' your in-place reports with menus and permissions under your control. Our sample includes a variety of report sources, including Tableau and Power BI."
                                        ]
                                    ]
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "card m-2 w-full md:w-80 shadow bg-base-300"
                            prop.style [ style.flexGrow 1 ]
                            prop.children [
                                Html.figure [
                                    Html.img [
                                        prop.src "/img/SpeakEZdefaultBannerFB.png"
                                    ]
                                ]
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "Step 3: Something New"
                                        ]
                                        Html.p [
                                            prop.text "Our partner SpeakEZ avoids hyped terms like 'AI' and focuses delivering practical innovation. Their sample application lets you explore data in surprising ways that foster deeper understanding."
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
                            prop.className "p-2 mx-auto card-title mt-4"
                            prop.text "Get Access and Experience the Future for Yourself"
                        ]
                        Html.div [
                            prop.className "p-4 m-4 card-body mx-auto"
                            prop.text "Inspired by the Results Only Work Environment (ROWE) philosophy, Rower Consulting draws its strength from teamwork and performance. Our diverse team of experts, boasting over 100 years of combined experience, collaborates with companies across various industries to accelerate digital transformation and unlock their full potential."
                        ]
                    ]
                ]
                // Bottom row with three cards
                Html.div [
                    prop.className "flex flex-col md:flex-row justify-between w-4/5 mx-auto"
                    prop.children [
                        Html.div [
                            prop.className "card m-2 w-full md:w-80 shadow bg-base-300"
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
                            prop.className "card m-2 w-full md:w-80 shadow bg-base-300"
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
                            prop.className "card ml-2 w-full md:w-80 shadow bg-base-300"
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