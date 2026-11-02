# Feature Specification: Test Run Dashboard

**Feature Branch**: `002-report-dashboard`

**Created**: 2026-06-13

**Status**: Implemented (back-filled from project history on 2026-11-01)

**Input**: User description: "A dashboard that lists Playwright test runs from the configured report location, newest first, with pass/fail/skip/flaky metrics and per-file test result tables."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - See recent test runs at a glance (Priority: P1)

A developer opens Dotwright and sees one collapsible entry per test run, labeled with the report
source name and start time and ordered newest first. Expanding a run shows duration, worker
count, Playwright version, and counts of passed, failed, skipped, and flaky tests.

**Why this priority**: This is the product's main purpose: knowing at a glance whether the
latest runs are healthy.

**Independent Test**: Configure a report location that contains two reports with different
start times. Confirm two entries appear in newest-first order with correct counts.

**Acceptance Scenarios**:

1. **Given** a configured location containing several reports, **When** the dashboard loads,
   **Then** each report appears as an entry titled "<source file name> — <start time>", ordered
   newest first.
2. **Given** an expanded run, **When** the user looks at the summary row, **Then** duration (in
   seconds), workers, Playwright version, and passed/failed/skipped/flaky counts are shown.
3. **Given** a run, **When** expanded, **Then** each browser project in the run appears as a badge.

---

### User Story 2 - Drill into results by test file (Priority: P2)

Within a run, results are grouped by test file. Each group lists every test result with its
title, line number, browser project, status, duration, and retry count.

**Why this priority**: After seeing that a run failed, the next question is which test failed
and in which browser.

**Independent Test**: Load a report with failures in two files. Confirm both file groups show
the failing rows with a failed status.

**Acceptance Scenarios**:

1. **Given** a run with specs in several files, **When** expanded, **Then** results are grouped
   by file, files are sorted alphabetically (case-insensitive), and a spec with no file appears
   under "Unknown file".
2. **Given** a result with status passed, failed, or anything else, **When** displayed, **Then**
   it shows a success, danger, or warning indicator respectively, along with the status text.

---

### User Story 3 - Preview and errors without a working configuration (Priority: P3)

A first-time user with no report location configured still sees a sample dashboard. A user
whose configured location can't be read sees a clear warning instead of a blank page.

**Why this priority**: Makes the app understandable before setup and makes configuration
problems easy to diagnose.

**Independent Test**: Clear the stored settings and confirm sample runs appear. Then set an
unreadable path and confirm a warning appears.

**Acceptance Scenarios**:

1. **Given** no report path configured, **When** the dashboard loads, **Then** sample runs are
   shown and the header reads "Report path not configured".
2. **Given** a configured path that fails to load, **When** the dashboard loads, **Then** a
   warning with the error message is shown and no sample data is shown.
3. **Given** a configured path, **When** the dashboard loads, **Then** that path is shown under
   the page title.

---

### User Story 4 - Light/dark theme and quick access to settings (Priority: P3)

The user can switch between light and dark themes, and the choice is remembered. A settings
(cog) button opens the Settings page.

**Why this priority**: A comfort feature. It doesn't block the core use case.

**Independent Test**: Toggle the theme, reload, and confirm it persists. Click the cog and
confirm navigation to `/settings`.

**Acceptance Scenarios**:

1. **Given** light theme, **When** the theme toggle is clicked, **Then** the page switches to
   dark and the choice persists across reloads.
2. **Given** the dashboard, **When** the cog button is clicked, **Then** the user is taken to
   `/settings`.

### Edge Cases

- Source path is a URL: the label uses the last segment of the URL path.
- A configured location contains no valid reports: an empty list, no sample data, no crash.
- Non-report JSON files in a directory are skipped silently.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The dashboard MUST load all reports from the configured location (file,
  directory, or cloud URI) and display them newest first by run start time.
- **FR-002**: Each run MUST show duration, workers, Playwright version, project badges, and
  passed (expected), failed (unexpected), skipped, and flaky counts.
- **FR-003**: Results MUST be grouped by test file and show title, line, project, status,
  duration (ms), and retry.
- **FR-004**: The dashboard MUST show sample data only when no path is configured.
- **FR-005**: Load failures MUST appear as a visible warning that includes the error message.
- **FR-006**: The theme choice MUST persist in `localStorage` under `dotwright.theme` and be
  applied before first render to avoid a flash of the wrong theme.
- **FR-007**: The dashboard MUST provide navigation to the Settings page.

### Key Entities

- **PlaywrightReportSource**: A loaded report together with the path or URI it came from.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A user can tell whether the latest run passed within 5 seconds of opening the app.
- **SC-002**: A user can find a specific failing test (file, browser, retry count) in at most
  two clicks from the dashboard.
- **SC-003**: The theme preference survives a page reload 100% of the time.

## Assumptions

- Report loading behavior is defined by [001](../001-playwright-report-library/spec.md) and
  [004](../004-cloud-storage-reports/spec.md). The report location comes from
  [003](../003-report-path-settings/spec.md).
- Trend views and history across runs are out of scope here (see
  [005](../005-test-run-history/spec.md)).
