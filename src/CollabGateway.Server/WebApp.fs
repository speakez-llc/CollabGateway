module CollabGateway.Server.WebApp

open Giraffe
open Giraffe.GoodRead
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Microsoft.Extensions.Logging
open CollabGateway.Shared.API
open CollabGateway.Shared.Errors

let sendEmail (form: ContactForm) =
    // Implement email sending logic here
    task {
        // Example: Use an SMTP client to send the email
        // SmtpClient.Send(...)
        return ()
    }

type Service = {
    GetMessage: bool -> Async<string>
    SubmitContactForm: ContactForm -> Async<string>
}

let service = {
    GetMessage = fun success ->
        task {
            if success then return "Hi from Server!"
            else return ServerError.failwith (ServerError.Exception "OMG, something terrible happened")
        }
        |> Async.AwaitTask
    SubmitContactForm = fun form ->
        task {
            do! sendEmail form
            return "Form submitted successfully"
        }
        |> Async.AwaitTask
}

let webApp : HttpHandler =
    let remoting logger =
        Remoting.createApi()
        |> Remoting.withRouteBuilder Service.RouteBuilder
        |> Remoting.fromValue service
        |> Remoting.withErrorHandler (Remoting.errorHandler logger)
        |> Remoting.buildHttpHandler
    choose [
        Require.services<ILogger<_>> remoting
        htmlFile "public/index.html"
    ]