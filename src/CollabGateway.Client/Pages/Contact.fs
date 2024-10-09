module CollabGateway.Client.Pages.Contact

open Feliz
open Elmish
open CollabGateway.Client.Server
open CollabGateway.Shared.API
open UseElmish
open CollabGateway.Client.ViewMsg

type private State = {
    Message : string
}

type private Msg =
    | AskForMessage of bool
    | MessageReceived of ServerResult<string>

let private init () = { Message = "Feel Free To Reach Out" }, Cmd.none

let private update (msg:Msg) (model:State) : State * Cmd<Msg> =
    match msg with
    | AskForMessage success -> model, Cmd.OfAsync.eitherAsResult (fun _ -> service.GetMessage success) MessageReceived
    | MessageReceived (Ok msg) -> { model with Message = $"Got success response: {msg}" }, Cmd.none
    //| MessageReceived (Error error) -> { model with Message = $"Got server error: {error}" }, Cmd.none
    //| SendToast toast -> model, Cmd.OfMsg (ShowToast toast)

[<ReactComponent>]
let IndexView (parentDispatch : ViewMsg -> unit) =
    let state, dispatch = React.useElmish(init, update, [| |])

    let handleButtonClick event toast =
        parentDispatch (ShowToast toast )
        ()

    React.fragment [
        Html.div [
            prop.className "flex flex-col p-4 space-y-4 transition-opacity duration-900 ease-in-out w-full md:w-1/2 mx-auto max-w-screen-xl"
            prop.children [
                // Header with the message
                Html.h1 [
                    prop.className "text-2xl font-bold mb-4 mx-auto"
                    prop.text state.Message
                ]
                Html.div [
                    prop.className "card mx-auto bg-base-200 w-full md:w-4/5 mx-auto rounded-3xl"
                    prop.children [
                        Html.div [
                            prop.className "p-4 m-2 card-body mx-auto"
                            prop.text "If you're not ready to sign up on our waitlist, you can still let us know you're interested. Use the form below and someone at Rower will respond. We're always happy to hear from you."
                        ]
                    ]
                ]
                Html.input [
                    prop.className "rounded-lg h-10 w-2/3 lg:w-1/3 shadow bg-base-200 pl-2"
                    prop.placeholder "Name"
                    prop.autoComplete "Name"
                ]
                Html.input [
                    prop.className "rounded-lg h-10 w-2/3 lg:w-1/3 shadow bg-base-200 pl-2"
                    prop.placeholder "Email"
                    prop.autoComplete "Email"
                ]
                Html.textarea [
                    prop.className "rounded-lg h-32 w-full lg:w-1/2 shadow bg-base-200 p-2"
                    prop.placeholder "Your Message"
                ]
                Html.button [
                    prop.className "btn btn-primary h-10 w-full md:w-2/3 lg:w-1/3 text-gray-200 text-xl"
                    prop.text "Get In Touch!"
                    prop.onClick (fun event -> handleButtonClick event { Message="Message sent"; Level=Success })
                ]
            ]
        ]
    ]