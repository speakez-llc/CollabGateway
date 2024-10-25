module CollabGateway.Client.Server

open Fable.Core.JsInterop
open Fable.SimpleJson
open Fable.Remoting.Client
open CollabGateway.Shared.Errors
open CollabGateway.Shared.API

let private exnToError (e: exn) : ServerError =
    match e with
    | :? ProxyRequestException as ex ->
        try
            let serverError = Json.parseAs<{| error: ServerError |}>(ex.Response.ResponseBody)
            serverError.error
        with _ -> ServerError.Exception(e.Message)
    | _ -> ServerError.Exception(e.Message)

type ServerResult<'a> = Result<'a, ServerError>

module Cmd =
    open Elmish
    module OfAsync =
        let eitherAsResult fn resultMsg =
            Cmd.OfAsync.either fn () (Result.Ok >> resultMsg) (exnToError >> Result.Error >> resultMsg)

let baseURL =
    let envVar: string option = importMember("import.meta.env.VITE_BASE_URL")
    match envVar with
    | Some url -> url
    | None -> "http://localhost:5000"

let service =
    Remoting.createApi()
    |> Remoting.withBaseUrl(baseURL)
    |> Remoting.withRouteBuilder Service.RouteBuilder
    |> Remoting.buildProxy<Service>