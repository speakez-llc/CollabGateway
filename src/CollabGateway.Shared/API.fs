module CollabGateway.Shared.API

open System

type SessionToken = Guid

type ChatMessage = {
    role: string
    content: string
}

type OpenAIRequest = {
    messages: ChatMessage list
    max_tokens: int
    temperature: float
    frequency_penalty: float
    presence_penalty: float
    top_p: float
    stop: string option
}

type ClientIP = string

type ContactForm = {
    Name : string
    Email : string
    MessageBody : string
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
    ProcessContactForm : ContactForm -> Async<string>
    ProcessSessionToken : SessionToken -> Async<unit>
    ProcessPageVisited : Guid * string -> Async<unit>
    ProcessButtonClicked : Guid * string -> Async<unit>
}
with
    static member RouteBuilder _ m = sprintf "/api/service/%s" m