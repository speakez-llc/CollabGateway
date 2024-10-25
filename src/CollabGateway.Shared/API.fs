module CollabGateway.Shared.API

open System

type PageName =
    | DataPolicyPage
    | HomePage
    | ProjectPage
    | CMSDataPage
    | SignUpPage
    | RowerPage
    | SpeakEZPage
    | ContactPage
    | PartnersPage

type ButtonName =
    | DataPolicyAcceptButton
    | DataPolicyDeclineButton
    | DataPolicyResetButton
    | HomeButton
    | HomeProjectButton
    | HomeSignUpButton
    | ProjectButton
    | ProjectDataButton
    | ProjectSignUpButton
    | CMSDataButton
    | CMSDataSignUpButton
    | SignUpButton
    | RowerButton
    | RowerSignUpButton
    | SpeakEZButton
    | SpeakEZSignUpButton
    | ContactButton
    | PartnersButton
    | RowerSiteButton
    | CuratorSiteButton
    | TableauSiteButton
    | PowerBISiteButton
    | ThoughtSpotSiteButton
    | SpeakEZSiteButton

type DataPolicyChoice =
    | Accepted
    | Declined
    | Unknown

type StreamToken = Guid
type EventDateTime = DateTime
type ClientIP = string
type IpResponse = { ip: string }
type SmartFormRawContent = string

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


type ContactForm = {
    Name : string
    Email : string
    MessageBody : string
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
    RetrieveDataPolicyChoice : StreamToken -> Async<DataPolicyChoice>
    ProcessStreamToken : StreamToken * EventDateTime -> Async<unit>
    ProcessStreamClose : StreamToken * EventDateTime -> Async<unit>
    ProcessPageVisited : StreamToken * EventDateTime * PageName -> Async<unit>
    ProcessButtonClicked : StreamToken * EventDateTime * ButtonName -> Async<unit>
    ProcessUserClientIP : StreamToken * EventDateTime * ClientIP -> Async<unit>
    ProcessContactForm : StreamToken * EventDateTime * ContactForm -> Async<string>
    ProcessSmartForm : StreamToken * EventDateTime * SmartFormRawContent -> Async<SignUpForm>
    ProcessSignUpForm : StreamToken * EventDateTime * SignUpForm -> Async<string>
}
with
    static member RouteBuilder _ m = sprintf "/api/service/%s" m