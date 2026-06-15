using Microsoft.AspNetCore.Mvc;
using Dotwright.Playwright.Services;

namespace Dotwright.Playwright.Controllers;

[ApiController]
[Route("api/settings")]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _settings;

    public SettingsController(ISettingsService settings)
    {
        _settings = settings;
    }

    [HttpGet("filepath")]
    public async Task<IActionResult> GetFilePath()
    {
        var path = await _settings.GetPlaywrightReportPathAsync();
        return Ok(new { path });
    }

    public class UpdateRequest { public string? path { get; set; } }

    [HttpPost("filepath")]
    public async Task<IActionResult> SetFilePath([FromBody] UpdateRequest? req)
    {
        await _settings.SetPlaywrightReportPathAsync(req?.path);
        return NoContent();
    }
}
