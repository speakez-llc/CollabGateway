module CollabGateway.Client.Pages.SignUp

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

let private init () = { Message = "This is the SignUp page" }, Cmd.none

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
            prop.className "flex flex-col p-4 space-y-4 transition-all duration-300 ease-in-out w-4/5 mx-auto"
            prop.children [
                Html.div [
                    prop.children [
                        // Full Name
                        Html.div [
                            prop.className "relative flex flex-col space-y-2 w-full md:w-1/2"
                            prop.children [
                                Html.label [
                                    prop.className "absolute top-50 left-0 px-1"
                                    prop.style [
                                        style.zIndex 1
                                    ]
                                    prop.text "Full Name"
                                ]
                                Html.div [
                                    prop.className "skeleton rounded-lg h-10 w-full"
                                ]
                            ]
                        ]
                        // Email Address
                        Html.div [
                            prop.className "relative flex flex-col space-y-2 w-full md:w-1/2"
                            prop.children [
                                Html.label [
                                    prop.className "absolute top-50 left-0 px-1"
                                    prop.style [
                                        style.zIndex 1
                                    ]
                                    prop.text "Email Address"
                                ]
                                Html.div [
                                    prop.className "skeleton rounded-lg h-10 w-full"
                                ]
                            ]
                        ]
                        // Job Title
                        Html.div [
                            prop.className "relative flex flex-col space-y-2 w-full md:w-1/2"
                            prop.children [
                                Html.label [
                                    prop.className "absolute top-0 left-0 px-1"
                                    prop.style [
                                        style.zIndex 1
                                    ]
                                    prop.text "Job Title"
                                ]
                                Html.div [
                                    prop.className "skeleton rounded-lg h-10 w-full"
                                ]
                            ]
                        ]
                        // Department/Division
                        Html.div [
                            prop.className "relative flex flex-col space-y-2 w-full md:w-1/2"
                            prop.children [
                                Html.label [
                                    prop.className "absolute top-0 left-0 px-1"
                                    prop.style [
                                        style.zIndex 1
                                    ]
                                    prop.text "Department/Division"
                                ]
                                Html.div [
                                    prop.className "skeleton rounded-lg h-10 w-full"
                                ]
                            ]
                        ]
                        // Company Name
                        Html.div [
                            prop.className "relative flex flex-col space-y-2 w-full md:w-1/2"
                            prop.children [
                                Html.label [
                                    prop.className "absolute top-0 left-0 px-1"
                                    prop.style [
                                        style.zIndex 1
                                    ]
                                    prop.text "Company Name"
                                ]
                                Html.div [
                                    prop.className "skeleton rounded-lg h-10 w-full"
                                ]
                            ]
                        ]
                        // Phone Number
                        Html.div [
                            prop.className "relative flex flex-col space-y-2 w-full md:w-1/2"
                            prop.children [
                                Html.label [
                                    prop.className "absolute top-0 left-0 px-1"
                                    prop.style [
                                        style.zIndex 1
                                    ]
                                    prop.text "Phone Number"
                                ]
                                Html.div [
                                    prop.className "skeleton rounded-lg h-10 w-full"
                                ]
                            ]
                        ]
                        // Street Address
                        Html.div [
                            prop.className "relative flex flex-col space-y-2 w-full md:w-1/2"
                            prop.children [
                                Html.label [
                                    prop.className "absolute top-0 left-0 px-1"
                                    prop.style [
                                        style.zIndex 1
                                    ]
                                    prop.text "Street Address"
                                ]
                                Html.div [
                                    prop.className "skeleton rounded-lg h-10 w-full"
                                ]
                            ]
                        ]
                        // Street Address 2
                        Html.div [
                            prop.className "relative flex flex-col space-y-2 w-full md:w-1/2"
                            prop.children [
                                Html.label [
                                    prop.className "absolute top-0 left-0 px-1"
                                    prop.style [
                                        style.zIndex 1
                                    ]
                                    prop.text "Street Address 2"
                                ]
                                Html.div [
                                    prop.className "skeleton rounded-lg h-10 w-full"
                                ]
                            ]
                        ]
                        // PostCode
                        Html.div [
                            prop.className "relative flex flex-col space-y-2 w-full md:w-1/2"
                            prop.children [
                                Html.label [
                                    prop.className "absolute top-0 left-0 px-1"
                                    prop.style [
                                        style.zIndex 1
                                    ]
                                    prop.text "PostCode"
                                ]
                                Html.div [
                                    prop.className "skeleton rounded-lg h-10 w-full"
                                ]
                            ]
                        ]
                        // City
                        Html.div [
                            prop.className "relative flex flex-col space-y-2 w-full md:w-1/2"
                            prop.children [
                                Html.label [
                                    prop.className "absolute top-0 left-0 px-1"
                                    prop.style [
                                        style.zIndex 1
                                    ]
                                    prop.text "City"
                                ]
                                Html.div [
                                    prop.className "skeleton rounded-lg h-10 w-full"
                                ]
                            ]
                        ]
                        // State/Province
                        Html.div [
                            prop.className "relative flex flex-col space-y-2 w-full md:w-1/2"
                            prop.children [
                                Html.label [
                                    prop.className "absolute top-0 left-0 px-1"
                                    prop.style [
                                        style.zIndex 1
                                    ]
                                    prop.text "State/Province"
                                ]
                                Html.div [
                                    prop.className "skeleton rounded-lg h-10 w-full"
                                ]
                            ]
                        ]
                        // Country
                        Html.div [
                            prop.className "relative flex flex-col space-y-2 w-full md:w-1/2"
                            prop.children [
                                Html.label [
                                    prop.className "absolute top-0 left-0 px-1"
                                    prop.style [
                                        style.zIndex 1
                                    ]
                                    prop.text "Country"
                                ]
                                Html.div [
                                    prop.className "skeleton rounded-lg h-10 w-full"
                                ]
                            ]
                        ]
                        // Company Size
                        Html.div [
                            prop.className "relative flex flex-col space-y-2 w-full md:w-1/2"
                            prop.children [
                                Html.label [
                                    prop.className "absolute top-0 left-0 px-1"
                                    prop.style [
                                        style.zIndex 1
                                    ]
                                    prop.text "Company Size"
                                ]
                                Html.div [
                                    prop.className "skeleton rounded-lg h-10 w-full"
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]