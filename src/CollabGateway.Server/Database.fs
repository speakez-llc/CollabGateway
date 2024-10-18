module CollabGateway.Server.Database

open System
open Marten
open CollabGateway.Shared.Events
open Weasel.Core
open JasperFx.CodeGeneration
open Npgsql

module DatabaseTestHelpers =
    let execNonQuery connStr commandStr =
        use conn = new NpgsqlConnection(connStr)
        use cmd = new NpgsqlCommand(commandStr, conn)
        conn.Open()
        cmd.ExecuteNonQuery()

    let createDatabase connStr databaseName =
        let commandStr = $"DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_database WHERE datname = '%s{databaseName}') THEN CREATE DATABASE \"%s{databaseName}\" ENCODING = 'UTF8'; END IF; END $$;"
        execNonQuery connStr commandStr |> ignore

let configureMarten (options: StoreOptions) =
    let connectionString =
        match Environment.GetEnvironmentVariable("GATEWAY_STORE") with
        | null | "" -> failwith "Environment variable GATEWAY_STORE is not set."
        | connStr -> connStr
    options.Connection(connectionString)
    options.GeneratedCodeMode <- TypeLoadMode.Auto
    options.AutoCreateSchemaObjects <- AutoCreate.All
    options.Events.AddEventType(typeof<SessionEventCase>)
    options.Events.AddEventType(typeof<BaseEventCase>)

let store = DocumentStore.For(Action<StoreOptions>(configureMarten))

