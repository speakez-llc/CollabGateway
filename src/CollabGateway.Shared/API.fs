module CollabGateway.Shared.API

open System

type DataPolicyChoice =
    | Accepted
    | Declined
    | Unknown

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
    | ContactActivityButton
    | SignUpActivityButton
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


type StreamToken = Guid
type EventDateTime = DateTime
type SubscriptionToken = Guid
type VerificationToken = Guid
type ClientIP = string
type UserName = string
type EmailAddress = string
type IpResponse = { ip: string }
type SmartFormRawContent = string

type ChatMessage = {
    role: string
    content: string
}

type OllamaPropertiesType = {
    ``type``: string
}

type OllamaFormat = {
    ``type``: string
    properties: Map<string, OllamaPropertiesType>
    required: string list
}

type OllamaAddressRequest = {
    model: string
    messages: ChatMessage list
    stream: bool
    format: OllamaFormat
}


type ContactForm = {
    Name : UserName
    Email : EmailAddress
    MessageBody : string
}

type SignUpForm = {
    Name : UserName
    Email : EmailAddress
    JobTitle : string
    Phone : string
    Department : string
    Industry : string
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
    | Hidden

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
    EmailStatus: (EventDateTime * EmailAddress * EmailStatus) option
    SubscribeStatus: (EventDateTime * EmailAddress * SubscribeStatus) option
}

type Location = {
    country: string
    region: string
    city: string
    lat: float
    lng: float
    postalCode: string
    timezone: string
    geonameId: int
}

type AsInfo = {
    asn: int
    name: string
    route: string
    domain: string
    ``type``: string
}

type GeoInfo = {
    ip: string
    location: Location
    ``as``: AsInfo
    isp: string
}

[<RequireQualifiedAccess>]
type FullUserStreamEvent =
    | UserStreamInitiated of EventDateTime
    | UserStreamResumed of EventDateTime
    | UserStreamClosed of EventDateTime
    | UserStreamEnded of EventDateTime
    | DataPolicyAccepted of EventDateTime
    | DataPolicyDeclined of EventDateTime
    | DataPolicyReset of EventDateTime
    | HomePageVisited of EventDateTime
    | ProjectPageVisited of EventDateTime
    | DataPageVisited of EventDateTime
    | SignupPageVisited of EventDateTime
    | RowerPageVisited of EventDateTime
    | SpeakEZPageVisited of EventDateTime
    | ContactPageVisited of EventDateTime
    | PartnersPageVisited of EventDateTime
    | DataPolicyPageVisited of EventDateTime
    | SummaryActivityPageVisited of EventDateTime
    | HomeButtonClicked of EventDateTime
    | HomeProjectButtonClicked of EventDateTime
    | HomeSignUpButtonClicked of EventDateTime
    | ProjectButtonClicked of EventDateTime
    | ProjectDataButtonClicked of EventDateTime
    | ProjectSignUpButtonClicked of EventDateTime
    | DataButtonClicked of EventDateTime
    | DataSignUpButtonClicked of EventDateTime
    | SignUpButtonClicked of EventDateTime
    | SmartFormButtonClicked of EventDateTime
    | SmartFormSubmittedButtonClicked of EventDateTime
    | RowerButtonClicked of EventDateTime
    | RowerSignUpButtonClicked of EventDateTime
    | SpeakEZButtonClicked of EventDateTime
    | SpeakEZSignUpButtonClicked of EventDateTime
    | ContactButtonClicked of EventDateTime
    | PartnersButtonClicked of EventDateTime
    | RowerSiteButtonClicked of EventDateTime
    | CuratorSiteButtonClicked of EventDateTime
    | TableauSiteButtonClicked of EventDateTime
    | PowerBISiteButtonClicked of EventDateTime
    | ThoughtSpotSiteButtonClicked of EventDateTime
    | SpeakEZSiteButtonClicked of EventDateTime
    | DataPolicyAcceptButtonClicked of EventDateTime
    | DataPolicyDeclineButtonClicked of EventDateTime
    | DataPolicyResetButtonClicked of EventDateTime
    | SummaryActivityButtonClicked of EventDateTime
    | ContactFormSubmitted of EventDateTime * ContactForm
    | SignUpFormSubmitted of EventDateTime * SignUpForm
    | SmartFormSubmitted of EventDateTime * string
    | SmartFormResultReturned of EventDateTime * SignUpForm
    | EmailStatusAppended of EventDateTime * EmailStatus
    | SubscribeStatusAppended of EventDateTime * SubscribeStatus
    | UserClientIPDetected of EventDateTime * GeoInfo
    | UserClientIPUpdated of EventDateTime * GeoInfo
    | ContactActivityButtonClicked of EventDateTime
    | SignUpActivityButtonClicked of EventDateTime
    | OverviewButtonClicked of EventDateTime
    | OverviewPageVisited of EventDateTime
    | UserSummaryButtonClicked of EventDateTime
    | UserSummaryPageVisited of EventDateTime

