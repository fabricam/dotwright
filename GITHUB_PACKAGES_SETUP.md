# GitHub Packages Setup Checklist

This checklist describes what needs to be configured to enable automatic NuGet package publishing to GitHub Packages.

## Pre-requisites

✅ **Already Complete:**
- NuGet package metadata is configured in `src/Dotwright.Playwright/Dotwright.Playwright.csproj`
- CI workflow includes pack and publish steps in `.github/workflows/dotnet-tests.yml`
- PACKAGING.md contains detailed publishing instructions

## GitHub Secrets Configuration

### Automatic (No Action Needed)

The workflow uses `${{ secrets.GITHUB_TOKEN }}` which is automatically provided by GitHub Actions. This token has sufficient permissions to publish to GitHub Packages for the current repository.

**No manual secret configuration is required to publish to GitHub Packages.**

## What Happens Automatically

When you push commits to the `main` branch:

1. ✅ `.github/workflows/dotnet-tests.yml` triggers
2. ✅ Restores NuGet dependencies
3. ✅ Builds the library in Release configuration
4. ✅ Runs unit tests
5. ✅ **Packs the library** into `Fabricam.Dotwright.Playwright.0.1.0.nupkg`
6. ✅ **Publishes to GitHub Packages** at `https://nuget.pkg.github.com/fabricam/`

## Future Enhancements

### Publishing to NuGet.org

To also publish to the public NuGet.org registry:

1. Create a NuGet.org account at https://www.nuget.org/
2. Generate an API key from your account settings
3. Add the secret to GitHub:
   - Go to Settings > Secrets and variables > Actions > New repository secret
   - Name: `NUGET_ORG_API_KEY`
   - Value: Your NuGet.org API key
4. Update the workflow to add a second publish step:

```yaml
- name: Publish to NuGet.org
  run: |
    dotnet nuget push "src/Dotwright.Playwright/bin/Release/*.nupkg" --source https://api.nuget.org/v3/index.json --api-key ${{ secrets.NUGET_ORG_API_KEY }} --skip-duplicate
  if: github.ref == 'refs/heads/main' && github.event_name == 'push'
```

## Consuming the Package

### From CI/CD or Local Builds

Consumers need to configure the GitHub Packages source:

```bash
dotnet nuget add source \
  --username YOUR_GITHUB_USERNAME \
  --password YOUR_GITHUB_TOKEN \
  --store-password-in-clear-text \
  --name github \
  "https://nuget.pkg.github.com/fabricam/index.json"
```

Then reference in `.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="Fabricam.Dotwright.Playwright" Version="0.1.0" />
</ItemGroup>
```

## Release Workflow

When preparing a release:

1. Update version in `src/Dotwright.Playwright/Dotwright.Playwright.csproj`
   ```xml
   <Version>0.2.0</Version>
   ```
2. Create a commit with version bump (recommended message: "Bump version to 0.2.0")
3. Push to `main`
4. The workflow automatically builds, tests, and publishes the package
5. Create a GitHub Release tag if desired for release notes

## Troubleshooting

### Package Not Publishing

1. Check workflow run logs: https://github.com/fabricam/dotwright/actions
2. Verify the push is to `main` branch
3. Check that tests pass (publishing only runs if tests succeed)
4. Confirm the csproj has `GeneratePackageOnBuild>false</GeneratePackageOnBuild>` to prevent accidental double-packing

### Cannot Pull Package

1. Verify GitHub Packages source is configured
2. Check that the personal access token has `read:packages` scope
3. Confirm the package version exists in GitHub Packages

## Related Documentation

- [PACKAGING.md](./PACKAGING.md) - Local packaging and publishing instructions
- [GitHub Packages Documentation](https://docs.github.com/en/packages)
- [dotnet pack Reference](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-pack)
