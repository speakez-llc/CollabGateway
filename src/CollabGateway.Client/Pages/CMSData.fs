module CollabGateway.Client.Pages.CMSData

open System
open Feliz
open Elmish
open CollabGateway.Client.Server
open CollabGateway.Client.Router
open CollabGateway.Client.ViewMsg
open CollabGateway.Shared.API
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
    | AskForMessage success -> model, Cmd.OfAsync.eitherAsResult (fun _ -> service.GetMessage (if success then "true" else "false")) MessageReceived
    | MessageReceived (Ok msg) -> { model with Message = $"Got success response: {msg}" }, Cmd.none
    | MessageReceived (Result.Error error) -> { model with Message = $"Got server error: {error}" }, Cmd.none

[<ReactComponent>]
let IndexView (parentDispatch : ViewMsg -> unit) =
    let state, dispatch = React.useElmish(init, update, [| |])

    React.useEffectOnce(fun () ->
        parentDispatch (ProcessPageVisited CMSDataPage)
    )

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
                    prop.className "card mx-auto bg-base-200 rounded-3xl"
                    prop.children [
                        Html.h1 [
                            prop.className "p-2 mx-auto card-title mt-4"
                            prop.text "Leveraging 10 Years of Public Health Information from CMS"
                        ]
                        Html.div [
                            prop.className "p-4 m-2 card-body mx-auto"
                            prop.text "This showcase uses public data released by the Centers for Medicare & Medicaid Services (CMS). The 'Medicare Part D Prescribers by Provider and Drug' data set provides historical information on prescription drugs issued to Medicare beneficiaries enrolled in Part D. It contains a specific collation of prescription fills that were dispensed along with total drug cost paid, organized by prescribing National Provider Identifier (NPI), drug brand name (if applicable) and drug generic name. The span of time covered by this data ranges from 2013 to 2022, a full ten-year span."
                        ]
                    ]
                ]
                // Bottom row with three cards
                Html.div [
                    prop.className "flex flex-col md:flex-row gap-4 mx-auto"
                    prop.children [
                        Html.div [
                            prop.className "card w-full md:w-1/3 shadow bg-base-200 rounded-3xl"
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
                                            prop.text "We chose this particular public data set for it's 'narrow' scope and relatively deep history. And if this isn't related to your particular domain, don't worry. Our guided tour will help you understand the data and its real-world use."
                                        ]
                                    ]
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "card w-full md:w-1/3 shadow bg-base-200 rounded-3xl"
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
                                            prop.text "Nearly 250 million rows of fact data: the 'year-level grain', simplified demography (below and at/above 65 years of age), as well as geography features to the city/state level bring relatable features without being overly complex."
                                        ]
                                    ]
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "card w-full md:w-1/3 shadow bg-base-200 rounded-3xl"
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
                    prop.className "card mx-auto bg-base-200 rounded-3xl"
                    prop.children [
                        Html.h1 [
                            prop.className "p-2 mx-auto card-title mt-4"
                            prop.text "See The Story Develop as Current Investments Book-End New Technology"
                        ]
                        Html.div [
                            prop.className "p-4 card-body mx-auto"
                            prop.text "Organizations want to preserve their current investments while exploring new avenues of value. We've seen this so often that we followed suit by building a similar profile in this showcase. Efforts were taken to mimic a real-world data landscape."
                        ]
                    ]
                ]
                // Bottom row with three cards
                Html.div [
                    prop.className "flex flex-col md:flex-row gap-4 mx-auto"
                    prop.children [
                        Html.div [
                            prop.className "card w-full md:w-1/3 shadow bg-base-200 rounded-3xl"
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
                                            prop.text "This report focuses on the most recent year of data. It provides many common views and controls available in Tableau reporting, and is a good place to start if you're new to the data set. "
                                        ]
                                    ]
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "card w-full md:w-1/3 shadow bg-base-200 rounded-3xl"
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
                                            prop.text "The PowerBI reports provide a longer view of the data set. The added feature of an 'Is_Opioid' column to the medications dimension enhances the original CMS data."
                                        ]
                                    ]
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "card w-full md:w-1/3 shadow bg-base-200 rounded-3xl"
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
                                            prop.text "SpeakEZ 'Rex' is a new proof-of-concept using a 'chat' style interaction. This is where the power of 'AI' moves beyond the hype and allows users to explore the data with natural language."
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
                            prop.className "btn btn-primary text-lg text-gray-200"
                            prop.onClick (fun e -> Router.goToUrl(e); parentDispatch (ProcessButtonClicked CMSDataSignUpButton))
                            prop.href "/signup"
                            prop.text "Join Our Waitlist"
                        ]
                    ]
                ]
                // Bottom card spanning 2/3 width
                Html.div [
                    prop.className "card mx-auto bg-base-200 w-full md:w-2/3 rounded-3xl"
                    prop.children [
                        Html.h1 [
                            prop.className "p-2 mx-auto card-title mt-4"
                            prop.text "Data Enrichment: Adding Regions and Other Location Elements"
                        ]
                        Html.div [
                            prop.className "p-4 m-2 card-body mx-auto"
                            prop.text "One feature of Interworks Curator we've come to appreciate is its ability to adapt its navigation and permissions to map to the organization's existing structure. Of course that can take many forms, but here we opted to show the reports from a variety of regional perspectives. This meant enriching location data from the original data set such that the Census-defined attributes of Region and Division could be grouped as the scope of reports were changed to map to the responsibility of the viewer."
                        ]
                    ]
                ]
                // Image with rounded corners
                Html.div [
                    prop.className "w-full md:w-2/3 mx-auto"
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
    ]