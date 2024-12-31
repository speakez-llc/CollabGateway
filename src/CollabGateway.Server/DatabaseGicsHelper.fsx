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

let getGicsTableRowCountAsync connStr databaseName =
    task {
        let commandStr = $"SELECT COUNT(Embeddings) FROM \"%s{databaseName}\".public.\"GicsTaxonomy\";"
        use conn = new NpgsqlConnection(connStr)
        use cmd = new NpgsqlCommand(commandStr, conn)
        do! conn.OpenAsync()
        let! result = cmd.ExecuteScalarAsync() |> Async.AwaitTask
        return result :?> int64
    }

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

let verifyGicsTableSchemaAsync connStr =
    task {
        let commandStr = """
            SELECT column_name, data_type, udt_name
            FROM information_schema.columns
            WHERE table_schema = 'public' AND table_name = 'GicsTaxonomy';
        """
        use conn = new NpgsqlConnection(connStr)
        use cmd = new NpgsqlCommand(commandStr, conn)
        do! conn.OpenAsync()
        let! reader = cmd.ExecuteReaderAsync() |> Async.AwaitTask
        let columns =
            [ while reader.Read() do
                let columnName = reader.GetString(0)
                let dataType = reader.GetString(1)
                let udtName = reader.GetString(2)
                yield columnName, dataType, udtName ]
        reader.Close()

        let expectedSchema =
            [ "Id", "uuid", "uuid"
              "SubIndustryCode", "text", "text"
              "SubIndustry", "text", "text"
              "Definition", "text", "text"
              "IndustryCode", "text", "text"
              "Industry", "text", "text"
              "IndustryGroupCode", "text", "text"
              "IndustryGroup", "text", "text"
              "SectorCode", "text", "text"
              "Sector", "text", "text"
              "Embedding", "vector", "vector" ]

        let columnsSimplified = columns |> List.map (fun (name, dataType, udtName) -> name, dataType, udtName)
        Console.WriteLine $"GicsTaxonomy table schema: {columnsSimplified}"
        if (columnsSimplified |> List.sort) <> (expectedSchema |> List.sort) then
            Console.WriteLine "GicsTaxonomy table schema is incorrect."
            return false
        else
            Console.WriteLine "GicsTaxonomy table schema is correct."
            return true
    }

type GicsCsv = CsvProvider<"GICS.csv", HasHeaders=true, Schema="SubIndustryCode (string), SubIndustry (string), Definition (string), IndustryCode (string), Industry (string), IndustryGroupCode (string), IndustryGroup (string), SectorCode (string), Sector (string)">

let verifyEmbeddedColumnIsPopulatedAsync connStr =
    task {
        let commandStr = "SELECT COUNT(*) FROM public.\"GicsTaxonomy\" WHERE \"Embedding\" IS NOT NULL;"
        let totalRowsCommandStr = "SELECT COUNT(*) FROM public.\"GicsTaxonomy\";"
        use conn = new NpgsqlConnection(connStr)
        use cmd = new NpgsqlCommand(commandStr, conn)
        use totalRowsCmd = new NpgsqlCommand(totalRowsCommandStr, conn)
        do! conn.OpenAsync()
        let! populatedCount = cmd.ExecuteScalarAsync() |> Async.AwaitTask
        let! totalCount = totalRowsCmd.ExecuteScalarAsync() |> Async.AwaitTask

        let csv = GicsCsv.Load("GICS.csv")
        let csvRowCount = csv.Rows |> Seq.length |> int64

        if populatedCount = null || totalCount = null || (populatedCount :?> int64) <> csvRowCount then
            Console.WriteLine "Not all rows in the GicsTaxonomy table Embedding column are populated or the row count does not match the CSV file."
            return false
        else
            Console.WriteLine "All rows in the GicsTaxonomy table Embedding column are populated and match the CSV file row count."
            return true
    }

type EmbeddingResponse = {
    embeddings: float[][]
}

type TextRequest = {
    model: string
    input: string
}

let generateVector (text: string) =
    task {
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

let verifyGicsTaxonomyTableAsync =
    task {
        let! isSchemaCorrect = verifyGicsTableSchemaAsync connStr
        let! isEmbeddingPopulated = verifyEmbeddedColumnIsPopulatedAsync connStr
        if not isSchemaCorrect || not isEmbeddingPopulated then
            Console.WriteLine "GicsTaxonomy table schema is incorrect or Embedding column is not fully populated."
            do! insertGicsTaxonomyAsync
        else
            Console.WriteLine $"GicsTaxonomy table is in place and populated."
    }

verifyGicsTaxonomyTableAsync