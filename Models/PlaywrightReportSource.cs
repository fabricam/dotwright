namespace dotwright.Models;

public class PlaywrightReportSource
{
    public string SourcePath { get; set; } = string.Empty;
    public PlaywrightReport Report { get; set; } = new();
}
