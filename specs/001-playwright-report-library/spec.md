# Feature Specification: Playwright Report Library

**Feature Branch**: `001-playwright-report-library`

**Created**: 2026-06-15

**Status**: Implemented (back-filled from project history on 2026-11-01)

**Input**: User description: "Extract Playwright JSON report parsing into a reusable class library with DI registration, publish it as a NuGet package, and cover it with automated tests."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Parse a Playwright JSON report (Priority: P1)

A .NET developer passes Playwright JSON reporter output (as a string or a file path) to the
library and gets back a strongly typed report: configuration (Playwright version, worker count,
projects), suites, specs, tests, individual results, and summary stats.

**Why this priority**: Every other Dotwright capability (dashboard, settings, cloud storage)
depends on turning raw JSON into a usable model.

**Independent Test**: Feed a sample Playwright report to the reader and assert that the key
fields map correctly.

**Acceptance Scenarios**:

1. **Given** a valid Playwright JSON string, **When** it is parsed, **Then** config, suites,
   specs, tests, results, and stats are populated with the values from the JSON.
2. **Given** a path to a file containing a valid report, **When** it is read, **Then** the same
   model is returned as when parsing the file's contents directly.
3. **Given** malformed JSON, **When** a try-parse is attempted, **Then** it returns failure plus
   an error message and does not throw.
4. **Given** an empty string or a path that doesn't exist, **When** it is parsed or read,
   **Then** the result is "no report" (null), not an exception.

---

### User Story 2 - Register the reader through dependency injection (Priority: P2)

A host application calls one registration method and can then inject the report reader (and
its optional cloud storage helpers) wherever it's needed.

**Why this priority**: Lets any .NET host adopt the library with one line of setup.

**Independent Test**: Build a service collection, call the registration method, and resolve
the reader.

**Acceptance Scenarios**:

1. **Given** an empty service collection, **When** `AddPlaywrightReporting()` is called,
   **Then** `IPlaywrightReportReader` resolves as a singleton.
2. **Given** the library is referenced by a Blazor WASM host, **When** a developer reads the
   registration documentation, **Then** it states that filesystem reads only work on a
   server-side host.

---

### User Story 3 - Consume the library as a NuGet package (Priority: P3)

A developer outside this repository installs `Fabricam.Dotwright.Playwright` from GitHub
Packages and uses the reader without cloning the source.

**Why this priority**: Distribution is valuable but secondary to the library working in-repo.

**Independent Test**: Run `dotnet pack` locally and confirm a `.nupkg` with the correct
metadata is produced. After a merge to `main`, confirm the package appears in GitHub Packages.

**Acceptance Scenarios**:

1. **Given** a push to `main`, **When** CI succeeds, **Then** the package is packed and pushed
   to GitHub Packages, and an already-published version is skipped without failing the build.
2. **Given** a pull request, **When** CI runs, **Then** the library is built and tested but
   not published.

### Edge Cases

- A report contains fields that aren't modeled: they are ignored and parsing still succeeds.
- Property name casing differs from the model: matching is case-insensitive.
- Enum-like strings (e.g., status values) use camelCase in the JSON.
- The same package version is published twice: the push is skipped (`--skip-duplicate`).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The library MUST deserialize Playwright JSON reporter output into a typed model
  covering config, projects, suites, specs, tests, results (status, duration, retry, start time,
  errors), and stats (start time, duration, expected, unexpected, skipped, flaky).
- **FR-002**: The library MUST provide `FromJson`, `FromFileAsync`, and `TryParse` operations on
  an `IPlaywrightReportReader` interface.
- **FR-003**: `TryParse` MUST report failures through its return value and error message and
  MUST NOT throw on malformed input.
- **FR-004**: The library MUST expose `AddPlaywrightReporting(IServiceCollection)`, which
  registers the reader as a singleton.
- **FR-005**: The library MUST target `net10.0`, the same framework as the host app.
- **FR-006**: The package MUST be published as `Fabricam.Dotwright.Playwright` with authors,
  MIT license, description, and repository URL metadata.
- **FR-007**: CI MUST build and test on every push and pull request to `main`, and MUST
  pack and publish only on pushes to `main`.

### Key Entities

- **PlaywrightReport**: Root of a single test run. Contains config, suites, and stats.
- **ReportConfig / ProjectConfig**: Playwright version, worker count, and the browser projects
  (id, name, timeout, retries).
- **Suite → Spec → Test → TestResult**: Hierarchy from a test file down to a single attempt's
  outcome, duration, retry index, and errors.
- **ReportStats**: Aggregate counts and duration for the run.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A report produced by Playwright's JSON reporter parses with zero errors and all
  modeled fields populated.
- **SC-002**: A host can start using the reader with one registration call and no further
  configuration.
- **SC-003**: Every merge to `main` produces a published package (or an intentional skip) with
  no manual steps.

## Assumptions

- Consumers are .NET 10 applications.
- Reading files from disk only happens on server-side or desktop hosts. The browser host relies
  on HTTP or cloud reads (see [004-cloud-storage-reports](../004-cloud-storage-reports/spec.md)).
- GitHub Packages is an acceptable package feed for now. nuget.org is out of scope.
