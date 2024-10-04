module CollabGateway.Client.Pages.CMSData

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
                            prop.text "The showcase we provide in Interworks Curator leverages public data released by the Centers for Medicare & Medicaid Services (CMS). The 'Medicare Part D Prescribers by Provider and Drug' data set provides historical information on prescription drugs issued to Medicare beneficiaries enrolled in Part D. It contains a specific collation of prescription fills that were dispensed along with total drug cost paid, organized by prescribing National Provider Identifier (NPI), drug brand name (if applicable) and drug generic name. The span of time covered by this data ranges from 2013 to 2022, a full ten-year span."
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
                                        prop.src "/img/MedPtD_Cover.png"
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
                                            prop.text "We chose a public data set that anyone could verify for themselves. And even if your company is concerned with a different domain, everyone has a sense of the complexities in dealing with their own (and their family's) healthcare."
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
                                            prop.text "The data set is useful as it represents 10 years of history plus pre-categorized data for patients both below and at/above 65 years of age. Both demography and geography are relatable without being overly complex."
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
                                        prop.src "/img/SpeakEZ_Data_Prep.png"
                                    ]
                                ]
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "Data Enrichment"
                                        ]
                                        Html.p [
                                            prop.text "As with real world projects, data preparation was also a factor. In certain cases it was as simple as column labels for human (and LLM) readability. In others it was a matter of pulling in fresh data to add new avenues of discovery."
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
                            prop.text "See The Story Develop as Current Investments Book-End New Technology"
                        ]
                        Html.div [
                            prop.className "p-4 card-body mx-auto"
                            prop.text "Organizations don't want to abandon their current investments while exploring new avenues of value. We've seen this so often that we followed suit by building a similar profile in this showcase. Efforts were taken to mimic a real-world data landscape."
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
                            prop.src "/img/Census_Regions_US.svg"
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
                    prop.className "btn btn-primary text-lg text-gray-200"
                    prop.onClick (fun e -> Router.goToUrl(e))
                    prop.href "/signup"
                    prop.text "Get On The List"
                ]
            ]
        ]
    ]