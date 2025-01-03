#r "nuget: FSharp.Data"
#r "nuget: Newtonsoft.Json"
#r "nuget: Npgsql"
#r "nuget: System.Net.Http"

open System
open System.Text
open System.Net.Http
open FSharp.Data
open Newtonsoft.Json
open Npgsql

let execNonQuery connStr commandStr =
    use conn = new NpgsqlConnection(connStr)
    use cmd = new NpgsqlCommand(commandStr, conn)
    conn.Open()
    cmd.ExecuteNonQuery() |> ignore

let connStr =

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
    execNonQuery connStr commandStr

type GicsCsv = CsvProvider<"GICS.csv", HasHeaders=true, Schema="SubIndustryCode (string), SubIndustry (string), Definition (string), IndustryCode (string), Industry (string), IndustryGroupCode (string), IndustryGroup (string), SectorCode (string), Sector (string)">

type EmbeddingResponse = {
    embeddings: float[][]
}

type TextRequest = {
    model: string
    input: string
}

let generateVector (text: string) =
    use client = new HttpClient()
    let requestUri = "http://localhost:11434/api/embed"
    let content = new StringContent(JsonConvert.SerializeObject({ model = "granite-embedding:latest"; input = text }), Encoding.UTF8, "application/json")
    let response = client.PostAsync(requestUri, content).Result
    if not response.IsSuccessStatusCode then
        let errorContent = response.Content.ReadAsStringAsync().Result
        Console.WriteLine $"HTTP error: {response.StatusCode} - {errorContent}"
    let responseBody = response.Content.ReadAsStringAsync().Result
    try
        let embeddingResponse = JsonConvert.DeserializeObject<EmbeddingResponse>(responseBody)
        if embeddingResponse.embeddings = null || embeddingResponse.embeddings.Length = 0 then
            failwith "Embedding is null or empty"
        embeddingResponse.embeddings.[0]
    with
    | ex ->
        Console.WriteLine $"Error deserializing response: {ex.Message}"
        Array.empty<float>

let insertRow (databaseName: string) (row: GicsCsv.Row) =
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
        let vector = generateVector(textToEncode)
        let vectorStr = String.Join(",", vector)
        let commandStr = $"""
            INSERT INTO "%s{databaseName}".public."GicsTaxonomy"
            ("Id", "SubIndustryCode", "SubIndustry", "Definition", "IndustryCode", "Industry", "IndustryGroupCode", "IndustryGroup", "SectorCode", "Sector", "Embedding")
            VALUES ('%s{Guid.NewGuid().ToString()}', {subIndustryCode}, {subIndustry}, {definition}, '{industryCode}', '{industry}', '{industryGroupCode}', '{industryGroup}', '{sectorCode}', '{sector}', '[%s{vectorStr}]'::vector);
        """
        execNonQuery connStr commandStr
    with
    | ex -> Console.WriteLine $"Error generating vector or inserting row: {ex.Message}"

let insertGicsTaxonomy () =
    let filePath = "GICS.csv"
    let databaseName = parseDatabase(connStr)
    createGicsTaxonomyTable(databaseName)
    let truncateCommandStr = $"TRUNCATE TABLE \"%s{databaseName}\".public.\"GicsTaxonomy\";"
    execNonQuery connStr truncateCommandStr
    let csv = GicsCsv.Load(filePath)
    let rows = csv.Rows |> Seq.toArray
    Console.WriteLine $"Inserting GicsTaxonomy table with {rows.Length} rows."
    for index, row in rows |> Seq.mapi (fun i r -> i, r) do
        Console.WriteLine $"Inserting row {index}"
        insertRow databaseName row

insertGicsTaxonomy()