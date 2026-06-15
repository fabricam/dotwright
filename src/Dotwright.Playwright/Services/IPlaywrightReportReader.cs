using System.Threading.Tasks;
using Dotwright.Playwright.Models;

namespace Dotwright.Playwright.Services;

public interface IPlaywrightReportReader
{
    PlaywrightReport? FromJson(string json);
    Task<PlaywrightReport?> FromFileAsync(string path);
    bool TryParse(string json, out PlaywrightReport? report, out string? error);
}