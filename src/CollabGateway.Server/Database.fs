module CollabGateway.Server.Database

open System
open Marten
open CollabGateway.Shared.API
open Weasel.Core
open Npgsql

type Event =
    | ContactFormSubmitted of ContactForm
    | SignUpFormSubmitted of SignUpForm

type Document =
    | ContactFormDocument of ContactForm
    | SignUpFormDocument of SignUpForm

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
    options.AutoCreateSchemaObjects <- AutoCreate.All
    options.Events.AddEventType(typeof<Event>)
    options.Schema.For<ContactForm>().DocumentAlias("contact_forms") |> ignore
    options.Schema.For<SignUpForm>().DocumentAlias("sign_up_forms") |> ignore

let store = DocumentStore.For(Action<StoreOptions>(configureMarten))

let saveEvent (event: Event) =
    use session = store.LightweightSession()
    session.Events.Append(Guid.NewGuid(), event) |> ignore
    session.SaveChanges()

let saveDocument (document: Document) =
    use session = store.LightweightSession()
    match document with
    | ContactFormDocument form -> session.Store<ContactForm>(form)
    | SignUpFormDocument form -> session.Store<SignUpForm>(form)
    session.SaveChanges()