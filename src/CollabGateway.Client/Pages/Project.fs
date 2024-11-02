module CollabGateway.Client.Pages.Project

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

let private init () = { Message = "About The Project" }, Cmd.none

let private update (msg:Msg) (model:State) : State * Cmd<Msg> =
    match msg with
    | AskForMessage success -> model, Cmd.OfAsync.eitherAsResult (fun _ -> service.GetMessage (if success then "true" else "false")) MessageReceived
    | MessageReceived (Ok msg) -> { model with Message = $"Got success response: {msg}" }, Cmd.none
    | MessageReceived (Result.Error error) -> { model with Message = $"Got server error: {error}" }, Cmd.none

[<ReactComponent>]
let IndexView (parentDispatch : ViewMsg -> unit) =
    let state, dispatch = React.useElmish(init, update, [| |])

    React.useEffectOnce(fun () ->
        parentDispatch (ProcessPageVisited ProjectPage)
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
                            prop.text "Blending the Tried-and-True with Leading-Edge Engineering"
                        ]
                        Html.div [
                            prop.className "p-4 m-2 card-body mx-auto"
                            prop.text "Our Analytics Portal showcases the power of data-driven decision-making, taking you step-by-step through the journey to leverage existing investments while adding new capabilities in a single responsive web application. Our platform integrates seamlessly with your existing systems, providing real-time insights and actionable recommendations. With Rower's expertise and SpeakEZ technologies, you can harness the full potential of your data and drive your business forward."
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
                                        prop.src "/img/Curator_dashboard.png"
                                    ]
                                ]
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "Step 1: Interworks Curator"
                                        ]
                                        Html.p [
                                            prop.text "We use Interworks Curator as a flexible portal that collates and coordinates all of your data. It provides a cohesive experience to place existing reports with new decision support assets under one convenient 'pane of glass'."
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
                                        prop.src "/img/TableauPBI_splash.png"
                                    ]
                                ]
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "Step 2: Integrate Your Reports"
                                        ]
                                        Html.p [
                                            prop.text "The portal 'wraps' your in-place reports with menus and permissions to match your organization's roles and access. Our sample includes a variety of report sources, including Tableau and Power BI."
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
                                        prop.src "/img/SpeakEZdefaultBannerFB.png"
                                    ]
                                ]
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "Step 3: Introduce Something New"
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
                    prop.className "card mx-auto bg-base-200 rounded-3xl"
                    prop.children [
                        Html.h1 [
                            prop.className "p-2 mx-auto card-title mt-4"
                            prop.text "Sign Up to Experience the Future Hands-On"
                        ]
                        Html.div [
                            prop.className "p-4 card-body mx-auto"
                            prop.text "After signing up for our waitlist we'll set up a guided tour that's convenient to you. At a later date we'll be releasing the site as a private beta for current and future customers to explore on their own. This collaborative process will provide guided access to the data and allow you to develop your own ideas on how a blended information portal can help your organization make better, faster business decisions."
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
                                        prop.src "/img/SignUp.png"
                                    ]
                                ]
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "Sign Up For A Guided Tour"
                                        ]
                                        Html.p [
                                            prop.text "It's quick and easy. You have three options for filling out the form: 1) typing in by hand, 2) using your browser auto-fill, or 3) SpeakEZ's Smart Form feature. The third option is an early glimpse into the power of 'AI'."
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
                                        prop.src "/img/VerifyEmail.png"
                                    ]
                                ]
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "Confirm Your Email"
                                        ]
                                        Html.p [
                                            prop.text "You'll receive a message to verify that you own the address provided. This is a standard security measure to ensure that you are the one who signed up. Once confirmed you'll be contacted by someone to set up a time to review the site."
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
                                        prop.src "/img/Login_CuratorPortal.png"
                                    ]
                                ]
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "Join Us and Explore"
                                        ]
                                        Html.p [
                                            prop.text "Initially we're setting up guided tours both to demonstrate features of the site and to gather feedback. In time we'll release the site as a private beta for those who have toured the site with us to continue exploring on their own."
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
                            prop.className "btn btn-secondary text-lg text-gray-200"
                            prop.onClick (fun e -> Router.goToUrl(e); parentDispatch (ProcessButtonClicked ProjectDataButton))
                            prop.href "/cmsdata"
                            prop.text "About The Data"
                        ]
                        Html.button [
                            prop.className "btn btn-primary text-lg text-gray-200"
                            prop.onClick (fun e -> Router.goToUrl(e); parentDispatch (ProcessButtonClicked ProjectSignUpButton))
                            prop.href "/signup"
                            prop.text "Get On The List"
                        ]
                    ]
                ]
            ]
        ]
    ]