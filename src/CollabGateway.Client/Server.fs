module CollabGateway.Client.Server

open System
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


let serverUrl =
    match Environment.GetEnvironmentVariable("SERVER_URL") with
    | null | "" -> "http://localhost:5000"
    | url -> url

let service =
    Remoting.createApi()
    |> Remoting.withBaseUrl(serverUrl)
    |> Remoting.withRouteBuilder Service.RouteBuilder
    |> Remoting.buildProxy<Service>
