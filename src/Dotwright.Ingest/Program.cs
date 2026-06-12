using System.Data;
using Microsoft.Data.Sqlite;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var contentRoot = Directory.GetCurrentDirectory();
var dbFile = Path.Combine(contentRoot, "dotwright.db");
var schemaPath = Path.Combine(contentRoot, "data", "schema.sql");

EnsureDatabase();

app.MapPost("/ingest", async (HttpContext http) =>
{
    using var doc = await JsonDocument.ParseAsync(http.Request.Body);
    var root = doc.RootElement;
    var runId = root.TryGetProperty("runId", out var rid) ? rid.GetString() ?? Guid.NewGuid().ToString() : Guid.NewGuid().ToString();
    var project = root.TryGetProperty("project", out var proj) ? proj.GetString() : null;
    var startedAt = DateTime.UtcNow;
    var raw = root.GetRawText();

    using var conn = new SqliteConnection($"Data Source={dbFile}");
    conn.Open();
    using var tx = conn.BeginTransaction();
    var insertRun = conn.CreateCommand();
    insertRun.CommandText = "INSERT INTO runs (id, project, started_at, finished_at, status, raw_payload) VALUES ($id,$project,$started,$finished,$status,$raw);";
    insertRun.Parameters.AddWithValue("$id", runId);
    insertRun.Parameters.AddWithValue("$project", (object?)project ?? DBNull.Value);
    insertRun.Parameters.AddWithValue("$started", startedAt);
    insertRun.Parameters.AddWithValue("$finished", DBNull.Value);
    insertRun.Parameters.AddWithValue("$status", "running");
    insertRun.Parameters.AddWithValue("$raw", raw);
    insertRun.Transaction = tx;
    insertRun.ExecuteNonQuery();

    if (root.TryGetProperty("results", out var results) && results.ValueKind == JsonValueKind.Array)
    {
        foreach (var r in results.EnumerateArray())
        {
            var testName = r.GetProperty("testName").GetString() ?? "unknown";
            var status = r.TryGetProperty("status", out var s) ? s.GetString() ?? "unknown" : "unknown";
            var duration = r.TryGetProperty("durationMs", out var d) && d.TryGetInt32(out var di) ? di : 0;
            var err = r.TryGetProperty("errorMessage", out var e) ? e.GetString() : null;
            var insertT = conn.CreateCommand();
            insertT.CommandText = "INSERT INTO test_results (run_id, test_name, status, duration_ms, error_message, timestamp) VALUES ($run,$name,$status,$dur,$err,$ts);";
            insertT.Parameters.AddWithValue("$run", runId);
            insertT.Parameters.AddWithValue("$name", testName);
            insertT.Parameters.AddWithValue("$status", status);
            insertT.Parameters.AddWithValue("$dur", duration);
            insertT.Parameters.AddWithValue("$err", (object?)err ?? DBNull.Value);
            insertT.Parameters.AddWithValue("$ts", DateTime.UtcNow);
            insertT.Transaction = tx;
            insertT.ExecuteNonQuery();
        }
    }

    // mark finished and compute status
    var finish = DateTime.UtcNow;
    var update = conn.CreateCommand();
    update.CommandText = "UPDATE runs SET finished_at = $finish, status = $status WHERE id = $id;";
    update.Parameters.AddWithValue("$finish", finish);
    update.Parameters.AddWithValue("$status", "completed");
    update.Parameters.AddWithValue("$id", runId);
    update.Transaction = tx;
    update.ExecuteNonQuery();

    tx.Commit();
    return Results.Created($"/runs/{runId}", new { id = runId });
});

app.MapGet("/status", () =>
{
    using var conn = new SqliteConnection($"Data Source={dbFile}");
    conn.Open();
    var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT id, project, started_at, finished_at, status FROM runs ORDER BY started_at DESC LIMIT 1;";
    using var rdr = cmd.ExecuteReader();
    if (!rdr.Read()) return Results.Ok(new { message = "no runs" });
    var id = rdr.GetString(0);
    var project = rdr.IsDBNull(1) ? null : rdr.GetString(1);
    var started = rdr.IsDBNull(2) ? (DateTime?)null : rdr.GetDateTime(2);
    var finished = rdr.IsDBNull(3) ? (DateTime?)null : rdr.GetDateTime(3);
    var status = rdr.IsDBNull(4) ? null : rdr.GetString(4);
    return Results.Ok(new { id, project, started, finished, status });
});

app.MapGet("/runs", () =>
{
    using var conn = new SqliteConnection($"Data Source={dbFile}");
    conn.Open();
    var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT id, project, started_at, finished_at, status FROM runs ORDER BY started_at DESC LIMIT 50;";
    using var rdr = cmd.ExecuteReader();
    var list = new List<object>();
    while (rdr.Read())
    {
        list.Add(new {
            id = rdr.GetString(0),
            project = rdr.IsDBNull(1) ? null : rdr.GetString(1),
            started = rdr.IsDBNull(2) ? (DateTime?)null : rdr.GetDateTime(2),
            finished = rdr.IsDBNull(3) ? (DateTime?)null : rdr.GetDateTime(3),
            status = rdr.IsDBNull(4) ? null : rdr.GetString(4)
        });
    }
    return Results.Ok(list);
});

app.MapGet("/runs/{id}", (string id) =>
{
    using var conn = new SqliteConnection($"Data Source={dbFile}");
    conn.Open();
    var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT id, project, started_at, finished_at, status, raw_payload FROM runs WHERE id = $id;";
    cmd.Parameters.AddWithValue("$id", id);
    using var rdr = cmd.ExecuteReader();
    if (!rdr.Read()) return Results.NotFound();
    var run = new {
        id = rdr.GetString(0),
        project = rdr.IsDBNull(1) ? null : rdr.GetString(1),
        started = rdr.IsDBNull(2) ? (DateTime?)null : rdr.GetDateTime(2),
        finished = rdr.IsDBNull(3) ? (DateTime?)null : rdr.GetDateTime(3),
        status = rdr.IsDBNull(4) ? null : rdr.GetString(4),
        raw = rdr.IsDBNull(5) ? null : rdr.GetString(5)
    };

    var tcmd = conn.CreateCommand();
    tcmd.CommandText = "SELECT test_name, status, duration_ms, error_message, timestamp FROM test_results WHERE run_id = $id;";
    tcmd.Parameters.AddWithValue("$id", id);
    using var tr = tcmd.ExecuteReader();
    var tests = new List<object>();
    while (tr.Read())
    {
        tests.Add(new {
            testName = tr.IsDBNull(0) ? null : tr.GetString(0),
            status = tr.IsDBNull(1) ? null : tr.GetString(1),
            durationMs = tr.IsDBNull(2) ? 0 : tr.GetInt32(2),
            errorMessage = tr.IsDBNull(3) ? null : tr.GetString(3),
            timestamp = tr.IsDBNull(4) ? (DateTime?)null : tr.GetDateTime(4)
        });
    }

    return Results.Ok(new { run, tests });
});

app.Run();

void EnsureDatabase()
{
    try
    {
        var dir = Path.GetDirectoryName(dbFile);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
        var needCreate = !File.Exists(dbFile);
        using var conn = new SqliteConnection($"Data Source={dbFile}");
        conn.Open();
        if (needCreate)
        {
            if (!File.Exists(schemaPath))
            {
                Console.WriteLine($"Schema file not found: {schemaPath}");
                return;
            }
            var schema = File.ReadAllText(schemaPath);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = schema;
            cmd.ExecuteNonQuery();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }
}
