module CollabGateway.Shared.API

open Fable.Form.Simple

type ContactForm = {
    Name: string
    Email: string
    Message: string
}

type Model = Form.View.Model<ContactForm>

type Service = {
    GetMessage : bool -> Async<string>
    SubmitContactForm: ContactForm -> Async<string>
}
with
    static member RouteBuilder _ m = sprintf "/api/service/%s" m