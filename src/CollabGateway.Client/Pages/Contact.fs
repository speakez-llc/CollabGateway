module CollabGateway.Client.Pages.Contact

open Fable.React.Props
open Feliz
open Elmish
open UseElmish
open Fable.Form.Simple
open CollabGateway.Client.Server
open CollabGateway.Shared.API
open CollabGateway.Shared.Types

type private State = {
    Form: Form.View.Model<ContactForm>
    Message: string
}

type private Msg =
    | AskForMessage of bool
    | MessageReceived of ServerResult<string>
    | FormUpdated of Form.View.Model<ContactForm>

let validateName name  : Result<string,'a> =
    if System.String.IsNullOrWhiteSpace(name) then Error "Name is required"
    else Ok name

let validateEmail email : Result<string,'a> =
    match EmailAddress.tryParse email with
    | Ok _ -> Ok email
    | _ -> failwith "todo"

let validateMessage message  : Result<string,'a> =
    if System.String.IsNullOrWhiteSpace(message) then Error "Message is required"
    else Ok message

let contactForm =
    let nameField : Form.Form<ContactForm, string, _> =
        Form.textField {
            Parser = Ok
            Value = fun values -> values.Name
            Update = fun newValue values -> { values with Name = newValue }
            Error = fun _ -> None
            Attributes = {
                Label = "Name"
                Placeholder = "Your name"
                HtmlAttributes = [ prop.autoComplete "name" ]
            }
        }

    let emailField  : Form.Form<ContactForm, string, _> =
        Form.textField {
            Parser = validateEmail
            Value = fun values -> values.Email
            Update = fun newValue values -> { values with Email = newValue }
            Error = fun _ -> None
            Attributes = {
                Label = "Email"
                Placeholder = "your.email@example.com"
                HtmlAttributes = [ prop.autoComplete "email" ]
            }
        }

    let messageField : Form.Form<ContactForm, string, _> =
        Form.textField {
            Parser = Ok
            Value = fun values -> values.Message
            Update = fun newValue values -> { values with Message = newValue }
            Error = fun _ -> None
            Attributes = {
                Label = "Message"
                Placeholder = "Your message"
                HtmlAttributes = [ prop.autoComplete "off" ]
            }
        }

    let onSubmit = fun nameField emailField messageField -> AskForMessage true

    Form.succeed onSubmit
    |> Form.append nameField
    |> Form.append emailField
    |> Form.append messageField

let private init () =
    { Form = Form.View.idle { Name = ""; Email = ""; Message = "" }
      Message = "Feel free to send us a message" }, Cmd.none

let private update (msg: Msg) (model: State) : State * Cmd<Msg> =
    match msg with
    | AskForMessage success -> model, Cmd.OfAsync.eitherAsResult (fun _ -> service.GetMessage success) MessageReceived
    | MessageReceived (Ok msg) -> { model with Message = $"Got success response: {msg}" }, Cmd.none
    | MessageReceived (Error error) -> { model with Message = $"Got server error: {error}" }, Cmd.none
    | FormUpdated form -> { model with Form = form }, Cmd.none

[<ReactComponent>]
let IndexView () =
    let state, dispatch = React.useElmish(init, update, [| |])

    let onSubmit form =
        match form with
        | Ok formData ->
            // All fields are valid, send formData to the backend
            dispatch (AskForMessage true)
        | Error _ ->
            // One or more fields are invalid
            dispatch (AskForMessage false)

    React.fragment [
        Html.div [
            prop.className "flex flex-col p-4 space-y-4 transition-opacity duration-900 ease-in-out w-4/5 mx-auto max-w-screen-xl"
            prop.children [
                Html.h1 [
                    prop.className "text-2xl font-bold mb-4 mx-auto"
                    prop.text state.Message
                ]
                Form.View.render state.Form (fun form -> dispatch (FormUpdated form))
                Html.button [
                    prop.className "btn btn-primary h-10 w-1/2 md:w-1/4"
                    prop.text "Get In Touch"
                    prop.onClick (fun _ -> onSubmit state.Form)
                ]
            ]
        ]
    ]