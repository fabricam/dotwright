using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Dotwright.Playwright.Models;

namespace Dotwright.Playwright.Services;

public class PlaywrightReportReader : IPlaywrightReportReader
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public PlaywrightReport? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        return JsonSerializer.Deserialize<PlaywrightReport>(json, _options);
    }

    public async Task<PlaywrightReport?> FromFileAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return null;
        using var fs = File.OpenRead(path);
        try
        {
            return await JsonSerializer.DeserializeAsync<PlaywrightReport>(fs, _options);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public bool TryParse(string json, out PlaywrightReport? report, out string? error)
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