module CollabGateway.Server.Program

open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Giraffe
open Microsoft.AspNetCore.Cors.Infrastructure
open CollabGateway.Server.Database

let private configureCors (builder: CorsPolicyBuilder) =
    builder.AllowAnyOrigin()
           .AllowAnyHeader()
           .AllowAnyMethod()
    |> ignore

let private configureWeb (builder: WebApplicationBuilder) =
    builder.Services.AddGiraffe() |> ignore
    builder.Services.AddLogging() |> ignore
    builder.Services.AddCors(fun options ->
        options.AddPolicy("AllowAll", configureCors)
    ) |> ignore
    builder.Services.AddSingleton(store) |> ignore
    builder

let private configureApp (app: WebApplication) =
    app.UseCors("AllowAll") |> ignore
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
