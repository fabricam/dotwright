using System.Text.Json;

namespace Dotwright.Playwright.Services;

internal class SettingsModel
{
    public string? Path { get; set; }
}

public class FileSettingsService : ISettingsService
{
    private readonly string _filePath;

    public FileSettingsService()
    {
        // Persist under the repository's data directory. Host must run from the repo root for this path to match TEAM_ROOT.
        var repoRoot = Directory.GetCurrentDirectory();
        var dataDir = Path.Combine(repoRoot, "data");
        Directory.CreateDirectory(dataDir);
        _filePath = Path.Combine(dataDir, "settings.json");
    }

    public async Task<string?> GetPlaywrightReportPathAsync()
    {
        if (!File.Exists(_filePath))
            return null;

        try
        {
            using var stream = File.OpenRead(_filePath);
            var model = await JsonSerializer.DeserializeAsync<SettingsModel>(stream);
            return model?.Path;
        }
        catch
        {
            return null;
        }
    }

    public async Task SetPlaywrightReportPathAsync(string? path)
    {
        var model = new SettingsModel { Path = path };
        var json = JsonSerializer.Serialize(model, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_filePath, json);
    }
}
