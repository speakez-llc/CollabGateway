﻿module CollabGateway.Client.Pages.Contact

open Feliz
open Elmish
open Fable.Core
open Thoth.Fetch
open Thoth.Json
open CollabGateway.Client.Server
open CollabGateway.Shared.API
open CollabGateway.Client.ViewMsg
open UseElmish


type IpResponse = { ip: string }

let getClientIP =
    async {
        let url = "https://api.ipify.org?format=json"
        let! response = Fetch.get(url) |> Async.AwaitPromise
        match response with
        | Ok json ->
            match Decode.Auto.fromString<IpResponse>(json) with
            | Ok ipResponse ->
                Browser.Dom.console.log($"Client IP: {ipResponse.ip}")
                return ipResponse.ip
            | Result.Error error ->
                Browser.Dom.console.log($"Error decoding IP: {error}")
                return $"Error decoding IP: {error}"
        | Result.Error error ->
            Browser.Dom.console.log($"Error fetching IP: {error}")
            return $"Error fetching IP: {error}"
    }

type private State = {
    ContactForm: ContactForm
    ResponseMessage: string
    InFormMessage: string
}

type private Msg =
    | UpdateName of string
    | UpdateEmail of string
    | UpdateMessageBody of string
    | UpdateClientIP of string
    | SubmitForm
    | FormSubmitted of ServerResult<string>
    | ParentDispatch of ViewMsg

let private init () =
    let initialContactForm = {
        Name = ""
        Email = ""
        MessageBody = ""
        ClientIP = ""
    }
    let initialState = { ContactForm = initialContactForm; InFormMessage = "Feel Free To Reach Out"; ResponseMessage = "" }
    initialState, Cmd.none

let private update (msg: Msg) (model: State) (parentDispatch: ViewMsg -> unit) : State * Cmd<Msg> =
    match msg with
    | UpdateClientIP ip -> { model with State.ContactForm.ClientIP = ip }, Cmd.none
    | UpdateName name -> { model with State.ContactForm.Name = name }, Cmd.none
    | UpdateEmail email -> { model with State.ContactForm.Email = email }, Cmd.none
    | UpdateMessageBody messageBody -> { model with State.ContactForm.MessageBody = messageBody }, Cmd.none
    | SubmitForm ->
        let cmd = Cmd.OfAsync.eitherAsResult (fun _ -> service.ProcessContactForm model.ContactForm) FormSubmitted
        model, cmd
    | FormSubmitted (Ok response) ->
        parentDispatch (ShowToast { Message = "Message sent"; Level = Success })
        { model with ResponseMessage = $"Got success response: {response}" }, Cmd.none
    | FormSubmitted (Result.Error ex) ->
        parentDispatch (ShowToast { Message = "Failed to send message"; Level = Error })
        { model with ResponseMessage = $"Failed to submit form: {ex.ToString()}" }, Cmd.none
    | ParentDispatch viewMsg ->
        parentDispatch viewMsg
        model, Cmd.none

[<ReactComponent>]
let IndexView (parentDispatch : ViewMsg -> unit) =
    let state, dispatch = React.useElmish((fun () -> init ()), (fun msg model -> update msg model parentDispatch), [| |])

    React.useEffectOnce(fun () ->
        let fetchIP () = async {
            let! ip = getClientIP
            dispatch (UpdateClientIP ip)
        }
        fetchIP () |> Async.StartImmediate
    )

    let handleButtonClick (_: Browser.Types.MouseEvent) =
        dispatch SubmitForm
        ()

    React.fragment [
        Html.div [
            prop.className "flex flex-col p-4 space-y-4 transition-opacity duration-900 ease-in-out w-full md:w-1/2 mx-auto max-w-screen-xl"
            prop.children [
                Html.h1 [
                    prop.className "text-2xl font-bold mb-4 mx-auto"
                    prop.text state.InFormMessage
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
                    prop.value state.ContactForm.Name
                    prop.onChange (fun (e: Browser.Types.Event) ->
                        let target = e.target :?> Browser.Types.HTMLInputElement
                        dispatch (UpdateName target.value))
                ]
                Html.input [
                    prop.className "rounded-lg h-10 w-2/3 lg:w-1/3 shadow bg-base-200 pl-2"
                    prop.placeholder "Email"
                    prop.autoComplete "Email"
                    prop.value state.ContactForm.Email
                    prop.onChange (fun (e: Browser.Types.Event) ->
                        let target = e.target :?> Browser.Types.HTMLInputElement
                        dispatch (UpdateEmail target.value))
                ]
                Html.textarea [
                    prop.className "rounded-lg h-32 w-full lg:w-1/2 shadow bg-base-200 p-2"
                    prop.placeholder "Your Message"
                    prop.value state.ContactForm.MessageBody
                    prop.onChange (fun (e: Browser.Types.Event) ->
                        let target = e.target :?> Browser.Types.HTMLTextAreaElement
                        dispatch (UpdateMessageBody target.value))
                ]
                Html.button [
                    prop.className "btn btn-primary h-10 w-full md:w-2/3 lg:w-1/3 text-gray-200 text-xl"
                    prop.text "Get In Touch!"
                    prop.onClick handleButtonClick
                ]
            ]
        ]
    ]