# Feature Specification: Test Run History and Flaky Test Detection

**Feature Branch**: `005-test-run-history`

**Created**: 2026-11-01

**Status**: Draft

**Input**: User description: "Ingest Playwright report artifacts into a persistent history of test runs so users can see trends across runs and spot flaky tests. Originally tracked as GitHub issues #7 (report artifact ingestion) and #8 (persistence layer)."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Ingest runs into a history (Priority: P1)

After each CI run, its Playwright report is ingested into Dotwright's run history. Ingestion
happens once per run; re-ingesting the same run doesn't create a duplicate.

**Why this priority**: Trends and flaky detection both depend on stored history.

**Independent Test**: Ingest the same report twice and one different report. Confirm the
history contains exactly two runs with correct counts.

**Acceptance Scenarios**:

1. **Given** a valid Playwright report, **When** it is ingested, **Then** a run is recorded
   with its start time, duration, configuration, projects, and every test result.
2. **Given** a report already ingested, **When** it is ingested again, **Then** no duplicate run
   is created.
3. **Given** a malformed report, **When** ingestion is attempted, **Then** it is rejected with a
   clear error and the history is unchanged.
4. **Given** a report referencing attachments (screenshots, traces, videos), **When** ingested,
   **Then** the attachment locations are recorded as references and the files themselves are
   not copied.

---

### User Story 2 - See a test's history across runs (Priority: P2)

A user selects a test and sees its outcome in each recent run, per browser project, with
durations.

**Why this priority**: Answers "when did this start failing?", which a single-run view can't.

**Independent Test**: Ingest five runs where one test fails in runs 3–5. Confirm the test's
history shows passes in runs 1–2 and failures in 3–5.

**Acceptance Scenarios**:

1. **Given** a history of runs, **When** the user opens a test, **Then** its result per run and
   project is listed newest first, with duration and retry count.
2. **Given** a test renamed or moved between runs, **When** viewed, **Then** it is treated as a
   different test (identity is file + title path + project).

---

### User Story 3 - Spot flaky tests (Priority: P2)

The dashboard flags tests that are flaky: tests whose outcome flips between pass and fail within
the last N runs, or that pass only after a retry.

**Why this priority**: Flaky tests erode trust in CI. Surfacing them is a core reason to keep
history.

**Independent Test**: Ingest runs where a test alternates pass/fail. Confirm it is flagged.
Confirm a test that always passes is not flagged.

**Acceptance Scenarios**:

1. **Given** a test with both passed and failed results within the last N runs, **When** the
   flaky list is viewed, **Then** the test is listed with its pass/fail ratio.
2. **Given** a test reported by Playwright as `flaky` (passed on retry), **When** viewed,
   **Then** it counts toward flakiness.
3. **Given** a test that passed in all of the last N runs, **When** viewed, **Then** it is not
   flagged.

---

### User Story 4 - Run-level trends (Priority: P3)

The dashboard shows pass rate and duration across recent runs.

**Acceptance Scenarios**:

1. **Given** at least two ingested runs, **When** the dashboard loads, **Then** a trend of pass
   rate and total duration per run is shown.

### Edge Cases

- Two runs with the same start time from different CI jobs must both be kept. The duplicate
  check needs more than the timestamp.
- A run with zero tests is stored and shown with zero counts.
- History grows without bound unless a retention policy applies.
- An attachment reference points to a location that has since expired.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST ingest Playwright JSON reports from every source the reader
  already supports (file, directory, cloud URI).
- **FR-002**: Ingestion MUST be idempotent per run.
- **FR-003**: The system MUST store runs, suites, specs, tests, results, and attachment
  references in a normalized history.
- **FR-004**: Attachments MUST be stored as external references only.
- **FR-005**: The system MUST flag a test as flaky when, within the last N runs, it has both
  passing and failing results or a Playwright `flaky` status. N defaults to 10 and MUST be
  configurable.
- **FR-006**: Users MUST be able to view a single test's history across runs.
- **FR-007**: History MUST be stored in [NEEDS CLARIFICATION: where does history live? Options:
  (a) in the browser (IndexedDB), which keeps the app serverless but makes history per-user and
  per-device; (b) a server-side database (prior direction: SQLite + EF Core), which shares
  history across the team but requires a host and must be justified under Constitution II;
  (c) a history file written by CI next to the reports in cloud storage].
- **FR-008**: Ingestion MUST be triggered by [NEEDS CLARIFICATION: a CI step that pushes each
  report, the dashboard discovering new reports when it loads, or both?].
- **FR-009**: The system MUST keep history for [NEEDS CLARIFICATION: retention period or
  maximum number of runs?].

### Key Entities

- **TestRun**: One ingested report: start time, duration, Playwright version, workers, source
  location, and a unique run identity.
- **TestSuite / TestCase**: The file and title path of a test. Identity is stable across runs.
- **TestResult**: One attempt of a test case in a run for a browser project: status, duration,
  retry, errors.
- **Attachment**: Name, content type, and an external location for a result's artifact.
- **FlakinessSummary**: Computed per test case over the last N runs: pass count, fail count,
  flaky flag.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Re-ingesting the same report 10 times leaves exactly one run in history.
- **SC-002**: A user can find when a test started failing in under 30 seconds.
- **SC-003**: Every test that alternated pass/fail in the last 10 runs appears in the flaky list
  (no false negatives on the seeded test set).
- **SC-004**: The dashboard with 100 ingested runs loads in under 3 seconds.

## Assumptions

- Prior team direction (not yet validated against this spec): SQLite with EF Core for the MVP
  store, a normalized domain model, external references for artifacts, and a simple N-run
  window for flaky detection. `/speckit-plan` should confirm or revise this against
  Constitution II once FR-007 is clarified.
- Reports come from Playwright's JSON reporter, as in [001](../001-playwright-report-library/spec.md).
- Authentication and multi-tenant access control are out of scope for this feature.
