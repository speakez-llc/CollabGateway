module CollabGateway.Shared.API

type ContactForm = {
    Name : string
    Email : string
    Message : string
    ClientIP: string
}

type Service = {
    GetMessage : bool -> Async<string>
    SendEmailMessage : ContactForm -> Async<string>
}
with
    static member RouteBuilder _ m = sprintf "/api/service/%s" m