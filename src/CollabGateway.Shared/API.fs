module CollabGateway.Shared.API

type Message = {
    role: string
    content: string
}

type OpenAIRequest = {
    messages: Message list
    max_tokens: int
    temperature: float
    frequency_penalty: float
    presence_penalty: float
    top_p: float
    stop: string option
}

type ContactForm = {
    Name : string
    Email : string
    Message : string
    ClientIP: string
}

type SignUpForm = {
    Name : string
    Email : string
    JobTitle : string
    Phone : string
    Department : string
    Company : string
    StreetAddress1 : string
    StreetAddress2 : string
    City : string
    StateProvince : string
    PostCode : string
    Country : string
    ClientIP: string
}

type AlertLevel =
    | Success
    | Error
    | Warning
    | Info

type Toast = {
    Message: string
    Level: AlertLevel
}

type Service = {
    GetMessage : bool -> Async<string>
    SendEmailMessage : ContactForm -> Async<string>
}
with
    static member RouteBuilder _ m = sprintf "/api/service/%s" m