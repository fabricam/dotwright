using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Azure;
using Azure.Storage.Blobs;

namespace Dotwright.Playwright.Services
{
    public class AzureBlobCloudStorageProvider : ICloudStorageProvider
    {
        private static readonly HttpClient _httpClient = new();

        public AzureBlobCloudStorageProvider()
        {
        }

        public async Task<Stream> OpenReadAsync(Uri uri, CancellationToken ct = default)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            try
            {
                // If the URI is an https blob URL (including SAS), BlobClient can be constructed directly
                if (uri.Scheme == "https" || uri.Scheme == "http")
                {
                    // Use a plain HTTP GET for browser-hosted clients to avoid SDK-specific headers
                    // that can trigger CORS preflight failures.
                    return await _httpClient.GetStreamAsync(uri, ct).ConfigureAwait(false);
                }

                // azure://container/path/to/blob
                if (uri.Scheme == "azure")
                {
                    // Expect an environment connection string as a convenience for azure:// scheme
                    var conn = ResolveConnectionString();
                    if (string.IsNullOrWhiteSpace(conn))
                        throw new IOException("azure:// URIs require AZURE_STORAGE_CONNECTION_STRING to be set in the environment or use an https blob URL (SAS) instead.");

                    var service = new BlobServiceClient(conn);
                    var container = uri.Host;
                    var blobPath = uri.AbsolutePath.TrimStart('/');
                    var containerClient = service.GetBlobContainerClient(container);
                    var blobClient = containerClient.GetBlobClient(blobPath);
                    var resp = await blobClient.DownloadAsync(ct).ConfigureAwait(false);
                    return resp.Value.Content;
                }

                throw new IOException($"AzureBlobCloudStorageProvider cannot handle URI scheme '{uri.Scheme}'");
            }
            catch (RequestFailedException ex)
            {
                throw new IOException($"Failed to read Azure blob '{uri}': {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to read Azure blob '{uri}': {ex.Message}", ex);
            }
        }

        public async Task<IReadOnlyList<Uri>> ListFilesAsync(Uri uri, CancellationToken ct = default)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            try
            {
                if (uri.Scheme == "azure")
                {
                    var conn = ResolveConnectionString();
                    if (string.IsNullOrWhiteSpace(conn))
                        throw new IOException("azure:// URIs require AZURE_STORAGE_CONNECTION_STRING to be set in the environment.");

                    var service = new BlobServiceClient(conn);
                    var container = uri.Host;
                    var prefix = uri.AbsolutePath.TrimStart('/');

                    if (prefix.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    {
                        return new[] { uri };
                    }

                    var containerClient = service.GetBlobContainerClient(container);
                    var files = new List<Uri>();

                    await foreach (var blob in containerClient.GetBlobsAsync(prefix: prefix, cancellationToken: ct).ConfigureAwait(false))
                    {
                        if (blob.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                        {
                            files.Add(new Uri($"azure://{container}/{blob.Name}"));
                        }
                    }

                    return files;
                }

                if (uri.Scheme == "https" || uri.Scheme == "http")
                {
                    if (uri.AbsolutePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    {
                        return new[] { uri };
                    }

                    return await ListFilesOverHttpAsync(uri, ct).ConfigureAwait(false);
                }

                return Array.Empty<Uri>();
            }
            catch (RequestFailedException ex)
            {
                throw new IOException($"Failed to list Azure blobs for '{uri}': {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to list Azure blobs for '{uri}': {ex.Message}", ex);
            }
        }

        private static string? ResolveConnectionString()
        {
            return Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
        }

        private static BlobClient CreateBlobClient(Uri uri)
        {
            var conn = ResolveConnectionString();
            if (string.IsNullOrWhiteSpace(conn))
            {
                return new BlobClient(uri);
            }

            var builder = new BlobUriBuilder(uri);
            if (string.IsNullOrWhiteSpace(builder.BlobName))
            {
                return new BlobClient(uri);
            }

            var service = new BlobServiceClient(conn);
            return service
                .GetBlobContainerClient(string.IsNullOrWhiteSpace(builder.BlobContainerName) ? "$root" : builder.BlobContainerName)
                .GetBlobClient(builder.BlobName);
        }

        private static async Task<IReadOnlyList<Uri>> ListContainerFilesAsync(BlobContainerClient containerClient, string containerName, string prefix, Uri sourceUri, CancellationToken ct)
        {
            var files = new List<Uri>();

            await foreach (var blob in containerClient.GetBlobsAsync(prefix: prefix, cancellationToken: ct).ConfigureAwait(false))
            {
                if (blob.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    files.Add(BuildBlobUri(sourceUri, containerName, blob.Name));
                }
            }

            return files;
        }

        private static Uri BuildBlobUri(Uri sourceUri, string containerName, string blobName)
        {
            var builder = new UriBuilder(sourceUri)
            {
                Path = string.Equals(containerName, "$root", StringComparison.OrdinalIgnoreCase)
                    ? blobName
                    : $"{containerName}/{blobName}"
            };

            return builder.Uri;
        }

        private static async Task<IReadOnlyList<Uri>> ListFilesOverHttpAsync(Uri uri, CancellationToken ct)
        {
            var segments = uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0)
            {
                throw new IOException("For HTTP/HTTPS blob listing, provide at least a container path (e.g. https://<account>.blob.core.windows.net/<container>/<optional-prefix>). ");
            }

            var containerName = segments[0];
            var prefix = segments.Length > 1 ? string.Join('/', segments.Skip(1)) : string.Empty;
            if (prefix.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                return new[] { uri };
            }

            var preservedQueryParts = ParseAndFilterSourceQuery(uri.Query);
            var files = new List<Uri>();
            string? marker = null;

            do
            {
                var queryParts = new List<string>(preservedQueryParts)
                {
                    "restype=container",
                    "comp=list"
                };

                if (!string.IsNullOrWhiteSpace(prefix))
                {
                    queryParts.Add($"prefix={Uri.EscapeDataString(prefix)}");
                }

                if (!string.IsNullOrWhiteSpace(marker))
                {
                    queryParts.Add($"marker={Uri.EscapeDataString(marker)}");
                }

                var listUri = new UriBuilder(uri.Scheme, uri.Host)
                {
                    Path = containerName,
                    Query = string.Join("&", queryParts)
                }.Uri;

                var xml = await _httpClient.GetStringAsync(listUri, ct).ConfigureAwait(false);
                var doc = XDocument.Parse(xml);

                foreach (var blob in doc.Descendants().Where(e => string.Equals(e.Name.LocalName, "Blob", StringComparison.Ordinal)))
                {
                    var name = blob.Elements().FirstOrDefault(e => string.Equals(e.Name.LocalName, "Name", StringComparison.Ordinal))?.Value;
                    if (!string.IsNullOrWhiteSpace(name) && name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    {
                        var blobQuery = string.Join("&", preservedQueryParts);
                        var blobUri = new UriBuilder(uri.Scheme, uri.Host)
                        {
                            Path = $"{containerName}/{name}",
                            Query = blobQuery
                        }.Uri;

                        files.Add(blobUri);
                    }
                }

                marker = doc.Descendants().FirstOrDefault(e => string.Equals(e.Name.LocalName, "NextMarker", StringComparison.Ordinal))?.Value;
            }
            while (!string.IsNullOrWhiteSpace(marker));

            return files;
        }

        private static IReadOnlyList<string> ParseAndFilterSourceQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Array.Empty<string>();
            }

            var parts = query.TrimStart('?')
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

            return parts;
        }
    }
}