module CollabGateway.Server.Notifications

open System
open Giraffe
open Microsoft.AspNetCore.Http
open CollabGateway.Server.Database


let private apiKey = Environment.GetEnvironmentVariable("NOTIFICATION_KEY")

let private authenticateApiKey (next: HttpFunc) (ctx: HttpContext) =
    task {
        let apiKeyParam = ctx.Request.Query["api_key"]
        if apiKeyParam = apiKey then
            return! next ctx
        else
            ctx.Response.StatusCode <- 401
            return! text "Unauthorized" next ctx
    }

let confirmEmailHandler (next: HttpFunc) (ctx: HttpContext) =
    task {
        let token = ctx.Request.Query["token"]
        // Your email confirmation logic here using the token
        return! text $"Email confirmed with token: {token}" next ctx
    }

let unsubscribeHandler (next: HttpFunc) (ctx: HttpContext) =
    task {
        let token = ctx.Request.Query["token"]
        // Your unsubscribe logic here using the token
        return! text $"Unsubscribed with token: {token}" next ctx
    }

let notificationManager : HttpHandler  =
    choose [
        route "/api/prefs/confirm" >=> authenticateApiKey >=> confirmEmailHandler
        route "/api/prefs/unsubscribe" >=> authenticateApiKey >=> unsubscribeHandler
    ]