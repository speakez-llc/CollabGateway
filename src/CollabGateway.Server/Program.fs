module CollabGateway.Server.Program

open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Microsoft.AspNetCore.Cors.Infrastructure
open CollabGateway.Server.Database

let clientOrigin =
    match Environment.GetEnvironmentVariable("CLIENT_ORIGIN") with
    | null -> "http://localhost:8080"
    | url -> url

let private configureCors (builder: CorsPolicyBuilder) =
    builder.WithOrigins(clientOrigin)
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
    builder.Services.AddSingleton(store) |> ignore
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

app.Run("http://*:5000")
