module CollabGateway.Client.Pages.SpeakEZ

open Feliz
open Feliz.DaisyUI
open Elmish
open CollabGateway.Client.Server
open UseElmish
open CollabGateway.Client.Router


type private State = {
    Message : string
}

type private Msg =
    | AskForMessage of bool
    | MessageReceived of ServerResult<string>

let private init () = { Message = "Next-Gen Decision Support For Security And The Bottom Line" }, Cmd.none

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
            prop.className "flex flex-col p-4 space-y-4 transition-all duration-300 ease-in-out mx-auto"
            prop.children [
                Html.div [
                    prop.className "text-2xl font-bold mb-4 mx-auto"
                    prop.children [
                        Html.h1 [
                            prop.className "text-2xl font-bold mb-4 mx-auto"
                            prop.text state.Message
                        ]
                    ]
                ]
                Html.div [
                    prop.className "card w-4/5 mx-auto bg-base-200 shadow-3xl"
                    prop.children [
                        Html.div [
                            prop.className "flex flex-col md:flex-row"
                            prop.children [
                                Html.div [
                                    prop.className "w-full md:w-1/6 flex md:ml-4 mt-4 md:mt-0  justify-center"
                                    prop.children [
                                        Html.div [
                                            prop.className ""
                                            prop.children [
                                                Html.img [
                                                    prop.src "/img/SpeakEZcoloricon_small.svg"
                                                    prop.className "h-full w-full justify-center"
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                                Html.div [
                                    prop.className "card-body w-full"
                                    prop.children [
                                        Html.h1 [
                                            prop.className "card-title mx-auto"
                                            prop.text "Seasoned Experience, Modern Tools and Innovative Design"
                                        ]
                                        Html.p [
                                            prop.text "SpeakEZ is an 'AI Refinery' that uses both tried-and-true as well as leading edge machine learning tools to build and deploy customized real-time decision support systems. Solutions are designed with security and privacy as first-class considerations, and are built to run efficiently in a variety of environments. If you're looking for industrial-strength tools to support skilled, mission-critical knowledge work, SpeakEZ is the place to start."
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
                Html.div [
                    prop.className "flex flex-col md:flex-row gap-4 w-4/5 mx-auto"
                    prop.children [
                        Html.div [
                            prop.className "card w-full shadow bg-base-200"
                            prop.children [
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "Balancing Innovation with Security"
                                        ]
                                        Html.p [
                                            prop.text "Harness the power of AI to transform data into actionable insights, enabling smarter and faster decision-making processes."
                                        ]
                                        Html.p [
                                            prop.text "Our innovative solutions are designed to meet the unique needs of your business, providing you with the tools you need to succeed."
                                        ]
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "Systm2 - A Preview"
                                        ]
                                        Html.p [
                                            prop.text "Our innovative solutions are designed to meet the unique needs of your business, providing you with the tools you need to succeed."
                                        ]
                                    ]
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "card w-full shadow bg-base-200"
                            prop.children [
                                Html.img [
                                    prop.src "img/Systm2_badge_580px.svg"
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "card w-full shadow bg-base-200"
                            prop.children [
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "Deep Systems"
                                        ]
                                        Html.p [
                                            prop.text "Our innovative solutions are designed to meet the unique needs of your business, providing you with the tools you need to succeed."
                                        ]
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "Deep Background"
                                        ]
                                        Html.p [
                                            prop.text "Harness the power of AI to transform data into actionable insights, enabling smarter and faster decision-making processes."
                                        ]
                                        Html.p [
                                            prop.text "Harness the power of AI to transform data into actionable insights, enabling smarter and faster decision-making processes."
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
                Html.div [
                    prop.className "card w-4/5 mx-auto bg-base-200 shadow-3xl"
                    prop.children [
                        Html.div [
                            prop.className "flex"
                            prop.children [
                                Html.div [
                                    prop.className "card-body w-full md:w-2/3"
                                    prop.children [
                                        Html.img [
                                            prop.src "/img/Collab_Logo_narrow.svg"
                                            prop.className "object-contain h-full w-full"
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
            prop.className "flex justify-center mt-4"
            prop.children [
                Html.button [
                    prop.className "btn btn-primary text-lg"
                    prop.onClick (fun e -> Router.goToUrl(e))
                    prop.href "/signup"
                    prop.text "Sign Up Now"
                ]
            ]
        ]
    ]