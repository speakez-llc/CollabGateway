﻿module CollabGateway.Client.Pages.CMSData

open Feliz
open Elmish
open CollabGateway.Client.Server
open CollabGateway.Client.Router
open UseElmish

type private State = {
    Message : string
}

type private Msg =
    | AskForMessage of bool
    | MessageReceived of ServerResult<string>

let private init () = { Message = "About The Data" }, Cmd.none

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
            prop.className "flex flex-col p-4 space-y-4 transition-all duration-300 ease-in-out mx-auto max-w-screen-xl"
            prop.children [
                // Header with the message
                Html.h1 [
                    prop.className "text-2xl font-bold mb-4 mx-auto"
                    prop.text state.Message
                ]
                // Top card spanning 80% width
                Html.div [
                    prop.className "card mx-auto bg-base-200"
                    prop.children [
                        Html.h1 [
                            prop.className "p-2 mx-auto card-title mt-4"
                            prop.text "Leveraging 10 Years of Public Health Information from CMS"
                        ]
                        Html.div [
                            prop.className "p-4 m-2 card-body mx-auto"
                            prop.text "This showcase leverages public data released by the Centers for Medicare & Medicaid Services (CMS). The 'Medicare Part D Prescribers by Provider and Drug' dataset provides information on prescription drugs issued to Medicare beneficiaries enrolled in Part D by physicians and other health care providers. This dataset contains the total number of prescription fills that were dispensed and the total drug cost paid organized by prescribing National Provider Identifier (NPI), drug brand name (if applicable) and drug generic name. The span of time covered by this data ranges from 2013 to 2022, a full ten-year span."
                        ]
                    ]
                ]
                // Bottom row with three cards
                Html.div [
                    prop.className "flex flex-col md:flex-row gap-4 mx-auto"
                    prop.children [
                        Html.div [
                            prop.className "card w-full md:w-1/3 shadow bg-base-200"
                            prop.children [
                                Html.figure [
                                    Html.img [
                                        prop.src "/img/MedPtD_Splash_Image.png"
                                    ]
                                ]
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "A Big Picture"
                                        ]
                                        Html.p [
                                            prop.text "Interworks Curator is a flexible portal that collates and coordinates all of your data. It provides a cohesive experience to place existing reports with new decision support assets under one convenient 'pane of glass'."
                                        ]
                                    ]
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "card w-full md:w-1/3 shadow bg-base-200"
                            prop.children [
                                Html.figure [
                                    Html.img [
                                        prop.src "/img/MedPtD_PbyP.png"
                                    ]
                                ]
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "Selecting A Specific View"
                                        ]
                                        Html.p [
                                            prop.text "The portal 'wraps' your in-place reports with menus and permissions to match your organization's roles and access. Our sample includes a variety of report sources, including Tableau and Power BI."
                                        ]
                                    ]
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "card w-full md:w-1/3 shadow bg-base-200"
                            prop.children [
                                Html.figure [
                                    Html.img [
                                        prop.src "/img/MedPtD_OtherData.png"
                                    ]
                                ]
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "Options for Exploration"
                                        ]
                                        Html.p [
                                            prop.text "Our partner SpeakEZ has provided a sample application that lets you explore data in surprising new ways, and yes, even Large Language Models are allowed to enter the conversation."
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
            prop.className "flex flex-col p-4 space-y-4 transition-all duration-300 ease-in-out mx-auto max-w-screen-xl"
            prop.children [
                // Top card spanning 80% width
                Html.div [
                    prop.className "card mx-auto bg-base-200"
                    prop.children [
                        Html.h1 [
                            prop.className "p-2 mx-auto card-title mt-4"
                            prop.text "Get Access and Experience the Future Hands-On"
                        ]
                        Html.div [
                            prop.className "p-4 card-body mx-auto"
                            prop.text "After signing up and confirming your email you'll be provisioned as an 'external user' to our Curator portal. The site will grant you access to a variety of reports and dashboards.  including the SpeakEZ application. You'll be able to explore the data and see how the portal can help you make better decisions."
                        ]
                    ]
                ]
                // Bottom row with three cards
                Html.div [
                    prop.className "flex flex-col md:flex-row gap-4 mx-auto"
                    prop.children [
                        Html.div [
                            prop.className "card w-full md:w-1/3 shadow bg-base-200"
                            prop.children [
                                Html.figure [
                                    Html.img [
                                        prop.src "/img/Tableau_Cards.png"
                                    ]
                                ]
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "Tableau Summary (2022)"
                                        ]
                                        Html.p [
                                            prop.text "It's quick and easy. You have three options for filling out the form: 1) by hand, 2) using your browser auto-fill, or 3) our smart paste feature. The third option is an early glimpse into the power of 'AI'."
                                        ]
                                    ]
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "card w-full md:w-1/3 shadow bg-base-200"
                            prop.children [
                                Html.figure [
                                    Html.img [
                                        prop.src "/img/PowerBI_Waterfall.png"
                                    ]
                                ]
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "PowerBI Details (10 Years of Data)"
                                        ]
                                        Html.p [
                                            prop.text "You'll receive a message to verify that you own the address you provided. This is a standard security measure to ensure that you are the one who signed up. Once confirmed you'll see a link to the Curator portal."
                                        ]
                                    ]
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "card w-full md:w-1/3 shadow bg-base-200"
                            prop.children [
                                Html.figure [
                                    Html.img [
                                        prop.src "/img/SpeakEZ_Rex_Chat.png"
                                    ]
                                ]
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "SpeakEZ 'Rex' AI Chat"
                                        ]
                                        Html.p [
                                            prop.text "You can use the provided password to log in. If your email is part of Microsoft365, you will log in with your corporate credentials. Don't worry, only Microsoft sees your login info to confirm your identity."
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
                // Image with rounded corners
                Html.div [
                    prop.className "w-full"
                    prop.children [
                        Html.img [
                            prop.src "/img/Census_Regions_and_Division_of_the_United_States.svg"
                            prop.className "h-full w-full object-cover rounded-3xl"
                            prop.alt "Rower Logo"
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
                    prop.text "Get On The List"
                ]
            ]
        ]
    ]