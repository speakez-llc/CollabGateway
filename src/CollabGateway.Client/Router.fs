module CollabGateway.Client.Router

open Browser.Types
open Feliz.Router
open Fable.Core.JsInterop

type Page =
    | Index
    | Project
    | CMSData
    | SignUp
    | Rower
    | SpeakEZ
    | Contact
    | Partners
    | Activity

[<RequireQualifiedAccess>]
module Page =
    let defaultPage = Page.Index

    let parseFromUrlSegments = function
        | [ "project" ] -> Page.Project
        | [ "cmsdata" ] -> Page.CMSData
        | [ "signup" ] -> Page.SignUp
        | [ "rower" ] -> Page.Rower
        | [ "speakez" ] -> Page.SpeakEZ
        | [ "contact" ] -> Page.Contact
        | [ "partners" ] -> Page.Partners
        | [ "activity" ] -> Page.Activity
        | [ ] -> Page.Index
        | _ -> defaultPage

    let noQueryString segments : string list * (string * string) list = segments, []

    let toUrlSegments = function
        | Page.Index -> [ ] |> noQueryString
        | Page.Project -> [ "project" ] |> noQueryString
        | Page.CMSData -> [ "cmsdata" ] |> noQueryString
        | Page.SignUp -> [ "signup" ] |> noQueryString
        | Page.Rower -> [ "rower" ] |> noQueryString
        | Page.SpeakEZ -> [ "speakez" ] |> noQueryString
        | Page.Contact -> [ "contact" ] |> noQueryString
        | Page.Partners -> [ "partners" ] |> noQueryString
        | Page.Activity -> [ "activity" ] |> noQueryString

[<RequireQualifiedAccess>]
module Router =
    let goToUrl (e:MouseEvent) =
        e.preventDefault()
        let href : string = !!e.currentTarget?attributes?href?value
        Router.navigatePath href

    let navigatePage (p:Page) = p |> Page.toUrlSegments |> Router.navigatePath

[<RequireQualifiedAccess>]
module Cmd =
    let navigatePage (p:Page) = p |> Page.toUrlSegments |> Cmd.navigatePath