module CollabGateway.Shared.Events

open System
open CollabGateway.Shared.API

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


type ClientIPEvent = {
    Id: Guid
    UserClientIP: ClientIP
    UserGeoInfo: GeoInfo
}

type ContactFormEvent = {
    Id: Guid
    Form: ContactForm
}

type SignUpFormEvent = {
    Id: Guid
    Form: SignUpForm
}

type BaseEvent = {
    Id: Guid
}

type SessionEvent = {
    Id: Guid
    SessionID: SessionToken
}

type BaseEventCase =
    | HomePageVisited of BaseEvent
    | ProjectPageVisited of BaseEvent
    | DataPageVisited of BaseEvent
    | SignupPageVisited of BaseEvent
    | RowerPageVisited of BaseEvent
    | SpeakEZPageVisited of BaseEvent
    | ContactPageVisited of BaseEvent
    | PartnersPageVisited of BaseEvent
    | DataPolicyPageVisited of BaseEvent
    | HomeButtonClicked of BaseEvent
    | HomeProjectButtonClicked of BaseEvent
    | HomeSignUpButtonClicked of BaseEvent
    | ProjectButtonClicked of BaseEvent
    | ProjectDataButtonClicked of BaseEvent
    | ProjectSignUpButtonClicked of BaseEvent
    | DataButtonClicked of BaseEvent
    | DataSignUpButtonClicked of BaseEvent
    | SignUpButtonClicked of BaseEvent
    | SmartFormButtonClicked of BaseEvent
    | RowerButtonClicked of BaseEvent
    | RowerSignUpButtonClicked of BaseEvent
    | SpeakEZButtonClicked of BaseEvent
    | SpeakEZSignUpButtonClicked of BaseEvent
    | ContactButtonClicked of BaseEvent
    | PartnersButtonClicked of BaseEvent
    | RowerSiteButtonClicked of BaseEvent
    | CuratorSiteButtonClicked of BaseEvent
    | TableauSiteButtonClicked of BaseEvent
    | PowerBISiteButtonClicked of BaseEvent
    | ThoughtSpotSiteButtonClicked of BaseEvent
    | SpeakEZSiteButtonClicked of BaseEvent
    | DataPolicyAcceptButtonClicked of BaseEvent
    | DataPolicyDeclineButtonClicked of BaseEvent
    | DataPolicyResetButtonClicked of BaseEvent
    member this.Id =
        match this with
        | HomePageVisited e -> e.Id
        | ProjectPageVisited e -> e.Id
        | DataPageVisited e -> e.Id
        | SignupPageVisited e -> e.Id
        | RowerPageVisited e -> e.Id
        | SpeakEZPageVisited e -> e.Id
        | ContactPageVisited e -> e.Id
        | PartnersPageVisited e -> e.Id
        | HomeButtonClicked e -> e.Id
        | HomeProjectButtonClicked e -> e.Id
        | HomeSignUpButtonClicked e -> e.Id
        | ProjectButtonClicked e -> e.Id
        | ProjectDataButtonClicked e -> e.Id
        | ProjectSignUpButtonClicked e -> e.Id
        | DataButtonClicked e -> e.Id
        | DataSignUpButtonClicked e -> e.Id
        | SignUpButtonClicked e -> e.Id
        | SmartFormButtonClicked e -> e.Id
        | RowerButtonClicked e -> e.Id
        | RowerSignUpButtonClicked e -> e.Id
        | SpeakEZButtonClicked e -> e.Id
        | SpeakEZSignUpButtonClicked e -> e.Id
        | ContactButtonClicked e -> e.Id
        | PartnersButtonClicked e -> e.Id
        | RowerSiteButtonClicked e -> e.Id
        | CuratorSiteButtonClicked e -> e.Id
        | TableauSiteButtonClicked e -> e.Id
        | PowerBISiteButtonClicked e -> e.Id
        | ThoughtSpotSiteButtonClicked e -> e.Id
        | SpeakEZSiteButtonClicked e -> e.Id
        | DataPolicyAcceptButtonClicked e -> e.Id
        | DataPolicyDeclineButtonClicked e -> e.Id
        | DataPolicyResetButtonClicked e -> e.Id


type SessionEventCase =
    | UserSessionInitiated of SessionEvent
    | UserSessionResumed of SessionEvent
    | UserSessionClosed of SessionEvent
    | UserSessionEnded of SessionEvent
    | UserClientIPDetected of ClientIPEvent
    | UserClientIPUpdated of ClientIPEvent
    member this.Id =
        match this with
        | UserSessionInitiated e -> e.Id
        | UserSessionResumed e -> e.Id
        | UserSessionClosed e -> e.Id
        | UserSessionEnded e -> e.Id
        | UserClientIPDetected e -> e.Id
        | UserClientIPUpdated e -> e.Id
