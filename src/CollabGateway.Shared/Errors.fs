module CollabGateway.Shared.Errors

type ProcessErrorKind =
    | EmptyResponse
    | InvalidStatusCode of int
    | NoContentField
    | EmptyContent 
    | DeserializationError

type ServerError =
    | Exception of string
    | Authentication of string
    | ProcessError of ProcessErrorKind

exception ServerException of ServerError

module ServerError =
    let failwith (er:ServerError) = raise (ServerException er)

    let ofResult<'a> (v:Result<'a,ServerError>) =
        match v with
        | Ok v -> v
        | Error e -> e |> failwith

    let ofProcessError (err: ProcessErrorKind) =
        let msg = 
            match err with
            | EmptyResponse -> "Empty response from service"
            | InvalidStatusCode code -> $"Service returned status code {code}"
            | NoContentField -> "No content field in JSON response"
            | EmptyContent -> "Empty content in JSON response"
            | DeserializationError -> "Failed to deserialize response"
        ProcessError err