using Microsoft.Extensions.DependencyInjection;
using Dotwright.Playwright.Services;

namespace Dotwright.Playwright;

public static class PlaywrightReportingServiceCollectionExtensions
{
    /// <summary>
    /// Registers Playwright reporting services. The reader performs file I/O and is safe to register as a singleton.
    /// If you plan to use the reader in Blazor WebAssembly (browser) it will not be able to access the host filesystem; prefer server-side registration.
    /// </summary>
    public static IServiceCollection AddPlaywrightReporting(this IServiceCollection services)
    {
        // Register cloud providers (optional). Hosts that do not reference cloud SDKs may ignore these; providers will throw
        // IOException at runtime if SDKs are unavailable. See PACKAGING.md for packaging notes.
        services.AddTransient<AwsS3CloudStorageProvider>();
        services.AddTransient<AzureBlobCloudStorageProvider>();
        services.AddTransient<GoogleCloudStorageProvider>();

        services.AddSingleton<CloudStorageProviderResolver>();

        services.AddSingleton<IPlaywrightReportReader, PlaywrightReportReader>();
        // Note: server-side file-backed settings (ISettingsService/FileSettingsService) were removed from this library.
        // Hosts that require server-backed persistence can register their own ISettingsService implementation manually.
        return services;
    }
}