module CollabGateway.Server.Program

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Giraffe
open Microsoft.AspNetCore.Cors.Infrastructure

let private configureCors (builder: CorsPolicyBuilder) =
    builder.WithOrigins("http://localhost:8080", "https://rower.speakez.tech", "https://rower-stg.speakez.tech")
           .AllowAnyHeader()
           .AllowAnyMethod()
           .AllowCredentials()
    |> ignore

let private configureWeb (builder: WebApplicationBuilder) =
    builder.Services.AddGiraffe() |> ignore
    builder.Services.AddLogging() |> ignore
    builder.Services.AddCors(fun options ->
        options.AddPolicy("AllowClient", configureCors)
    ) |> ignore
    builder

let private configureApp (app: WebApplication) =
    app.UseCors("AllowClient") |> ignore
    app.UseStaticFiles() |> ignore
    app.UseGiraffe WebApp.webApp
    app

let private builderOptions = WebApplicationOptions(WebRootPath = "public")

let private builder =
    WebApplication.CreateBuilder(builderOptions)
    |> configureWeb

let app =
    builder.Build()
    |> configureApp

app.Run()
