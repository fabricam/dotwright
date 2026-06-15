using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;

namespace Dotwright.Playwright.Services
{
    public class GoogleCloudStorageProvider : ICloudStorageProvider
    {
        public GoogleCloudStorageProvider()
        {
        }

        public Task<Stream> OpenReadAsync(Uri uri, CancellationToken ct = default)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            try
            {
                var bucket = uri.Host;
                var obj = uri.AbsolutePath.TrimStart('/');
                var client = StorageClient.Create();
                var ms = new MemoryStream();
                // Download the object into memory and return a readable stream
                client.DownloadObject(bucket, obj, ms);
                ms.Position = 0;
                return Task.FromResult((Stream)ms);
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to read GCS URI '{uri}': {ex.Message}", ex);
            }
        }
    }
}