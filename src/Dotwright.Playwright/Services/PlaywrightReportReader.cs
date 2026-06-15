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

    private readonly CloudStorageProviderResolver? _resolver;

    public PlaywrightReportReader(CloudStorageProviderResolver? resolver = null)
    {
        _resolver = resolver;
    }

    public PlaywrightReport? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        return JsonSerializer.Deserialize<PlaywrightReport>(json, _options);
    }

    public async Task<PlaywrightReport?> FromFileAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;

        // If the input looks like a URI (s3://, gs://, azure://, https://), prefer FromUriAsync
        if (Uri.TryCreate(path, UriKind.Absolute, out var maybeUri))
        {
            if (!string.IsNullOrWhiteSpace(maybeUri.Scheme) && maybeUri.Scheme != "file")
            {
                return await FromUriAsync(maybeUri).ConfigureAwait(false);
            }
        }

        if (!File.Exists(path)) return null;
        using var fs = File.OpenRead(path);
        try
        {
            return await JsonSerializer.DeserializeAsync<PlaywrightReport>(fs, _options).ConfigureAwait(false);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public async Task<PlaywrightReport?> FromUriAsync(Uri uri, CancellationToken ct = default)
    {
        if (uri == null) return null;

        // file scheme -> local file
        if (uri.IsFile || string.Equals(uri.Scheme, "file", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(uri.Scheme))
        {
            var path = uri.LocalPath;
            return await FromFileAsync(path).ConfigureAwait(false);
        }

        // Delegate to resolver if available
        var provider = _resolver?.Resolve(uri);
        if (provider == null)
        {
            throw new IOException($"No cloud storage provider registered for URI '{uri}'. Register cloud providers via AddPlaywrightReporting().");
        }

        await using var stream = await provider.OpenReadAsync(uri, ct).ConfigureAwait(false);
        try
        {
            return await JsonSerializer.DeserializeAsync<PlaywrightReport>(stream, _options, ct).ConfigureAwait(false);
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