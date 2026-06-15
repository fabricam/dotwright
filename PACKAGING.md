# Dotwright.Playwright NuGet Package

This document describes how to package and publish the `Fabricam.Dotwright.Playwright` library.

## Local Packaging

To build the NuGet package locally:

```bash
cd src/Dotwright.Playwright
dotnet pack -c Release
```

The `.nupkg` file will be generated in `bin/Release/` with the name pattern:
```
Fabricam.Dotwright.Playwright.0.1.0.nupkg
```

## CI/CD Pipeline

The GitHub Actions workflow (`.github/workflows/dotnet-tests.yml`) automatically:

1. **Builds and tests** the library on every push and pull request
2. **Packs the library** when pushing to `main` branch
3. **Publishes to GitHub Packages** on successful pack (when pushing to `main`)

### Required GitHub Secrets Configuration

To enable package publishing to GitHub Packages, configure the following **GitHub repository secret**:

**Note:** The workflow already uses `${{ secrets.GITHUB_TOKEN }}` which is automatically available in GitHub Actions. No manual secret configuration is required for basic GitHub Packages publishing.

However, if you want to publish to **NuGet.org** in the future, you would need to add:
- `NUGET_API_KEY` - Your NuGet.org API key

## Publishing Locally

### To GitHub Packages

```bash
# Configure NuGet source (one-time setup)
dotnet nuget add source --username YOUR_GITHUB_USERNAME --password YOUR_GITHUB_TOKEN --store-password-in-clear-text --name github "https://nuget.pkg.github.com/fabricam/index.json"

# Push the package
cd src/Dotwright.Playwright
dotnet pack -c Release
dotnet nuget push bin/Release/Fabricam.Dotwright.Playwright.*.nupkg --source github --skip-duplicate
```

Replace:
- `YOUR_GITHUB_USERNAME` - Your GitHub username
- `YOUR_GITHUB_TOKEN` - A GitHub Personal Access Token with `write:packages` scope

### To NuGet.org

```bash
# Configure NuGet source (one-time setup)
dotnet nuget add source --name nuget.org https://api.nuget.org/v3/index.json

# Push the package
dotnet nuget push bin/Release/Fabricam.Dotwright.Playwright.*.nupkg --source nuget.org --api-key YOUR_NUGET_API_KEY
```

## Package Metadata

The package is configured in `src/Dotwright.Playwright/Dotwright.Playwright.csproj`:

- **PackageId**: `Fabricam.Dotwright.Playwright`
- **Version**: `0.1.0` (update before releases)
- **Authors**: Chris Martin
- **License**: MIT
- **Repository**: https://github.com/fabricam/dotwright
- **Description**: Playwright test report models and utilities for parsing Playwright JSON reports

## Consuming the Package

### From GitHub Packages

```xml
<!-- In your .csproj file -->
<ItemGroup>
  <PackageReference Include="Fabricam.Dotwright.Playwright" Version="0.1.0" />
</ItemGroup>
```

Ensure your NuGet sources include the GitHub Packages feed:

```bash
dotnet nuget add source --username YOUR_GITHUB_USERNAME --password YOUR_GITHUB_TOKEN --store-password-in-clear-text --name github "https://nuget.pkg.github.com/fabricam/index.json"
```

### Usage Example

```csharp
using Dotwright.Playwright;
using Dotwright.Playwright.Services;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddPlaywrightReporting();

var provider = services.BuildServiceProvider();
var reader = provider.GetRequiredService<IPlaywrightReportReader>();

// Parse Playwright report JSON
var json = await File.ReadAllTextAsync("playwright-report.json");
var report = reader.FromJson(json);
```

Note: The optional server-side settings API (ISettingsService, FileSettingsService, SettingsController) was removed from the Fabricam.Dotwright.Playwright library; settings are now a client-side string stored in localStorage by default. Hosts that need server-backed persistence can register their own ISettingsService implementation. See branch: https://github.com/fabricam/dotwright/tree/squad/remove-settings-api for details.

## Versioning

Update the version in `src/Dotwright.Playwright/Dotwright.Playwright.csproj` before creating a release:

```xml
<Version>0.2.0</Version>
```

Follow [Semantic Versioning](https://semver.org/):
- **MAJOR.MINOR.PATCH**
- MAJOR: Breaking changes
- MINOR: New features (backward compatible)
- PATCH: Bug fixes
