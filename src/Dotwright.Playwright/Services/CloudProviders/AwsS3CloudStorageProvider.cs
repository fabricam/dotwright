using System;
using System.Collections.Generic;
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

        public async Task<IReadOnlyList<Uri>> ListFilesAsync(Uri uri, CancellationToken ct = default)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            try
            {
                var (bucket, keyOrPrefix) = ResolveBucketAndKey(uri);
                if (string.IsNullOrWhiteSpace(bucket))
                {
                    return Array.Empty<Uri>();
                }

                if (keyOrPrefix.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    return new[] { new Uri($"s3://{bucket}/{keyOrPrefix}") };
                }

                var client = new AmazonS3Client();
                var files = new List<Uri>();
                string? continuationToken = null;

                do
                {
                    var resp = await client.ListObjectsV2Async(new ListObjectsV2Request
                    {
                        BucketName = bucket,
                        Prefix = keyOrPrefix,
                        ContinuationToken = continuationToken
                    }, ct).ConfigureAwait(false);

                    foreach (var obj in resp.S3Objects)
                    {
                        if (obj.Key.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                        {
                            files.Add(new Uri($"s3://{bucket}/{obj.Key}"));
                        }
                    }

                    continuationToken = resp.IsTruncated ? resp.NextContinuationToken : null;
                }
                while (!string.IsNullOrEmpty(continuationToken));

                return files;
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to list S3 URI '{uri}': {ex.Message}", ex);
            }
        }

        private static (string Bucket, string Key) ResolveBucketAndKey(Uri uri)
        {
            if (uri.Scheme.StartsWith("s3", StringComparison.OrdinalIgnoreCase))
            {
                return (uri.Host, uri.AbsolutePath.TrimStart('/'));
            }

            if (uri.Host.Equals("s3.amazonaws.com", StringComparison.OrdinalIgnoreCase) || uri.Host.StartsWith("s3.", StringComparison.OrdinalIgnoreCase))
            {
                var segments = uri.AbsolutePath.TrimStart('/').Split('/', 2, StringSplitOptions.RemoveEmptyEntries);
                var bucket = segments.Length > 0 ? segments[0] : string.Empty;
                var key = segments.Length > 1 ? segments[1] : string.Empty;
                return (bucket, key);
            }

            var hostParts = uri.Host.Split(new[] { ".s3.amazonaws.com" }, StringSplitOptions.None);
            if (hostParts.Length > 1)
            {
                return (hostParts[0], uri.AbsolutePath.TrimStart('/'));
            }

            return (string.Empty, string.Empty);
        }
    }
}