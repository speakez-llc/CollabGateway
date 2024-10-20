module CollabGateway.Client.Pages.Partners

open Feliz
open Elmish
open CollabGateway.Client.Server
open CollabGateway.Client.ViewMsg
open CollabGateway.Shared.API
open UseElmish

type private State = {
    Message : string
}

type private Msg =
    | AskForMessage of bool
    | MessageReceived of ServerResult<string>

let private init () = { Message = "Partners & Links" }, Cmd.none

let private update (msg:Msg) (model:State) : State * Cmd<Msg> =
    match msg with
    | AskForMessage success -> model, Cmd.OfAsync.eitherAsResult (fun _ -> service.GetMessage success) MessageReceived
    | MessageReceived (Ok msg) -> { model with Message = $"Got success response: {msg}" }, Cmd.none
    | MessageReceived (Result.Error error) -> { model with Message = $"Got server error: {error}" }, Cmd.none

[<ReactComponent>]
let IndexView (parentDispatch : ViewMsg -> unit) =
    let state, dispatch = React.useElmish(init, update, [| |])

    React.useEffectOnce(fun () ->
        parentDispatch (ProcessPageVisited PartnersPage)
    )

    React.fragment [
        Html.div [
            prop.className "flex flex-col p-4 space-y-4 transition-all duration-300 ease-in-out mx-auto max-w-screen-xl w-full md:w-1/2"
            prop.children [
                // Header with the message
                Html.h1 [
                    prop.className "text-2xl font-bold mb-4 mx-auto"
                    prop.text state.Message
                ]
                Html.div [
                    prop.className "card mx-auto bg-base-200 shadow-3xl rounded-3xl w-full md:w-4/5 pt-6"
                    prop.children [
                        Html.figure [
                            Html.img [
                                prop.src "/img/RowerConsulting_Logo_t.svg"
                            ]
                        ]
                        Html.div [
                            prop.className "card-body"
                            prop.children [
                                Html.h2 [
                                    prop.className "card-title"
                                    prop.children [
                                        Html.a [
                                            prop.href "https://www.rowerconsulting.com/"
                                            prop.target "_blank"
                                            prop.onClick (fun _ -> parentDispatch (ProcessButtonClicked RowerSiteButton))
                                            prop.children [
                                                Html.span [
                                                    prop.text "Main Website"
                                                ]
                                                Html.i [
                                                    prop.className "fas fa-external-link-alt ml-2 text-gold"
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                            ]
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
                                        prop.className "bg-gray-300"
                                        prop.src "img/curator_by_interworks.png"
                                    ]
                                ]
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.children [
                                                Html.a [
                                                    prop.href "https://www.interworks.com/curator"
                                                    prop.target "_blank"
                                                    prop.onClick (fun _ -> parentDispatch (ProcessButtonClicked CuratorSiteButton))
                                                    prop.children [
                                                        Html.span [
                                                            prop.text "Curator"
                                                        ]
                                                        Html.i [
                                                            prop.className "fas fa-external-link-alt ml-2 text-gold"
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
                            prop.className "card w-full shadow bg-base-200 rounded-3xl"
                            prop.children [
                                Html.figure [
                                    Html.img [
                                        prop.className "bg-gray-200"
                                        prop.src "/img/Tableau-Emblem.png"
                                    ]
                                ]
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.children [
                                                Html.a [
                                                    prop.href "https://www.salesforce.com/products/tableau/"
                                                    prop.target "_blank"
                                                    prop.onClick (fun _ -> parentDispatch (ProcessButtonClicked TableauSiteButton))
                                                    prop.children [
                                                        Html.span [
                                                            prop.text "Tableau"
                                                        ]
                                                        Html.i [
                                                            prop.className "fas fa-external-link-alt ml-2 text-gold"
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
                                        prop.className "bg-gray-500"
                                        prop.src "img/PowerBI.png"
                                    ]
                                ]
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.children [
                                                Html.a [
                                                    prop.href "https://powerbi.microsoft.com/"
                                                    prop.target "_blank"
                                                    prop.onClick (fun _ -> parentDispatch (ProcessButtonClicked PowerBISiteButton))
                                                    prop.children [
                                                        Html.span [
                                                            prop.text "Power BI"
                                                        ]
                                                        Html.i [
                                                            prop.className "fas fa-external-link-alt ml-2 text-gold"
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
                            prop.className "card w-full shadow bg-base-200 rounded-3xl"
                            prop.children [
                                Html.figure [
                                    Html.img [
                                        prop.className "bg-gray-100"
                                        prop.src "/img/ThoughtSpot_Logo.png"
                                    ]
                                ]
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.children [
                                                Html.a [
                                                    prop.href "https://www.thoughtspot.com/"
                                                    prop.target "_blank"
                                                    prop.onClick (fun _ -> parentDispatch (ProcessButtonClicked ThoughtSpotSiteButton))
                                                    prop.children [
                                                        Html.span [
                                                            prop.text "ThoughtSpot"
                                                        ]
                                                        Html.i [
                                                            prop.className "fas fa-external-link-alt ml-2 text-gold"
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
                ]
                Html.div [
                    prop.className "card  mx-auto bg-base-200 shadow-3xl rounded-3xl w-full md:w-4/5"
                    prop.children [
                        Html.figure [
                            Html.img [
                                prop.className ""
                                prop.src "/img/SpeakEZcolorBanner.svg"
                            ]
                        ]
                        Html.div [
                            prop.className "card-body"
                            prop.children [
                                Html.h2 [
                                    prop.className "card-title"
                                    prop.children [
                                        Html.a [
                                            prop.href "https://speakez.ai/"
                                            prop.target "_blank"
                                            prop.onClick (fun _ -> parentDispatch (ProcessButtonClicked SpeakEZSiteButton))
                                            prop.children [
                                                Html.span [
                                                    prop.text "SpeakEZ.ai"
                                                ]
                                                Html.i [
                                                    prop.className "fas fa-external-link-alt ml-2 text-gold"
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
        ]
    ]