type FullUserStreamProjection = FullUserStreamEvent list

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

type EventEnvelope = {
    StreamId: StreamToken
    Timestamp: DateTimeOffset
    Data: obj
}

type GicsTaxonomy = {
    SectorCode: string
    SectorName: string
    IndustryGroupCode: string
    IndustryGroupName: string
    IndustryCode: string
    IndustryName: string
    SubIndustryCode: string
    SubIndustryName: string
}

type Service = {
    GetMessage : string -> Async<string>
    EstablishStreamToken : EventDateTime * StreamToken -> Async<unit>
    EstablishUserClientIP : EventDateTime * StreamToken * ClientIP -> Async<unit>
    AppendEmailStatus : EventDateTime * StreamToken * VerificationToken * EmailAddress * EmailStatus -> Async<unit>
    AppendUnsubscribeStatus : EventDateTime * StreamToken * SubscriptionToken * EmailAddress * SubscribeStatus -> Async<unit>
    FlagWebmailDomain : string -> Async<bool>
    ProcessContactForm : EventDateTime * StreamToken * ContactForm -> Async<string>
    ProcessStreamClose : EventDateTime * StreamToken -> Async<unit>
    ProcessPageVisited : EventDateTime * StreamToken * PageName -> Async<unit>
    ProcessButtonClicked : EventDateTime * StreamToken * ButtonName -> Async<unit>
    ProcessSmartForm : EventDateTime * StreamToken * SmartFormRawContent -> Async<SignUpForm>
    RetrieveSmartFormSubmittedCount: StreamToken -> Async<int>
    ProcessSignUpForm : EventDateTime * StreamToken * SignUpForm -> Async<string>
    RetrieveDataPolicyChoice : StreamToken -> Async<DataPolicyChoice>
    RetrieveEmailStatus : StreamToken -> Async<(EventDateTime * EmailAddress * EmailStatus) option>
    RetrieveUnsubscribeStatus : StreamToken -> Async<(EventDateTime * EmailAddress * SubscribeStatus) option>
    RetrieveLatestSubscriptionToken : StreamToken * EmailAddress -> Async<SubscriptionToken>
    RetrieveLatestVerificationToken : StreamToken * EmailAddress -> Async<VerificationToken>
    RetrieveContactFormSubmitted : StreamToken -> Async<bool>
    RetrieveSignUpFormSubmitted : StreamToken -> Async<bool>
    RetrieveUserSummary : StreamToken -> Async<UserSummaryAggregate>
    RetrieveFullUserStream : StreamToken -> Async<FullUserStreamProjection>
    RetrieveAllUserNames : unit -> Async<UserStreamProjection>
    RetrieveOverviewTotals : (int * Grain) option -> Async<OverviewTotalsProjection list>
    RetrieveClientIPLocations : unit -> Async<(string * float * float * int) list>
    RetrieveVerifiedEmailDomains : unit -> Async<(string * int) list>
    SendEmailVerification: UserName * EmailAddress * VerificationToken * SubscriptionToken -> Async<unit>
    CheckIfAdmin: StreamToken -> Async<bool>
    LoadGicsTaxonomy: unit -> Async<GicsTaxonomy[]>
    RetrieveCountOfEmptyStreams : unit -> Async<int>
    ArchiveEmptyStreams: unit -> Async<unit>
    ProcessSemanticSearch: string -> Async<GicsTaxonomy array>
}
with
    static member RouteBuilder _ m = $"/api/service/%s{m}"