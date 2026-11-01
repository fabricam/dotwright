using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Dotwright.Playwright.Services;
using dotwright.Models;

namespace dotwright.Services;

public static class PlaywrightReportReader
{
    private static readonly string[] _defaultReportFileNames =
    {
        "results.json",
        "playwright-report.json",
        "report.json"
    };

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
    /// Load one or more Playwright reports from a local file, local directory, or cloud-provider path/prefix.
    /// </summary>
    public static async Task<IReadOnlyList<PlaywrightReportSource>> FromPathAsync(string path, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return Array.Empty<PlaywrightReportSource>();
        }

        if (Uri.TryCreate(path, UriKind.Absolute, out var maybeUri) && !string.Equals(maybeUri.Scheme, "file", StringComparison.OrdinalIgnoreCase))
        {
            return await FromCloudPathAsync(maybeUri, ct).ConfigureAwait(false);
        }

        if (Directory.Exists(path))
        {
            return await FromDirectoryAsync(path, ct).ConfigureAwait(false);
        }

        var report = await FromFileAsync(path).ConfigureAwait(false);
        if (report == null)
        {
            return Array.Empty<PlaywrightReportSource>();
        }

        return new[]
        {
            new PlaywrightReportSource { SourcePath = path, Report = report }
        };
    }

    /// <summary>
    /// Asynchronously read and deserialize a Playwright JSON report from a file path.
    /// </summary>
    public static async Task<PlaywrightReport?> FromFileAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return null;
        using var fs = File.OpenRead(path);
        return await DeserializeAsync(fs).ConfigureAwait(false);
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

    private static async Task<IReadOnlyList<PlaywrightReportSource>> FromDirectoryAsync(string path, CancellationToken ct)
    {
        var files = Directory
            .EnumerateFiles(path, "*.json", SearchOption.AllDirectories)
            .OrderByDescending(file => file, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var reports = new List<PlaywrightReportSource>();

        foreach (var file in files)
        {
            ct.ThrowIfCancellationRequested();

            var report = await FromFileAsync(file).ConfigureAwait(false);
            if (report != null)
            {
                reports.Add(new PlaywrightReportSource { SourcePath = file, Report = report });
            }
        }

        return reports
            .OrderByDescending(item => item.Report.Stats.StartTime)
            .ToList();
    }

    private static async Task<IReadOnlyList<PlaywrightReportSource>> FromCloudPathAsync(Uri uri, CancellationToken ct)
    {
        var provider = CreateProvider(uri);
        if (provider == null)
        {
            return Array.Empty<PlaywrightReportSource>();
        }

        IReadOnlyList<Uri> fileUris;

        if (LooksLikeJsonFile(uri))
        {
            fileUris = new[] { uri };
        }
        else
        {
            try
            {
                fileUris = await provider.ListFilesAsync(uri, ct).ConfigureAwait(false);
            }
            catch
            {
                // In browser-hosted scenarios, listing can fail due to CORS or missing list permission.
                // Fall back to common report file names under the configured path.
                fileUris = BuildDefaultCloudReportUris(uri);
            }
        }

        var reports = new List<PlaywrightReportSource>();

        foreach (var fileUri in fileUris)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                await using var stream = await provider.OpenReadAsync(fileUri, ct).ConfigureAwait(false);
                var report = await DeserializeAsync(stream, ct).ConfigureAwait(false);
                if (report != null)
                {
                    reports.Add(new PlaywrightReportSource { SourcePath = fileUri.ToString(), Report = report });
                }
            }
            catch
            {
                // Continue trying additional candidate files.
            }
        }

        return reports
            .OrderByDescending(item => item.Report.Stats.StartTime)
            .ToList();
    }

    private static IReadOnlyList<Uri> BuildDefaultCloudReportUris(Uri baseUri)
    {
        var basePath = baseUri.AbsolutePath.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(basePath))
        {
            return Array.Empty<Uri>();
        }

        var query = string.Join("&", ParseAndFilterCloudQuery(baseUri.Query));
        var candidates = new List<Uri>(_defaultReportFileNames.Length);

        foreach (var fileName in _defaultReportFileNames)
        {
            var builder = new UriBuilder(baseUri)
            {
                Path = $"{basePath}/{fileName}",
                Query = query
            };

            candidates.Add(builder.Uri);
        }

        return candidates;
    }

    private static IReadOnlyList<string> ParseAndFilterCloudQuery(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Array.Empty<string>();
        }

        return query.TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Where(pair =>
            {
                var key = pair.Split('=', 2)[0];
                return !string.Equals(key, "restype", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(key, "comp", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(key, "prefix", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(key, "marker", StringComparison.OrdinalIgnoreCase);
            })
            .ToList();
    }

    private static ICloudStorageProvider? CreateProvider(Uri uri)
    {
        var scheme = uri.Scheme?.ToLowerInvariant() ?? string.Empty;
        var host = uri.Host ?? string.Empty;

        if (scheme.StartsWith("s3", StringComparison.OrdinalIgnoreCase) || host.Contains("s3.amazonaws.com", StringComparison.OrdinalIgnoreCase))
        {
            return new AwsS3CloudStorageProvider();
        }

        if (string.Equals(scheme, "gs", StringComparison.OrdinalIgnoreCase) || host.Contains("storage.googleapis.com", StringComparison.OrdinalIgnoreCase))
        {
            return new GoogleCloudStorageProvider();
        }

        if (string.Equals(scheme, "azure", StringComparison.OrdinalIgnoreCase) || host.Contains("blob.core.windows.net", StringComparison.OrdinalIgnoreCase))
        {
            return new AzureBlobCloudStorageProvider();
        }

        return null;
    }

    private static bool LooksLikeJsonFile(Uri uri)
    {
        return uri.AbsolutePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<PlaywrightReport?> DeserializeAsync(Stream stream, CancellationToken ct = default)
    {
        try
        {
            return await JsonSerializer.DeserializeAsync<PlaywrightReport>(stream, _options, ct).ConfigureAwait(false);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
