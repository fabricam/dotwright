using System;
using System.Collections.Generic;

namespace dotwright.Models;

public class PlaywrightReport
{
    public ReportStats Stats { get; set; } = new();
    public List<Suite> Suites { get; set; } = new();
    public ReportConfig Config { get; set; } = new();
}

public class ReportStats
{
    public DateTime StartTime { get; set; }
    public double Duration { get; set; }
    public int Expected { get; set; }
    public int Skipped { get; set; }
    public int Unexpected { get; set; }
    public int Flaky { get; set; }
}

public class ReportConfig
{
    public string Version { get; set; } = string.Empty;
    public List<ProjectConfig> Projects { get; set; } = new();
    public int Workers { get; set; }
}

public class ProjectConfig
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Timeout { get; set; }
    public int Retries { get; set; }
}

public class Suite
{
    public string Title { get; set; } = string.Empty;
    public string File { get; set; } = string.Empty;
    public List<Spec> Specs { get; set; } = new();
}

public class Spec
{
    public string Title { get; set; } = string.Empty;
    public bool Ok { get; set; }
    public string File { get; set; } = string.Empty;
    public int Line { get; set; }
    public List<Test> Tests { get; set; } = new();
}

public class Test
{
    public string ProjectName { get; set; } = string.Empty;
    public string ExpectedStatus { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<TestResult> Results { get; set; } = new();
}

public class TestResult
{
    public string Status { get; set; } = string.Empty;
    public int Duration { get; set; }
    public int Retry { get; set; }
    public DateTime StartTime { get; set; }
    public List<string> Errors { get; set; } = new();
}
