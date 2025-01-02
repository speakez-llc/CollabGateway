#r "nuget:System"
#r "nuget:System.IO"
#r "nuget:System.Net.Http"
#r "nuget:FSharp.Data, 6.4.0"
#r "nuget:Npgsql, 6.0.0"
#r "nuget:Marten, 7.31.0"
#r "nuget:Newtonsoft.Json, 13.0.1"

open System
open System.Text
open System.Net.Http
open FSharp.Data
open Newtonsoft.Json
open Npgsql
open System.Threading.Tasks

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
    let databasePart = parts |> Array.find (fun part -> part.StartsWith("Database="))
    databasePart.Split('=') |> Array.last

let createGicsTaxonomyTable databaseName =
    let commandStr = $"""
        DROP TABLE IF EXISTS "%s{databaseName}".public."GicsTaxonomy";
        CREATE TABLE "%s{databaseName}".public."GicsTaxonomy" (
            "Id" UUID PRIMARY KEY,
            "SubIndustryCode" TEXT,
            "SubIndustry" TEXT,
            "Definition" TEXT,
            "IndustryCode" TEXT,
            "Industry" TEXT,
            "IndustryGroupCode" TEXT,
            "IndustryGroup" TEXT,
            "SectorCode" TEXT,
            "Sector" TEXT,
            "Embedding" VECTOR(384)
        );
        CREATE INDEX IF NOT EXISTS gics_embedding_idx ON "%s{databaseName}".public."GicsTaxonomy" USING hnsw ("Embedding" vector_cosine_ops);
    """
    execNonQueryAsync connStr commandStr

type GicsCsv = CsvProvider<"GICS.csv", HasHeaders=true, Schema="SubIndustryCode (string), SubIndustry (string), Definition (string), IndustryCode (string), Industry (string), IndustryGroupCode (string), IndustryGroup (string), SectorCode (string), Sector (string)">

type EmbeddingResponse = {
    embeddings: float[][]
}

type TextRequest = {
    model: string
    input: string
}

let generateVector (text: string) =
    async {
        use client = new HttpClient()
        let requestUri = "http://localhost:11434/api/embed"
        let content = new StringContent(JsonConvert.SerializeObject({ model = "granite-embedding:latest"; input = text }), Encoding.UTF8, "application/json")
        let! response = client.PostAsync(requestUri, content) |> Async.AwaitTask
        if not response.IsSuccessStatusCode then
            let! errorContent = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            Console.WriteLine $"HTTP error: {response.StatusCode} - {errorContent}"
        let! responseBody = response.Content.ReadAsStringAsync() |> Async.AwaitTask
        try
            let embeddingResponse = JsonConvert.DeserializeObject<EmbeddingResponse>(responseBody)
            if embeddingResponse.embeddings = null || embeddingResponse.embeddings.Length = 0 then
                failwith "Embedding is null or empty"
            let vectorBody = embeddingResponse.embeddings.[0]
            return vectorBody
        with
        | ex ->
            Console.WriteLine $"Error deserializing response: {ex.Message}"
            return Array.empty<float>
    }

let insertGicsTaxonomyAsync =
    task {
        let filePath = "GICS.csv"
        let databaseName = parseDatabase connStr
        do! createGicsTaxonomyTable databaseName
        let truncateCommandStr = $"TRUNCATE TABLE \"%s{databaseName}\".public.\"GicsTaxonomy\";"
        do! execNonQueryAsync connStr truncateCommandStr
        let csv = GicsCsv.Load(filePath)
        let rows = csv.Rows |> Seq.toArray
        Console.WriteLine $"Inserting GicsTaxonomy table with {rows.Length} rows."
        for index, row in rows |> Seq.mapi (fun i r -> i, r) do
            Console.WriteLine $"Inserting row {index}"
            let subIndustryCode = if String.IsNullOrWhiteSpace(row.SubIndustryCode) then "NULL" else $"'{row.SubIndustryCode}'"
            let subIndustry = if String.IsNullOrWhiteSpace(row.SubIndustry) then "NULL" else $"'{row.SubIndustry}'"
            let definition = if String.IsNullOrWhiteSpace(row.Definition) then "NULL" else $"'{row.Definition}'"
            let industryCode = row.IndustryCode
            let industry = row.Industry
            let industryGroupCode = row.IndustryGroupCode
            let industryGroup = row.IndustryGroup
            let sectorCode = row.SectorCode
            let sector = row.Sector
            let textToEncode =
                if subIndustry = "NULL" then
                    $"{sector} {industryGroup} {industry} {definition}"
                else
                    $"{sector} {industryGroup} {industry} {subIndustry} {definition}"
            try
                let! vector = generateVector(textToEncode)
                let vectorStr = String.Join(",", vector)
                let commandStr = $"""
                    INSERT INTO "%s{databaseName}".public."GicsTaxonomy"
                    ("Id", "SubIndustryCode", "SubIndustry", "Definition", "IndustryCode", "Industry", "IndustryGroupCode", "IndustryGroup", "SectorCode", "Sector", "Embedding")
                    VALUES ('%s{Guid.NewGuid().ToString()}', {subIndustryCode}, {subIndustry}, {definition}, '{industryCode}', '{industry}', '{industryGroupCode}', '{industryGroup}', '{sectorCode}', '{sector}', '[%s{vectorStr}]'::vector);
                """
                do! execNonQueryAsync connStr commandStr
            with
            | ex -> Console.WriteLine $"Error generating vector or inserting row: {ex.Message}"
    }

insertGicsTaxonomyAsync |> Async.AwaitTask |> ignore