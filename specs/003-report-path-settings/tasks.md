---

description: "Task list for Report Path Settings (back-filled)"
---

# Tasks: Report Path Settings

**Input**: Design documents from `/specs/003-report-path-settings/`

**Prerequisites**: plan.md, spec.md

**Note**: Completed tasks are a back-filled record of shipped work. Open tasks are known gaps.

## Format: `[ID] [P?] [Story] Description`

## Phase 1: User Story 1 - Configure where reports live (P1) 🎯 MVP

- [x] T001 [US1] Define `ISettingsService` in `Services/ISettingsService.cs`
- [x] T002 [US1] Implement `localStorage`-backed `SettingsService` in `Services/SettingsService.cs`
- [x] T003 [US1] Register `ISettingsService` in `Program.cs`
- [x] T004 [US1] Build the `/settings` page with validation, Save, and Cancel in `Pages/Settings.razor`

## Phase 2: User Story 2 - Navigation (P2)

- [x] T005 [US2] Add a cog button on the dashboard linking to `/settings` in `Pages/Home.razor`

## Phase 3: Reverted

- [x] T006 Remove the server-side `SettingsController`, `FileSettingsService`, and server `ISettingsService` from `src/Dotwright.Playwright/Services`
- [x] T007 Remove `FileSettingsService` registration from `AddPlaywrightReporting()`

## Phase 4: Open Follow-ups

- [ ] T008 [US1] Fix `tests/playwright/tests/settings.spec.ts`: it asserts the key `settings.filepath` and selector `input[name="filepath"]`, but the app uses `dotwright.playwrightReportPath` and an unnamed `InputText`. Add a stable selector (e.g., `id="report-path"`) in `Pages/Settings.razor`
- [ ] T009 Delete `tests/Dotwright.Settings.Tests`: it tests an in-test stub of the removed `/api/settings/filepath` API, not product code (Constitution III)
- [ ] T010 [P] Add `tests/playwright/package.json`, `playwright.config.ts`, and a committed `package-lock.json`, and run the Playwright tests in CI with `npm ci` against the WASM dev server
- [ ] T011 [P] [US1] Remove the stale "implement an API" comment at the top of `Pages/Settings.razor` and the server-registration comments in `Program.cs` that describe the removed settings API
