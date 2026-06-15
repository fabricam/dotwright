using System;
using System.IO;
using System.Threading.Tasks;
using dotwright.Models;
using dotwright.Services;
using Xunit;

namespace Dotwright.Playwright.Tests;

public class PlaywrightReportReaderTests
{
    private const string SampleJson = @"{
  ""stats"": {
    ""startTime"": ""2026-06-15T12:00:00Z"",
    ""duration"": 1234,
    ""expected"": 1,
    ""skipped"": 0,
    ""unexpected"": 0,
    ""flaky"": 0
  },
  ""suites"": [
    {
      ""title"": ""Suite A"",
      ""file"": ""tests/suiteA.spec.ts"",
      ""specs"": [
        {
          ""title"": ""Spec 1"",
          ""ok"": true,
          ""file"": ""tests/suiteA.spec.ts"",
          ""line"": 10,
          ""tests"": [
            {
              ""projectName"": ""Chromium"",
              ""expectedStatus"": ""passed"",
              ""status"": ""passed"",
              ""results"": [
                {
                  ""status"": ""passed"",
                  ""duration"": 500,
                  ""retry"": 0,
                  ""startTime"": ""2026-06-15T12:00:00Z"",
                  ""errors"": []
                }
              ]
            }
          ]
        }
      ]
    }
  ],
  ""config"": {
    ""version"": ""1.0"",
    ""projects"": [ { ""id"": ""proj"", ""name"": ""proj"", ""timeout"": 0, ""retries"": 0 } ],
    ""workers"": 1
  }
}";

    [Fact]
    public void FromJson_Parses_Normal()
    {
        var report = PlaywrightReportReader.FromJson(SampleJson);
        Assert.NotNull(report);

        // Stats start time maps correctly
        var expectedStart = DateTime.Parse("2026-06-15T12:00:00Z").ToUniversalTime();
        Assert.Equal(expectedStart, report!.Stats.StartTime.ToUniversalTime());

        // Suites count
        Assert.Equal(1, report.Suites.Count);

        // Test result duration
        var duration = report.Suites[0].Specs[0].Tests[0].Results[0].Duration;
        Assert.Equal(500, duration);
    }

    [Fact]
    public async Task FromFileAsync_Reads_File()
    {
        var dir = Path.Combine(Directory.GetCurrentDirectory(), "playwright-test-data");
        Directory.CreateDirectory(dir);
        var file = Path.Combine(dir, "report.json");
        await File.WriteAllTextAsync(file, SampleJson);

        var report = await PlaywrightReportReader.FromFileAsync(file);
        Assert.NotNull(report);
        Assert.Equal(1, report!.Suites.Count);

        // cleanup
        File.Delete(file);
    }

    [Fact]
    public void TryParse_Returns_Error_For_Invalid_Json()
    {
        var invalid = "{ this is not: valid json }";
        var ok = PlaywrightReportReader.TryParse(invalid, out var report, out var error);
        Assert.False(ok);
        Assert.Null(report);
        Assert.False(string.IsNullOrWhiteSpace(error));
    }
}
