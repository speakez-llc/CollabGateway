module CollabGateway.Client.Pages.Contact

open Feliz
open Elmish
open UseElmish
open CollabGateway.Client.Server
open CollabGateway.Shared.API
open Fable.Form.Simple
open Fable.Form.Simple.Bulma

type Model = {
    Form: Form.View.Model<ContactForm>
    Message: string
}

type private Msg =
    | FormChanged of Form.View.Model<ContactForm>
    | FormSubmitted of ServerResult<string>

let private init () =
    { Form = Form.View.idle { Name = ""; Email = ""; Message = "" }
      Message = "This is the Contact Form page" }, Cmd.none

let private update (msg:Msg) (model:Model) : Model * Cmd<Msg> =
    match msg with
    | FormChanged form -> { model with Form = form }, Cmd.none
    | FormSubmitted (Ok msg) -> { model with Message = $"Got success response: {msg}" }, Cmd.none
    | FormSubmitted (Error error) -> { model with Message = $"Got server error: {error}" }, Cmd.none

let private form: Form.Form<ContactForm, Msg, _> =
    let nameField =
        Form.textField {
            Parser = Ok
            Value = fun f -> f.Name
            Update = fun v f -> { f with Name = v }
            Error = fun _ -> None
            Attributes = {
                Label = "Full Name"
                Placeholder = "Your full name"
                HtmlAttributes = [ prop.autoComplete "name" ]
            }
        }

    let emailField =
        Form.textField {
            Parser = EmailAddress.tryParse
            Value = fun f -> f.Email
            Update = fun v f -> { f with Email = v }
            Error = fun _ -> None
            Attributes = {
                Label = "Email"
                Placeholder = "some@email.com"
                HtmlAttributes = [ prop.autoComplete "email" ]
            }
        }

    let messageField =
        Form.textareaField {
            Parser = Ok
            Value = fun f -> f.Message
            Update = fun v f -> { f with Message = v }
            Error = fun _ -> None
            Attributes = {
                Label = "Message"
                Placeholder = "Your message"
                HtmlAttributes = [ prop.autoComplete "off" ]
            }
        }

    let onSubmit (form: ContactForm) =
        FormSubmitted (Ok "Form submitted successfully")


    Form.succeed (fun form -> onSubmit form)
    |> Form.append nameField
    |> Form.append emailField
    |> Form.append messageField

[<ReactComponent>]
let IndexView () =
    let state, dispatch = React.useElmish(init, update, [| |])

    React.fragment [
        Html.div [
            prop.className "flex flex-col p-4 space-y-4 transition-opacity duration-900 ease-in-out w-4/5 mx-auto"
            prop.children [
                Html.h1 [
                    prop.className "text-2xl font-bold mb-4 mx-auto"
                    prop.text state.Message
                ]
                Form.View.asHtml {
                    Dispatch = dispatch
                    OnChange = FormChanged
                    Action = Form.View.Action.SubmitOnly "Submit"
                    Validation = Form.View.ValidateOnSubmit
                } form state.Form
            ]
        ]
    ]