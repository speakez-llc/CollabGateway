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

let private init () = { Message = "Next-Gen Decision Support For Your Security And Your Bottom Line" }, Cmd.none

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
                                            prop.text "Seasoned Experience, Modern Tools and Human Centered Design"
                                        ]
                                        Html.p [
                                            prop.text "SpeakEZ is an 'AI Refinery' that combines long-standing and leading-edge machine learning tools to build and deploy customized real-time decision support systems. Solutions are designed with security and privacy as first-class considerations, and are built to run efficiently in a variety of environments. If you're looking for a secure industrial-strength platform to support skilled, mission-critical knowledge work - all with lower operational overhead, SpeakEZ is the place to start."
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
                                            prop.text "Because of the .com and SaaS eras, we've become inured to divulging data as a devil's bargain with Corporate Surveillance conglomerates. Their real business is selling ads to you, or worse, selling your data to others. SpeakEZ is security-first and is solely focused on delivering high quality knowledge services."
                                        ]
                                        Html.h2 [
                                            prop.className "card-title"
                                            prop.text "Systm2"
                                        ]
                                        Html.p [
                                            prop.text "Rather than following the 'AI hype train' to the fastest answer, SpeakEZ's Systm2 platform quickly develops multiple paths to the best answer. It helps knowledge workers make optimal decisions on mission-critical tasks."
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
                                            prop.text "We have all experienced how LLMs become more shallow as they age. SpeakEZ's systems are 'deep' by design, by virtue of their ability to continue to grow and adapt with new data. "
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