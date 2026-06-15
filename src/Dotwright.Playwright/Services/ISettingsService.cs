namespace Dotwright.Playwright.Services;

public interface ISettingsService
{
    /// <summary>
    /// Returns the stored Playwright report path, or null if not set.
    /// </summary>
    Task<string?> GetPlaywrightReportPathAsync();

    /// <summary>
    /// Stores the Playwright report path. Passing null clears the value.
    /// </summary>
    Task SetPlaywrightReportPathAsync(string? path);
}
