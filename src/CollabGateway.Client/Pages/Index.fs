module CollabGateway.Client.Pages.Index

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

let private init () = { Message = "Experience the Next Evolution in Decision Support" }, Cmd.none

let private update (msg:Msg) (model:State) : State * Cmd<Msg> =
    match msg with
    | AskForMessage success -> model, Cmd.OfAsync.eitherAsResult (fun _ -> service.GetMessage (if success then "true" else "false")) MessageReceived
    | MessageReceived (Ok msg) -> { model with Message = $"Got success response: {msg}" }, Cmd.none
    | MessageReceived (Result.Error error) -> { model with Message = $"Got server error: {error}" }, Cmd.none

[<ReactComponent>]
let IndexView (parentDispatch : ViewMsg -> unit) =
    let state, dispatch = React.useElmish(init, update, [| |])

    React.useEffectOnce(fun () ->
        parentDispatch (ProcessPageVisited HomePage)
    )

    React.fragment [
        Html.div [
            prop.className "flex flex-col items-center justify-center space-y-4 mx-auto max-w-screen-xl"
            prop.children [
                // Header with the message
                Html.h1 [
                    prop.className "text-2xl mt-8 font-bold mx-auto"
                    prop.text state.Message
                ]
                // Animated SVG
                Html.div [
                    prop.className "card rounded-3xl shadow-lg overflow-hidden w-full"
                    prop.children [
                        Html.embed [
                            prop.src "/img/RowerCollab2.svg"
                            prop.type' "image/svg+xml"
                            prop.className "w-full h-auto"
                        ]
                    ]
                ]
                Html.div [
                    prop.className "card w-full bg-base-200 rounded-3xl"
                    prop.children [
                        Html.div [
                            prop.className "card-body"
                            prop.children [
                                Html.div [
                                    prop.className "flex flex-col space-y-4 mt-4 items-center md:items-end"
                                    prop.children [
                                        Html.p [
                                            prop.text "Rower Consulting & SpeakEZ Platform Services are proud to present a showcase that demonstrates return-of-value on existing analytics investments while establishing a clear path for bringing in high-leverage additions to your solution portfolio. Our fully functional portal site demonstrates in real time how this hybrid approach can help advance an organization's mission while protecting the bottom line."
                                        ]
                                        Html.p [
                                            prop.className "text-left"
                                            prop.text "A live demo of this scale may seem challenging to take in all at once. That's why, aside from the preamble in this gateway site, we are offering to provide a guided tour through this technology landscape. When you're ready to see the solution in action, sign up for our waitlist - and get ready to experience the future hands-on!"
                                        ]
                                        Html.div [
                                            prop.className "flex flex-col md:flex-row space-y-4 md:space-y-0 md:space-x-4 w-full md:w-auto"
                                            prop.children [
                                                Html.button [
                                                    prop.className "btn btn-secondary text-lg w-full md:w-auto text-gray-200"
                                                    prop.onClick (fun e -> parentDispatch (ProcessButtonClicked HomeProjectButton); Router.goToUrl(e))
                                                    prop.href "/project"
                                                    prop.text "Learn More"
                                                ]
                                                Html.button [
                                                    prop.className "btn btn-primary text-lg w-full md:w-auto text-gray-200"
                                                    prop.onClick (fun e ->  parentDispatch (ProcessButtonClicked HomeSignUpButton); Router.goToUrl(e))
                                                    prop.href "/signup"
                                                    prop.text "Join The Waitlist"
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