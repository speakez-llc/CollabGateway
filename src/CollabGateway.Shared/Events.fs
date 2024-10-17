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


type ContactFormEvent = {
    SessionID: SessionToken
    Form: ContactForm
    UserGeoInfo: GeoInfo
}

type SignUpFormEvent = {
    Id: Guid
    SessionID: SessionToken
    Form: SignUpForm
}

type SessionTokenEvent = {
    Id: Guid
    SessionID: SessionToken
}

type EventLabel =
    | UserSessionInitiated of SessionTokenEvent
    | UserSessionResumed of SessionTokenEvent
    | UserSessionEnded of SessionTokenEvent
    | TosAccepted of SessionToken
    | TosRejected of SessionToken
    | TosReset of SessionToken
    | ThemeChanged of SessionToken
    | IndexPageVisited of SessionToken
    | IndexLearnMoreClicked of SessionToken
    | IndexJoinWaitlistClicked of SessionToken
    | ProductPageVisited of SessionToken
    | ProductAboutTheDataClicked of SessionToken
    | ProductJoinWaitlistClicked of SessionToken
    | DataPageVisited of SessionToken
    | DataJoinWaitlistClicked of SessionToken
    | SignUpPageVisited of SessionToken
    | SmartFormRequestSubmitted of string
    | SmartFormResponseReceived of SignUpForm
    | SmartFormErrorGenerated of string
    | SignUpFormSubmitted of SignUpFormEvent
    | SignUpFormResponseGenerated of string
    | RowerPageVisited of SessionToken
    | RowerJoinWaitlistClicked of SessionToken
    | SpeakEZPageVisited of SessionToken
    | SpeakEZJoinWaitlistClicked of SessionToken
    | ContactPageVisited of SessionToken
    | ContactFormSubmitted of GeoInfo
    | ContactFormResponseReceived of string
    | PartnersPageVisited of SessionToken


