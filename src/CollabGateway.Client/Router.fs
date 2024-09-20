module CollabGateway.Client.Router

open Browser.Types
open Feliz.Router
open Fable.Core.JsInterop

type Page =
    | Index
    | Project
    | SignUp
    | Rower
    | SpeakEZ
    | Contact

[<RequireQualifiedAccess>]
module Page =
    let defaultPage = Page.Index

    let parseFromUrlSegments = function
        | [ "project" ] -> Page.Project
        | [ "signup" ] -> Page.SignUp
        | [ "rower" ] -> Page.Rower
        | [ "speakez" ] -> Page.SpeakEZ
        | [ "contact" ] -> Page.Contact
        | [ ] -> Page.Index
        | _ -> defaultPage

    let noQueryString segments : string list * (string * string) list = segments, []

    let toUrlSegments = function
        | Page.Index -> [ ] |> noQueryString
        | Page.Project -> [ "project" ] |> noQueryString
        | Page.SignUp -> [ "signup" ] |> noQueryString
        | Page.Rower -> [ "rower" ] |> noQueryString
        | Page.SpeakEZ -> [ "speakez" ] |> noQueryString
        | Page.Contact -> [ "contact" ] |> noQueryString

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