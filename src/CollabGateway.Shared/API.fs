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

type DataPolicyChoice =
    | Accepted
    | Declined
    | Unknown

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

type UnsubscribeStatus =
    | Open
    | Unsubscribed

type EmailStatus =
    | Open
    | Verified
    | Evicted


type Toast = {
    Message: string
    Level: AlertLevel
}
type UserSummaryAggregate = {
    StreamInitiated: EventDateTime
    DataPolicyDecision: (EventDateTime * DataPolicyChoice) option
    ContactFormSubmitted: (EventDateTime * ContactForm) option
    SignUpFormSubmitted: (EventDateTime * SignUpForm) option
    EmailStatus: (EventDateTime * EmailAddress * EmailStatus) list option
    UnsubscribeStatus: (EventDateTime * EmailAddress * UnsubscribeStatus) list option
}

type FullUserStreamProjection = (string * EventDateTime * obj option) list

type UserNameProjection = (string option * StreamToken) list

type Service = {
    EstablishStreamToken : StreamToken * EventDateTime -> Async<unit>
    EstablishUserClientIP : StreamToken * EventDateTime * ClientIP -> Async<unit>
    AppendUnsubscribeStatus : StreamToken * EventDateTime * ValidationToken * EmailAddress * UnsubscribeStatus -> Async<unit>
    AppendEmailStatus : StreamToken * EventDateTime * ValidationToken * EmailAddress * EmailStatus -> Async<unit>
    FlagWebmailDomain : string -> Async<bool>
    ProcessContactForm : StreamToken * EventDateTime * ContactForm -> Async<string>
    ProcessStreamClose : StreamToken * EventDateTime -> Async<unit>
    ProcessPageVisited : StreamToken * EventDateTime * PageName -> Async<unit>
    ProcessButtonClicked : StreamToken * EventDateTime * ButtonName -> Async<unit>
    ProcessSmartForm : StreamToken * EventDateTime * SmartFormRawContent -> Async<SignUpForm>
    ProcessSignUpForm : StreamToken * EventDateTime * SignUpForm -> Async<string>
    RetrieveDataPolicyChoice : StreamToken -> Async<DataPolicyChoice>
    RetrieveEmailStatus : StreamToken -> Async<(EventDateTime * EmailAddress * EmailStatus) list>
    RetrieveUnsubscribeStatus : StreamToken -> Async<(EventDateTime * EmailAddress * UnsubscribeStatus) list>
    RetrieveUserSummary : StreamToken -> Async<UserSummaryAggregate>
    RetrieveFullUserStream : StreamToken -> Async<FullUserStreamProjection>
    RetrieveAllUserNames : unit -> Async<UserNameProjection>
}
with
    static member RouteBuilder _ m = $"/api/service/%s{m}"