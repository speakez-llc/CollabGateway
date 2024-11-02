module CollabGateway.Shared.API

open System

type DataPolicyChoice =
    | Accepted
    | Declined
    | Unknown

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
    | ActivityPage

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
    | ActivityButton

type StreamToken = Guid
type EventDateTime = DateTime
type EventToken = Guid
type ValidationToken = Guid
type ClientIP = string
type EmailAddress = string
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

type SubscribeStatus =
    | Open
    | Unsubscribed

type EmailStatus =
    | Open
    | Verified
    | Evicted


type Toast = {
    Index: int
    Message: string
    Level: AlertLevel
}
type UserSummaryAggregate = {
    StreamInitiated: EventDateTime
    DataPolicyDecision: (EventDateTime * DataPolicyChoice) option
    ContactFormSubmitted: (EventDateTime * ContactForm) option
    SignUpFormSubmitted: (EventDateTime * SignUpForm) option
    EmailStatus: (EventDateTime * EmailAddress * EmailStatus) list option
    SubscribeStatus: (EventDateTime * EmailAddress * SubscribeStatus) list option
}

type FullUserStreamProjection = (string * EventDateTime * obj option) list

type UserNameProjection = (string option * StreamToken) list

type Service = {
    GetMessage : string -> Async<string>
    EstablishStreamToken : EventDateTime * StreamToken -> Async<unit>
    EstablishUserClientIP : EventDateTime * StreamToken * ClientIP -> Async<unit>
    AppendEmailStatus : EventDateTime * StreamToken * ValidationToken * EmailAddress * EmailStatus -> Async<unit>
    AppendUnsubscribeStatus : EventDateTime * StreamToken * ValidationToken * EmailAddress * SubscribeStatus -> Async<unit>
    FlagWebmailDomain : string -> Async<bool>
    ProcessContactForm : EventDateTime * StreamToken * ContactForm -> Async<string>
    ProcessStreamClose : EventDateTime * StreamToken -> Async<unit>
    ProcessPageVisited : EventDateTime * StreamToken * PageName -> Async<unit>
    ProcessButtonClicked : EventDateTime * StreamToken * ButtonName -> Async<unit>
    ProcessSmartForm : EventDateTime * StreamToken * SmartFormRawContent -> Async<SignUpForm>
    ProcessSignUpForm : EventDateTime * StreamToken * SignUpForm -> Async<string>
    RetrieveDataPolicyChoice : StreamToken -> Async<DataPolicyChoice>
    RetrieveEmailStatus : StreamToken -> Async<(EventDateTime * EmailAddress * EmailStatus) list option>
    RetrieveUnsubscribeStatus : StreamToken -> Async<(EventDateTime * EmailAddress * SubscribeStatus) list option>
    RetrieveUserSummary : StreamToken -> Async<UserSummaryAggregate>
    RetrieveFullUserStream : StreamToken -> Async<FullUserStreamProjection>
    RetrieveAllUserNames : unit -> Async<UserNameProjection>
}
with
    static member RouteBuilder _ m = $"/api/service/%s{m}"