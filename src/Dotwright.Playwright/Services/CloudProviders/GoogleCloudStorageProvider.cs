using System;
using System.Collections.Generic;
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

        public Task<IReadOnlyList<Uri>> ListFilesAsync(Uri uri, CancellationToken ct = default)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            try
            {
                var bucket = uri.Host;
                var prefix = uri.AbsolutePath.TrimStart('/');

                if (prefix.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult<IReadOnlyList<Uri>>(new[] { uri });
                }

                var client = StorageClient.Create();
                var files = new List<Uri>();

                foreach (var obj in client.ListObjects(bucket, prefix))
                {
                    if (obj.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    {
                        files.Add(new Uri($"gs://{bucket}/{obj.Name}"));
                    }
                }

                return Task.FromResult<IReadOnlyList<Uri>>(files);
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to list GCS URI '{uri}': {ex.Message}", ex);
            }
        }
    }
}