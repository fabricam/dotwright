---

description: "Task list for Cloud Storage Report Sources (back-filled)"
---

# Tasks: Cloud Storage Report Sources

**Input**: Design documents from `/specs/004-cloud-storage-reports/`

**Prerequisites**: plan.md, spec.md

**Note**: Completed tasks are a back-filled record of shipped work. Open tasks are known gaps.

## Format: `[ID] [P?] [Story] Description`

## Phase 1: Foundational

- [x] T001 Define `ICloudStorageProvider` in `src/Dotwright.Playwright/Services/ICloudStorageProvider.cs`
- [x] T002 Add `CloudStorageProviderResolver` in `src/Dotwright.Playwright/Services/CloudStorageProviderResolver.cs`
- [x] T003 Add SDK package references to `src/Dotwright.Playwright/Dotwright.Playwright.csproj`
- [x] T004 Register providers and the resolver in `AddPlaywrightReporting()`

## Phase 2: User Story 1 - Azure Blob (P1) 🎯 MVP

- [x] T005 [US1] Implement `AzureBlobCloudStorageProvider` (HTTP GET, HTTP listing, `azure://` via connection string)
- [x] T006 [US1] Add `FromPathAsync` / `FromCloudPathAsync` with the default-file-name fallback in `src/Dotwright.Web/Services/PlaywrightReportReader.cs`

## Phase 3: User Story 2 - S3 (P2)

- [x] T007 [US2] Implement `AwsS3CloudStorageProvider` with paginated `ListObjectsV2`

## Phase 4: User Story 3 - GCS (P3)

- [x] T008 [US3] Implement `GoogleCloudStorageProvider`

## Phase 5: User Story 4 - Extensibility (P3)

- [x] T009 [US4] Add `FromUriAsync` to the library `PlaywrightReportReader`
- [x] T010 [US4] Document cloud support in `docs/PACKAGING.md`

## Phase 6: Open Follow-ups

- [ ] T011 [P] [US1] xUnit tests for provider detection (schemes and hosts) and the fallback URL builder, including query filtering
- [ ] T012 [P] [US1] xUnit test that one corrupt file in a prefix doesn't stop the others loading (stub provider)
- [ ] T013 [P] [US4] xUnit tests for `CloudStorageProviderResolver.Resolve`
- [ ] T014 [US2] [US3] Support pre-signed/public HTTPS URLs for S3 and GCS via plain HTTP GET, so they work in the browser. Today `OpenReadAsync` treats the URL host as the bucket name, which breaks `https://bucket.s3.amazonaws.com/...` and `https://storage.googleapis.com/bucket/...`
- [ ] T015 Remove the duplicate detection logic in the host `CreateProvider` and use `CloudStorageProviderResolver` (with 001 T012)
- [ ] T016 [P] [US3] Make `GoogleCloudStorageProvider` truly async (`DownloadObjectAsync`, `ListObjectsAsync`) and honor the `CancellationToken`
- [ ] T017 [P] Pin `Google.Cloud.Storage.V1` to a version that exists. 3.10.0 isn't on the feed, so restore silently resolves 4.0.0 (NU1603), a major-version jump (Constitution V)
