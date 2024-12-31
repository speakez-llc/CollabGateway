#r "nuget:System"
#r "nuget:System.IO"
#r "nuget:System.Net.Http"
#r "nuget:System.Threading.Tasks"
#r "nuget:FSharp.Data, 6.4.0"
#r "nuget:Marten, 7.31.0"
#r "nuget:Newtonsoft.Json, 13.0.1"
#r "nuget:Npgsql, 5.7.0"

open System
open System.IO
open System.Threading.Tasks
open System.Net.Http
open FSharp.Data
open Marten
open Newtonsoft.Json
open Npgsql

let execNonQueryAsync connStr commandStr =
    task {
        use conn = new NpgsqlConnection(connStr)
        use cmd = new NpgsqlCommand(commandStr, conn)
        do! conn.OpenAsync()
        let! result = cmd.ExecuteNonQueryAsync() |> Async.AwaitTask
        return result |> ignore
    }

let connStr = Environment.GetEnvironmentVariable("GATEWAY_STORE")

let parseDatabase (connectionString: string) =
    let parts = connectionString.Split(';')
    let databasePart = parts |> Array.find (_.StartsWith("Database="))
    databasePart.Split('=') |> Array.last

let getEmailTableRowCountAsync connStr databaseName =
    task {
        let commandStr = $"SELECT COUNT(*) FROM \"%s{databaseName}\".public.\"FreeEmailProviders\";"
        use conn = new NpgsqlConnection(connStr)
        use cmd = new NpgsqlCommand(commandStr, conn)
        do! conn.OpenAsync()
        let! result = cmd.ExecuteScalarAsync() |> Async.AwaitTask
        return result :?> int64
    }


let createFreeEmailDomainTableAsync databaseName =
    let commandStr = $"CREATE TABLE IF NOT EXISTS \"%s{databaseName}\".public.\"FreeEmailProviders\" (\"Id\" UUID PRIMARY KEY, \"Domain\" TEXT UNIQUE);"
    execNonQueryAsync connStr commandStr

let upsertFreeEmailDomainsAsync =
    task {
        let filePath = "D:\repos\CollabGateway\src\CollabGateway.Server\FreeEmailDomains.txt"
        let databaseName = parseDatabase connStr
        do! createFreeEmailDomainTableAsync databaseName
        let! fileLines = File.ReadAllLinesAsync(filePath) |> Async.AwaitTask
        let domains = fileLines |> Array.filter (fun line -> not (String.IsNullOrWhiteSpace(line))) |> Set.ofArray |> Set.toArray
        let fileRowCount = domains.Length |> int64
        let! tableRowCount = getEmailTableRowCountAsync connStr databaseName
        if tableRowCount < fileRowCount then
            Console.WriteLine $"Upserting FreeEmailProviders table with {fileRowCount - tableRowCount} new rows."
            for domainName in domains do
                let commandStr = $"INSERT INTO \"%s{databaseName}\".public.\"FreeEmailProviders\" (\"Id\", \"Domain\") VALUES ('%s{Guid.NewGuid().ToString()}', '%s{domainName}') ON CONFLICT (\"Domain\") DO NOTHING;"
                do! execNonQueryAsync connStr commandStr
        else
            Console.WriteLine "No new Webmail Domain rows to upsert."
    }

upsertFreeEmailDomainsAsync