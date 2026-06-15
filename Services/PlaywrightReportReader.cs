using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using dotwright.Models;

namespace dotwright.Services;

public static class PlaywrightReportReader
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Deserialize a Playwright JSON report from a string.
    /// </summary>
    public static PlaywrightReport? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        return JsonSerializer.Deserialize<PlaywrightReport>(json, _options);
    }

    /// <summary>
    /// Asynchronously read and deserialize a Playwright JSON report from a file path.
    /// </summary>
    public static async Task<PlaywrightReport?> FromFileAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return null;
        using var fs = File.OpenRead(path);
        try
        {
            return await JsonSerializer.DeserializeAsync<PlaywrightReport>(fs, _options);
        }
        catch (JsonException)
        {
            // let caller handle null return for invalid JSON
            return null;
        }
    }

    /// <summary>
    /// Try-parse variant that returns success flag and error message when applicable.
    /// </summary>
    public static bool TryParse(string json, out PlaywrightReport? report, out string? error)
    {
        report = null;
        error = null;
        if (string.IsNullOrWhiteSpace(json))
        {
            error = "input json is null or empty";
            return false;
        }

        try
        {
            report = JsonSerializer.Deserialize<PlaywrightReport>(json, _options);
            return report != null;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
}
