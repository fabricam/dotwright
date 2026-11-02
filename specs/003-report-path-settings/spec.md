# Feature Specification: Report Path Settings

**Feature Branch**: `003-report-path-settings`

**Created**: 2026-06-15

**Status**: Implemented (back-filled from project history on 2026-11-01)

**Input**: User description: "Add a Settings page where the user enters the location of their Playwright reports. Save it in the browser, and open the page from a cog on the dashboard."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Configure where reports live (Priority: P1)

A user opens Settings, enters a report location (a local file or directory path, or a cloud
URI), and saves it. They return to the dashboard, which now loads reports from that location.

**Why this priority**: Without a configured location, the dashboard can only show sample data.

**Independent Test**: Open `/settings`, enter a value, save, and confirm the browser stores it
and the dashboard header shows it.

**Acceptance Scenarios**:

1. **Given** the Settings page, **When** the user enters a path and clicks Save, **Then** the
   value is stored in `localStorage` under `dotwright.playwrightReportPath` and the user is
   returned to the dashboard.
2. **Given** a previously saved path, **When** Settings is opened, **Then** the input is
   pre-filled with that value.
3. **Given** an empty input, **When** Save is clicked, **Then** a required-field validation
   message appears and nothing is saved.
4. **Given** unsaved edits, **When** Cancel is clicked, **Then** the user returns to the
   dashboard and the stored value is unchanged.

---

### User Story 2 - Reach Settings from anywhere it's needed (Priority: P2)

The user can open Settings from the dashboard's cog button.

**Why this priority**: Makes the feature discoverable, but it's secondary to the setting
itself working.

**Independent Test**: Click the cog on the dashboard and confirm `/settings` loads.

**Acceptance Scenarios**:

1. **Given** the dashboard, **When** the cog is clicked, **Then** `/settings` opens.

### Edge Cases

- `localStorage` is unavailable (e.g., some private modes): reading returns no value and the
  dashboard falls back to sample data.
- Very long paths or URIs with SAS query strings are stored verbatim.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The app MUST provide a dedicated `/settings` page (not a modal) with one
  required "Playwright Report File Path" input, plus Save and Cancel.
- **FR-002**: The value MUST persist in browser `localStorage` under the key
  `dotwright.playwrightReportPath`.
- **FR-003**: Settings access MUST go through an `ISettingsService` abstraction
  (`GetPlaywrightReportPathAsync`, `SetPlaywrightReportPathAsync`) registered in DI.
- **FR-004**: The app MUST NOT require a server to store settings.
- **FR-005**: The page MUST tell the user that only the path string is stored. The browser
  doesn't read the local filesystem itself.

### Key Entities

- **Report location setting**: A single string: a file path, directory path, or cloud URI.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A first-time user can configure a report location in under 30 seconds.
- **SC-002**: A saved setting survives a browser reload 100% of the time when `localStorage`
  is available.

## Assumptions

- One location per browser. Multiple named locations are out of scope.
- **Decision history**: An optional server-side Settings API (`GET`/`POST
  /api/settings/filepath`, file-backed at `data/settings.json`) was built and then removed. It
  required hosting an ASP.NET Core server for little benefit. Server persistence stays out of
  scope unless a future spec justifies it (Constitution II and IV).
