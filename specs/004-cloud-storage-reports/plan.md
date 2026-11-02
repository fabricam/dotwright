# Implementation Plan: Cloud Storage Report Sources

**Branch**: `004-cloud-storage-reports` | **Date**: 2026-06-15 (back-filled 2026-11-01) | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/004-cloud-storage-reports/spec.md`

## Summary

Add `ICloudStorageProvider` with Azure Blob, AWS S3, and GCS implementations in
`Dotwright.Playwright`, plus a URI-based resolver. Extend the readers with cloud loading:
`FromUriAsync` in the library and `FromPathAsync` (file / directory / cloud prefix with
fallback) in the host.

## Technical Context

**Language/Version**: C# / .NET 10

**Primary Dependencies**: Azure.Storage.Blobs 12.14.0, AWSSDK.S3 3.7.0, Google.Cloud.Storage.V1 3.10.0

**Storage**: Remote object storage (read-only)

**Testing**: xUnit. **No provider tests exist yet** (T011–T013).

**Target Platform**: Browser (Azure HTTPS) and server hosts (all providers)

**Project Type**: Library extension

**Performance Goals**: Load files sequentially. Acceptable for tens of reports.

**Constraints**: Browser CORS; no committed credentials

**Scale/Scope**: 3 providers, 1 resolver

## Constitution Check

- [x] I. Providers live in the library
- [ ] II. Only Azure HTTPS works in the browser. S3/GCS need pre-signed/public HTTPS support (T014)
- [ ] III. No automated tests for providers or the resolver
- [ ] IV. Detection logic is duplicated in `CloudStorageProviderResolver` and the host's `PlaywrightReportReader.CreateProvider` (T015)
- [x] V. SDK versions pinned in the csproj

## Decisions (migrated from team decision log)

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Abstraction | `ICloudStorageProvider` + resolver | Add providers without changing callers |
| Detection | URI scheme first, then known host patterns | Handles both native (`s3://`) and HTTPS forms |
| Browser reads | Plain `HttpClient` GET for Azure HTTPS | SDK headers trigger CORS preflight |
| Listing failure | Fall back to `results.json`, `playwright-report.json`, `report.json` | List permission is often missing on SAS URLs |
| Error type | Wrap in `IOException` | One exception type for callers to handle |

## Project Structure

### Documentation (this feature)

```text
specs/004-cloud-storage-reports/
├── spec.md
├── plan.md
└── tasks.md
```

### Source Code (repository root)

```text
src/Dotwright.Playwright/Services/
├── ICloudStorageProvider.cs
├── CloudStorageProviderResolver.cs
└── CloudProviders/
    ├── AzureBlobCloudStorageProvider.cs
    ├── AwsS3CloudStorageProvider.cs
    └── GoogleCloudStorageProvider.cs
Services/PlaywrightReportReader.cs   # host FromPathAsync / FromCloudPathAsync
```

**Structure Decision**: Providers live in the library. Host-side orchestration moves into the
library under 001 T012.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Three cloud SDKs are hard dependencies of the package | All providers ship in one package | Splitting into per-provider packages is a candidate if package size becomes a problem |
