---

description: "Task list for Test Run Dashboard (back-filled)"
---

# Tasks: Test Run Dashboard

**Input**: Design documents from `/specs/002-report-dashboard/`

**Prerequisites**: plan.md, spec.md

**Note**: Completed tasks are a back-filled record of shipped work. Open tasks are known gaps.

## Format: `[ID] [P?] [Story] Description`

## Phase 1: User Story 1 - Recent runs at a glance (P1) 🎯 MVP

- [x] T001 [US1] Add `PlaywrightReportSource` in `Models/PlaywrightReportSource.cs`
- [x] T002 [US1] Render runs as accordion items with summary metrics in `Pages/Home.razor`
- [x] T003 [US1] Order runs newest first by `Stats.StartTime`

## Phase 2: User Story 2 - Results by file (P2)

- [x] T004 [US2] Group specs by file and render per-file result tables in `Pages/Home.razor`
- [x] T005 [US2] Status icons for passed / failed / other

## Phase 3: User Story 3 - Preview and errors (P3)

- [x] T006 [US3] Show sample runs when no path is configured
- [x] T007 [US3] Show a load-error warning alert

## Phase 4: User Story 4 - Theme and settings access (P3)

- [x] T008 [US4] Add the `dotwrightTheme` helper and pre-render theme apply in `wwwroot/index.html`
- [x] T009 [US4] Add the theme toggle and settings cog to `Pages/Home.razor`

## Phase 5: Open Follow-ups

- [ ] T010 [P] [US1] Playwright test: dashboard lists sample runs when unconfigured, in `tests/playwright/tests/dashboard.spec.ts`
- [ ] T011 [P] [US3] Playwright test: unreadable path shows a warning and no sample data
- [ ] T012 [P] [US4] Playwright test: theme persists across reload
