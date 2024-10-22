﻿module CollabGateway.Shared.Events

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
    TimeStamp: DateTime
    UserClientIP: ClientIP
    UserGeoInfo: GeoInfo
}

type ContactFormEvent = {
    Id: Guid
    TimeStamp: DateTime
    Form: ContactForm
}

type SignUpFormEvent = {
    Id: Guid
    TimeStamp: DateTime
    Form: SignUpForm
}

type BaseEvent = {
    Id: Guid
    TimeStamp: DateTime
}

type StreamEvent = {
    Id: Guid
    StreamID: StreamToken
    TimeStamp: DateTime
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
        | DataPolicyPageVisited e -> e.Id
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
    | UserStreamInitiated of StreamEvent
    | UserStreamResumed of StreamEvent
    | UserStreamClosed of StreamEvent
    | UserStreamEnded of StreamEvent
    | UserClientIPDetected of ClientIPEvent
    | UserClientIPUpdated of ClientIPEvent
    member this.Id =
        match this with
        | UserStreamInitiated e -> e.Id
        | UserStreamResumed e -> e.Id
        | UserStreamClosed e -> e.Id
        | UserStreamEnded e -> e.Id
        | UserClientIPDetected e -> e.Id
        | UserClientIPUpdated e -> e.Id

type ContactFormEventCase =
    | ContactFormSubmitted of ContactFormEvent
    | SignUpFormSubmitted of SignUpFormEvent
    member this.Id =
        match this with
        | ContactFormSubmitted e -> e.Id
        | SignUpFormSubmitted e -> e.Id