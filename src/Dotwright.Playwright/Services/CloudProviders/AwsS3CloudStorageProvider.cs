using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace Dotwright.Playwright.Services
{
    public class AwsS3CloudStorageProvider : ICloudStorageProvider
    {
        public AwsS3CloudStorageProvider()
        {
        }

        public async Task<Stream> OpenReadAsync(Uri uri, CancellationToken ct = default)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            try
            {
                var bucket = uri.Host;
                var key = uri.AbsolutePath.TrimStart('/');

                var client = new AmazonS3Client(); // uses default credentials/region chain
                var req = new GetObjectRequest
                {
                    BucketName = bucket,
                    Key = key
                };

                var resp = await client.GetObjectAsync(req, ct).ConfigureAwait(false);
                return resp.ResponseStream;
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to read S3 URI '{uri}': {ex.Message}", ex);
            }
        }
    }
}