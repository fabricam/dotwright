using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;

namespace Dotwright.Playwright.Services
{
    public class AzureBlobCloudStorageProvider : ICloudStorageProvider
    {
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
                    var client = new BlobClient(uri);
                    var resp = await client.DownloadAsync(ct).ConfigureAwait(false);
                    return resp.Value.Content;
                }

                // azure://container/path/to/blob
                if (uri.Scheme == "azure")
                {
                    // Expect an environment connection string as a convenience for azure:// scheme
                    var conn = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
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
    }
}