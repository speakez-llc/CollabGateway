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

type SmartFormSubmittedEvent = {
    Id: Guid
    TimeStamp: DateTime
    ClipboardInput: SmartFormRawContent
}

type PageEvent = {
    Id: Guid
    TimeStamp: DateTime
}
type ButtonEvent = {
    Id: Guid
    TimeStamp: DateTime
}
type DataPolicyEvent = {
    Id: Guid
    TimeStamp: DateTime
}

type StreamEvent = {
    Id: Guid
    StreamID: StreamToken
    TimeStamp: DateTime
}

type PageEventCase =
    | HomePageVisited of PageEvent
    | ProjectPageVisited of PageEvent
    | DataPageVisited of PageEvent
    | SignupPageVisited of PageEvent
    | RowerPageVisited of PageEvent
    | SpeakEZPageVisited of PageEvent
    | ContactPageVisited of PageEvent
    | PartnersPageVisited of PageEvent
    | DataPolicyPageVisited of PageEvent
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

type ButtonEventCase =
    | HomeButtonClicked of ButtonEvent
    | HomeProjectButtonClicked of ButtonEvent
    | HomeSignUpButtonClicked of ButtonEvent
    | ProjectButtonClicked of ButtonEvent
    | ProjectDataButtonClicked of ButtonEvent
    | ProjectSignUpButtonClicked of ButtonEvent
    | DataButtonClicked of ButtonEvent
    | DataSignUpButtonClicked of ButtonEvent
    | SignUpButtonClicked of ButtonEvent
    | SmartFormButtonClicked of ButtonEvent
    | SmartFormSubmittedButtonClicked of ButtonEvent
    | RowerButtonClicked of ButtonEvent
    | RowerSignUpButtonClicked of ButtonEvent
    | SpeakEZButtonClicked of ButtonEvent
    | SpeakEZSignUpButtonClicked of ButtonEvent
    | ContactButtonClicked of ButtonEvent
    | PartnersButtonClicked of ButtonEvent
    | RowerSiteButtonClicked of ButtonEvent
    | CuratorSiteButtonClicked of ButtonEvent
    | TableauSiteButtonClicked of ButtonEvent
    | PowerBISiteButtonClicked of ButtonEvent
    | ThoughtSpotSiteButtonClicked of ButtonEvent
    | SpeakEZSiteButtonClicked of ButtonEvent
    | DataPolicyAcceptButtonClicked of ButtonEvent
    | DataPolicyDeclineButtonClicked of ButtonEvent
    | DataPolicyResetButtonClicked of ButtonEvent
    member this.Id =
        match this with
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
        | SmartFormSubmittedButtonClicked e -> e.Id
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

type StreamEventCase =
    | UserStreamInitiated of StreamEvent
    | UserStreamResumed of StreamEvent
    | UserStreamClosed of StreamEvent
    | UserStreamEnded of StreamEvent
    member this.Id =
        match this with
        | UserStreamInitiated e -> e.Id
        | UserStreamResumed e -> e.Id
        | UserStreamClosed e -> e.Id
        | UserStreamEnded e -> e.Id

type DataPolicyEventCase =
    | DataPolicyAccepted of DataPolicyEvent
    | DataPolicyDeclined of DataPolicyEvent
    | DataPolicyReset of DataPolicyEvent
    member this.Id =
        match this with
        | DataPolicyAccepted e -> e.Id
        | DataPolicyDeclined e -> e.Id
        | DataPolicyReset e -> e.Id

type ClientIPEventCase =
    | UserClientIPDetected of ClientIPEvent
    | UserClientIPUpdated of ClientIPEvent
    member this.Id =
        match this with
        | UserClientIPDetected e -> e.Id
        | UserClientIPUpdated e -> e.Id

type FormEventCase =
    | ContactFormSubmitted of ContactFormEvent
    | SignUpFormSubmitted of SignUpFormEvent
    | SmartFormSubmitted of SmartFormSubmittedEvent
    | SmartFormResultReturned of SignUpFormEvent
    member this.Id =
        match this with
        | ContactFormSubmitted e -> e.Id
        | SignUpFormSubmitted e -> e.Id
        | SmartFormSubmitted e -> e.Id
        | SmartFormResultReturned e -> e.Id

type EventCaseType =
    | PageEventCase of PageEventCase
    | ButtonEventCase of ButtonEventCase
    | StreamEventCase of StreamEventCase
    | FormEventCase of FormEventCase
    | ClientIPEventCase of ClientIPEventCase

type EventProcessingMessage =
    | ProcessStreamToken of StreamToken * EventDateTime
    | ProcessUserClientIP of StreamToken * EventDateTime * ClientIP
    | ProcessPageVisited of StreamToken * EventDateTime * PageName
    | ProcessButtonClicked of StreamToken * EventDateTime * ButtonName
    | ProcessSessionClose of StreamToken * EventDateTime
    | ProcessContactForm of StreamToken * EventDateTime * ContactForm
    | ProcessSignUpForm of StreamToken * EventDateTime * SignUpForm
    | ProcessSmartFormInput of StreamToken * EventDateTime * SmartFormRawContent
    | ProcessSmartFormResult of StreamToken * EventDateTime * SignUpForm

