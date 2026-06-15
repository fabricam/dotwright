using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Dotwright.Playwright.Services
{
    /// <summary>
    /// Abstraction for reading streams from cloud storage URIs (s3://, gs://, https://...blob.core.windows.net/... etc.).
    /// Implementations should be defensive and throw IOException when the underlying SDK is unavailable or when runtime resolution fails.
    /// </summary>
    public interface ICloudStorageProvider
    {
        /// <summary>
        /// Open a read stream for the given cloud URI.
        /// </summary>
        Task<Stream> OpenReadAsync(Uri uri, CancellationToken ct = default);
    }
}