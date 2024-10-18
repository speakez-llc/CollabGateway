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
    Id: Guid
    ip: string
    location: Location
    ``as``: AsInfo
    isp: string
}


type ContactFormEvent = {
    Id: Guid
    Form: ContactForm
    UserGeoInfo: GeoInfo
}

type SignUpFormEvent = {
    Id: Guid
    SessionID: SessionToken
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
    | IndexPageVisited of BaseEvent
    | ProjectPageVisited of BaseEvent
    | DataPageVisited of BaseEvent
    | SignupPageVisited of BaseEvent
    | RowerPageVisited of BaseEvent
    | SpeakEZPageVisited of BaseEvent
    | ContactPageVisited of BaseEvent
    | PartnersPageVisited of BaseEvent
    member this.Id =
        match this with
        | IndexPageVisited e -> e.Id
        | ProjectPageVisited e -> e.Id
        | DataPageVisited e -> e.Id
        | SignupPageVisited e -> e.Id
        | RowerPageVisited e -> e.Id
        | SpeakEZPageVisited e -> e.Id
        | ContactPageVisited e -> e.Id
        | PartnersPageVisited e -> e.Id

type SessionEventCase =
    | UserSessionInitiated of SessionEvent
    | UserSessionResumed of SessionEvent
    | UserSessionClosed of SessionEvent
    | UserSessionEnded of SessionEvent
    member this.Id =
        match this with
        | UserSessionInitiated e -> e.Id
        | UserSessionResumed e -> e.Id
        | UserSessionClosed e -> e.Id
        | UserSessionEnded e -> e.Id
