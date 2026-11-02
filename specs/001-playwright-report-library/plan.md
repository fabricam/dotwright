# Implementation Plan: Playwright Report Library

**Branch**: `001-playwright-report-library` | **Date**: 2026-06-15 (back-filled 2026-11-01) | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-playwright-report-library/spec.md`

## Summary

Create the `Dotwright.Playwright` class library containing the Playwright report models, an
`IPlaywrightReportReader` built on `System.Text.Json`, and an `AddPlaywrightReporting()` DI
extension. Package it as `Fabricam.Dotwright.Playwright` and publish it from CI to GitHub
Packages.

## Technical Context

**Language/Version**: C# / .NET 10 (`net10.0`)

**Primary Dependencies**: System.Text.Json, Microsoft.Extensions.DependencyInjection 10.0.0

**Storage**: N/A (reads files and streams)

**Testing**: xUnit (`tests/Dotwright.Playwright.Tests`)

**Target Platform**: Any .NET 10 host. Filesystem APIs need a server or desktop host.

**Project Type**: Library + NuGet package

**Performance Goals**: Parse a typical report (≤ 5 MB) in well under a second

**Constraints**: Must not depend on ASP.NET Core, so it stays usable from Blazor WASM

**Scale/Scope**: One library project, one test project

## Constitution Check

- [x] I. Core logic lives in `Dotwright.Playwright`
- [x] II. Browser-safe: no server required; file I/O is documented as server-only
- [x] III. xUnit tests cover parse, file read, directory read, and invalid input
- [x] IV. No extra abstractions beyond one interface and one DI extension
- [x] V. SemVer `0.1.0`; publishes only from `main` using `GITHUB_TOKEN`

## Decisions (migrated from team decision log)

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Target framework | `net10.0` | Same framework as the Blazor WASM host |
| Namespaces | `Dotwright.Playwright`, models in `Dotwright.Playwright.Models` | Separates models from services |
| Reader lifetime | Singleton | Stateless; only holds `JsonSerializerOptions` |
| Registration | `AddPlaywrightReporting()` in `PlaywrightReportingServiceCollectionExtensions` | One-line adoption |
| Server-only file I/O | Documented in XML docs and `Program.cs` comments | WASM cannot read the host filesystem |
| Package ID / version | `Fabricam.Dotwright.Playwright` / `0.1.0` | Pre-1.0 while the API settles |
| Publishing | GitHub Packages on `main` push, `--skip-duplicate` | No extra secrets needed |

## Project Structure

### Documentation (this feature)

```text
specs/001-playwright-report-library/
├── spec.md
├── plan.md
└── tasks.md
```

### Source Code (repository root)

```text
src/Dotwright.Playwright/
├── Dotwright.Playwright.csproj        # package metadata
├── Models/PlaywrightReport.cs
├── PlaywrightReportingServiceCollectionExtensions.cs
└── Services/
    ├── IPlaywrightReportReader.cs
    └── PlaywrightReportReader.cs

tests/Dotwright.Playwright.Tests/
└── PlaywrightReportReaderTests.cs

.github/workflows/dotnet-tests.yml     # build, test, pack, publish
PACKAGING.md, GITHUB_PACKAGES_SETUP.md
```

**Structure Decision**: One library under `src/` and one matching test project under `tests/`.
The Blazor host references the library as a project reference.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Host app has its own static `dotwright.Services.PlaywrightReportReader` and `dotwright.Models` copies | Directory and cloud-prefix loading (`FromPathAsync`) was added in the host first | Known duplication. Consolidating into the library is tracked in tasks.md (T012) |
