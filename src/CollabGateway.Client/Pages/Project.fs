module CollabGateway.Client.Pages.Project

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
                                            prop.text "Interworks Curator is a flexible portal that collates and coordinates all of your data. It provides a cohesive experience to place existing reports with new decision support assets under one convenient 'pane of glass'."
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
                                            prop.text "The portal 'wraps' your in-place reports with menus and permissions to match your organization's roles and access. Our sample includes a variety of report sources, including Tableau and Power BI."
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
                                            prop.text "Our partner SpeakEZ has provided a sample application lets you explore data in surprising new ways, and yes, even Large Language Models are allowed to enter the conversation."
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
                    prop.className "flex flex-col md:flex-row justify-between w-4/5 mx-auto"
                    prop.children [
                        Html.div [
                            prop.className "card m-2 w-full md:w-80 shadow bg-base-300"

                            prop.style [ style.flexGrow 1 ]
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
                                            prop.text "Sign Up For Access"
                                        ]
                                        Html.p [
                                            prop.text "It's quick and easy. You have three options for filling out the form: 1) by hand, 2) using your browser auto-fill, or 3) our smart paste feature. The third option is an early glimpse into the power of 'AI'."
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
                                        prop.src "/img/VerifyEmail.png"
                                    ]
                                ]
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "Confirm Ownership of your Email"
                                        ]
                                        Html.p [
                                            prop.text "You'll receive an email from SpeakEZ that will prompt you to verify that you own the email address you provided. This is a standard security measure to ensure that you are the one who signed up."
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
                                        prop.src "/img/Login_CuratorPortal.png"
                                    ]
                                ]
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "Log In and Explore"
                                        ]
                                        Html.p [
                                            prop.text "You can use the provided password to log in. And if your email is part of a Microsoft365, you will log in with your corporate credentials. Don't worry, only Microsoft sees your login info to verify that you are 'you'."
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