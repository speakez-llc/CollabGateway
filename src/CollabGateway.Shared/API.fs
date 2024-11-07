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
    | OverviewPage
    | UserSummaryPage

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
    | OverviewButton
    | UserSummaryButton

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

type UserTopLineSummary = {
    StreamToken: StreamToken
    UserName: string option
    Email: string option
    EventCount: int
}

type UserStreamProjection = UserTopLineSummary list

type Grain =
    | Month
    | Week
    | Day
    | Hour
    | Minute

type IntervalStart = DateTime
type IntervalEnd = DateTime

type OverviewTotals = {
    TotalNewUserStreams: int
    TotalDataPolicyDeclines: int
    TotalContactFormsUsed: int
    TotalSmartFormUsers: int
    TotalSignUpFormsUsed: int
    TotalEmailVerifications: int
    TotalEmailUnsubscribes: int
    TotalUsersWhoReachedSmartFormLimit: int
}

type OverviewTotalsProjection = {
    IntervalStart: IntervalStart option
    IntervalEnd: IntervalEnd option
    OverviewTotals: OverviewTotals
}

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
    RetrieveSmartFormSubmittedCount: StreamToken -> Async<int>
    ProcessSignUpForm : EventDateTime * StreamToken * SignUpForm -> Async<string>
    RetrieveDataPolicyChoice : StreamToken -> Async<DataPolicyChoice>
    RetrieveEmailStatus : StreamToken -> Async<(EventDateTime * EmailAddress * EmailStatus) list option>
    RetrieveUnsubscribeStatus : StreamToken -> Async<(EventDateTime * EmailAddress * SubscribeStatus) list option>
    RetrieveUserSummary : StreamToken -> Async<UserSummaryAggregate>
    RetrieveFullUserStream : StreamToken -> Async<FullUserStreamProjection>
    RetrieveAllUserNames : unit -> Async<UserStreamProjection>
    RetrieveOverviewTotals : (int * Grain) option -> Async<OverviewTotalsProjection list>
    RetrieveClientIPLocations : unit -> Async<(string * float * float * int) list>
    RetrieveVerifiedEmailDomains : unit -> Async<(string * int) list>
}
with
    static member RouteBuilder _ m = $"/api/service/%s{m}"