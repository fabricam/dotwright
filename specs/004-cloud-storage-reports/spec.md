# Feature Specification: Cloud Storage Report Sources

**Feature Branch**: `004-cloud-storage-reports`

**Created**: 2026-06-15

**Status**: Implemented, partially verified (back-filled from project history on 2026-11-01)

**Input**: User description: "When the configured report location is a cloud URI (Azure Blob, AWS S3, Google Cloud Storage), detect the provider automatically and read reports from it. Load a single report or every report under a prefix."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Read reports from an Azure Blob URL (Priority: P1)

A team publishes Playwright JSON reports from CI to an Azure Blob container. A user pastes a blob
or container URL (optionally with a SAS token) into Settings. The dashboard shows those runs
straight from the browser.

**Why this priority**: CI publishes reports to cloud storage, so this is the most common
real-world source, and Azure HTTPS/SAS URLs work from the browser.

**Independent Test**: Upload two reports to a container with a read/list SAS and CORS enabled,
configure the container URL, and confirm both runs appear.

**Acceptance Scenarios**:

1. **Given** an `https://<account>.blob.core.windows.net/<container>/<path>.json` URL, **When**
   reports load, **Then** that single report is fetched with a plain HTTP GET and displayed.
2. **Given** a container or virtual-folder URL with list permission, **When** reports load,
   **Then** every `.json` blob under that prefix is listed, loaded, and shown newest first.
3. **Given** a folder URL where listing is denied (CORS or permissions), **When** reports load,
   **Then** the reader falls back to well-known names (`results.json`, `playwright-report.json`,
   `report.json`) under that path, keeps any SAS query parameters, and shows whichever exist.
4. **Given** an `azure://<container>/<prefix>` URI on a server host with
   `AZURE_STORAGE_CONNECTION_STRING` set, **When** reports load, **Then** blobs are read through
   the Azure SDK.

---

### User Story 2 - Read reports from AWS S3 (Priority: P2)

A user configures an `s3://bucket/prefix` URI (or an `*.s3.amazonaws.com` URL) and sees the runs
stored there.

**Why this priority**: The second most common CI artifact store.

**Independent Test**: On a server host with AWS credentials available, configure an S3 prefix
containing reports and confirm they load.

**Acceptance Scenarios**:

1. **Given** an `s3://bucket/key.json` URI and default AWS credentials, **When** loaded,
   **Then** the object is read.
2. **Given** an `s3://bucket/prefix/` URI, **When** loaded, **Then** every `.json` object under
   the prefix is listed (following pagination) and loaded.

---

### User Story 3 - Read reports from Google Cloud Storage (Priority: P3)

A user configures a `gs://bucket/prefix` URI (or a `storage.googleapis.com` URL) and sees the
runs stored there.

**Why this priority**: Less common among the project's users.

**Independent Test**: On a server host with Google application default credentials, configure
a GCS prefix and confirm the reports load.

**Acceptance Scenarios**:

1. **Given** a `gs://bucket/prefix` URI, **When** loaded, **Then** every `.json` object under the
   prefix is listed and loaded.

---

### User Story 4 - Add a new provider without changing callers (Priority: P3)

A contributor adds another storage backend by implementing one interface. Callers of the
reader don't change.

**Acceptance Scenarios**:

1. **Given** a new `ICloudStorageProvider` implementation, **When** it is registered and mapped
   to its URI scheme or host, **Then** reports load through it with no changes to the dashboard.

### Edge Cases

- An unrecognized scheme or host returns no reports and no error.
- One unreadable or malformed file in a prefix is skipped. The other files still load.
- Any listing failure triggers the fallback to well-known file names.
- Listing-only query parameters (`restype`, `comp`, `prefix`, `marker`) are removed when
  building fallback URLs.
- Missing SDK credentials surface as an `IOException` with a clear message.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The reader MUST detect the provider from the URI: `s3*` scheme or
  `s3.amazonaws.com` host → S3; `gs` scheme or `storage.googleapis.com` host → GCS; `azure`
  scheme or `blob.core.windows.net` host → Azure.
- **FR-002**: A URI ending in `.json` MUST be treated as a single report. Any other URI MUST be
  treated as a prefix whose `.json` objects are listed.
- **FR-003**: Providers MUST implement `ICloudStorageProvider` (`OpenReadAsync`,
  `ListFilesAsync`).
- **FR-004**: Azure HTTP(S) reads MUST use a plain HTTP GET so they work from the browser
  without CORS preflight.
- **FR-005**: When listing fails, the reader MUST fall back to the default report file names.
- **FR-006**: Per-file read or parse failures MUST NOT stop the remaining files from loading.
- **FR-007**: Credentials MUST come from the environment, SDK default chains, or SAS/pre-signed
  URLs, never from the repository or settings UI.
- **FR-008**: Provider failures MUST surface as `IOException` with the URI in the message.
- **FR-009**: The library MUST expose `FromUriAsync` on `PlaywrightReportReader` and register
  providers plus `CloudStorageProviderResolver` in `AddPlaywrightReporting()`.

### Key Entities

- **ICloudStorageProvider**: Reads a stream from a URI and lists `.json` URIs under a prefix.
- **CloudStorageProviderResolver**: Maps a URI to the registered provider.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A user can view runs from an Azure container by pasting one URL, with no server.
- **SC-002**: A prefix of 50 reports loads completely when one of them is corrupt (49 shown).
- **SC-003**: Adding a provider touches no dashboard or settings code.

## Assumptions

- Browser use requires the storage account to allow CORS from the Dotwright origin.
- S3 and GCS currently rely on SDK default credential chains. In practice that means a server
  or desktop host. Browser support for them through pre-signed or public HTTPS URLs is a known
  gap (see tasks.md).
