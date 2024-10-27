module CollabGateway.Shared.API

open System
open Marten.Events.Aggregation
open Marten.Events.Projections

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
type UserSummaryAggregate = {
    UserName: string option
    UserEmail: string option
    StreamInitiated: EventDateTime
    DataPolicyDecision: DataPolicyChoice * EventDateTime option
    ContactFormSubmitted: EventDateTime option
    ContactForm: ContactForm option
    SignUpFormSubmitted: EventDateTime option
    SignUpForm: SignUpForm option
}

type FullUserStreamProjection() =
    inherit SingleStreamProjection<(string * EventDateTime * obj option) list>()
    member val State: (string * EventDateTime * obj option) list = [] with get, set

type UserNameProjection() =
    inherit MultiStreamProjection<string option, StreamToken>()
    member val State: (string option * StreamToken) list = [] with get, set

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
    RetrieveUserSummary : StreamToken -> Async<UserSummaryAggregate>
    RetrieveFullUserStream : StreamToken -> Async<FullUserStreamProjection>
    RetrieveAllUserNames : unit -> Async<UserNameProjection>
}
with
    static member RouteBuilder _ m = sprintf "/api/service/%s" m