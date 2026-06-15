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
        services.AddSingleton<IPlaywrightReportReader, PlaywrightReportReader>();
        return services;
    }
}