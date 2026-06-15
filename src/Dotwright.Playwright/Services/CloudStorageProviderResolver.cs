using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Dotwright.Playwright.Services
{
    /// <summary>
    /// Resolve a cloud storage provider for a given URI. This resolver relies on DI-registered concrete provider types
    /// and selects the appropriate provider by inspecting the URI scheme or host patterns.
    /// </summary>
    public class CloudStorageProviderResolver
    {
        private readonly IServiceProvider _sp;

        public CloudStorageProviderResolver(IServiceProvider sp)
        {
            _sp = sp ?? throw new ArgumentNullException(nameof(sp));
        }

        public ICloudStorageProvider? Resolve(Uri uri)
        {
            if (uri == null) return null;
            var scheme = uri.Scheme?.ToLowerInvariant() ?? string.Empty;

            try
            {
                if (scheme.StartsWith("s3"))
                    return _sp.GetService<AwsS3CloudStorageProvider>();

                if (scheme == "gs")
                    return _sp.GetService<GoogleCloudStorageProvider>();

                if (scheme == "azure" || uri.Host.Contains(".blob.core.windows.net", StringComparison.OrdinalIgnoreCase))
                    return _sp.GetService<AzureBlobCloudStorageProvider>();

                // HTTP/S with known host patterns
                if ((scheme == "http" || scheme == "https") && uri.Host.IndexOf("s3.amazonaws.com", StringComparison.OrdinalIgnoreCase) >= 0)
                    return _sp.GetService<AwsS3CloudStorageProvider>();

                if ((scheme == "http" || scheme == "https") && uri.Host.IndexOf("storage.googleapis.com", StringComparison.OrdinalIgnoreCase) >= 0)
                    return _sp.GetService<GoogleCloudStorageProvider>();

                if ((scheme == "http" || scheme == "https") && uri.Host.IndexOf("blob.core.windows.net", StringComparison.OrdinalIgnoreCase) >= 0)
                    return _sp.GetService<AzureBlobCloudStorageProvider>();

                return null;
            }
            catch
            {
                // Let consumers handle null/exception - providers may not be registered
                return null;
            }
        }
    }
}