module CollabGateway.Client.Pages.SpeakEZ

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

let private init () = { Message = "Next-Gen Decision Support For Speed, Security And The Bottom Line" }, Cmd.none

let private update (msg:Msg) (model:State) : State * Cmd<Msg> =
    match msg with
    | AskForMessage success -> model, Cmd.OfAsync.eitherAsResult (fun _ -> service.GetMessage (if success then "true" else "false")) MessageReceived
    | MessageReceived (Ok msg) -> { model with Message = $"Got success response: {msg}" }, Cmd.none
    | MessageReceived (Result.Error error) -> { model with Message = $"Got server error: {error}" }, Cmd.none

[<ReactComponent>]
let IndexView (parentDispatch : ViewMsg -> unit) =
    let state, dispatch = React.useElmish(init, update, [| |])

    React.useEffectOnce(fun () ->
        parentDispatch (ProcessPageVisited SpeakEZPage)
    )

    React.fragment [
        Html.div [
            prop.className "flex flex-col p-4 space-y-4 transition-all duration-300 ease-in-out mx-auto max-w-screen-xl"
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
                    prop.className "card mx-auto bg-base-200 shadow-3xl rounded-3xl"
                    prop.children [
                        Html.div [
                            prop.className "flex flex-col md:flex-row"
                            prop.children [
                                Html.div [
                                    prop.className "w-full md:w-1/6 flex md:ml-4 mt-4 justify-center"
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
                                    prop.className "card-body w-full rounded-3xl"
                                    prop.children [
                                        Html.h1 [
                                            prop.className "card-title mx-auto"
                                            prop.text "Seasoned Experience, Modern Tools & Human Centered Design"
                                        ]
                                        Html.p [
                                            prop.text "SpeakEZ is an 'AI Refinery'. We combine battle-tested machine learning with leading-edge 'AI' tooling to build and deploy trustworthy decision support systems that remain fully in your sphere of control. If you imagine a high-performance, secure industrial-strength platform to support mission-critical knowledge work on the backplane and 'at the edge'- all with lower operational overhead and minimized environmental impact - SpeakEZ can be your guide in making that vision a reality."
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
                Html.div [
                    prop.className "flex flex-col md:flex-row gap-4 mx-auto"
                    prop.children [
                        Html.div [
                            prop.className "card w-full shadow bg-base-200 rounded-3xl"
                            prop.children [
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "Innovation without Compromise"
                                        ]
                                        Html.p [
                                            prop.text "After years of SaaS 'lock in' and surveillance capitalism, people are waking up to the inherent compromises in dealing with large service providers. SpeakEZ is different. We build security-first and privacy-first systems tailored to you and your business, with no shadowy agenda hidden in the terms of service."
                                        ]
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "Systm2"
                                        ]
                                        Html.p [
                                            prop.text "Rather than following the 'AI hype train', Systm2 uses a unique approach to develop multiple paths to a verifiable result. Your knowledge workers use System2 to make and shape optimal decisions while Systm2 provides efficient 'fan out' of solution options with clear user control points and full audit trail for compliance and transparency."
                                        ]
                                    ]
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "card w-full shadow bg-base-200 rounded-3xl"
                            prop.children [
                                Html.img [
                                    prop.src "img/Systm2_badge_580px.svg"
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "card w-full shadow bg-base-200 rounded-3xl"
                            prop.children [
                                Html.div [
                                    prop.className "card-body"
                                    prop.children [
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "Deep Systems"
                                        ]
                                        Html.p [
                                            prop.text "Everyone has seen LLMs grow 'shallow' through overuse. Systm2 is different - 'deep' by design - avoiding inherent weakness in language models by being sparse and targeted in their application. With our approach, users have the ability to 'teach' the system, and System2 can improve over time with guidance from users."
                                        ]
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "Deep Background"
                                        ]
                                        Html.p [
                                            prop.text "Many companies that are new to 'AI' are just now discovering some of the techniques that our engineers have been utilizing for decades."
                                        ]
                                        Html.p [
                                            prop.text "Houston Haynes, the founder of SpeakEZ, has more than 20 years of building intelligent systems in various forms. It's that experience that brings unique value to our collaboration with Rower Consulting."
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
                Html.div [
                    prop.className "card  mx-auto bg-base-200 shadow-3xl rounded-3xl"
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
                Html.div [
                    prop.className "flex flex-col md:flex-row gap-4 justify-end"
                    prop.children [
                        Html.button [
                            prop.className "btn btn-primary text-lg text-gray-200"
                            prop.onClick (fun e -> Router.goToUrl(e); parentDispatch (ProcessButtonClicked SpeakEZSignUpButton))
                            prop.href "/signup"
                            prop.text "Join Our Waitlist"
                        ]
                    ]
                ]
            ]
        ]
    ]