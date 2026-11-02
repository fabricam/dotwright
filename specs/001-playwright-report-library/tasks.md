---

description: "Task list for Playwright Report Library (back-filled)"
---

# Tasks: Playwright Report Library

**Input**: Design documents from `/specs/001-playwright-report-library/`

**Prerequisites**: plan.md, spec.md

**Note**: Completed tasks are a back-filled record of shipped work. Open tasks are known gaps
found while migrating to Spec Kit.

## Format: `[ID] [P?] [Story] Description`

## Phase 1: Setup

- [x] T001 Create `src/Dotwright.Playwright/Dotwright.Playwright.csproj` targeting net10.0 and reference it from `src/Dotwright.Web/Dotwright.Web.csproj`
- [x] T002 [P] Create xUnit project `tests/Dotwright.Playwright.Tests`

## Phase 2: User Story 1 - Parse a Playwright JSON report (P1) 🎯 MVP

- [x] T003 [US1] Define report models in `src/Dotwright.Playwright/Models/PlaywrightReport.cs`
- [x] T004 [US1] Define `IPlaywrightReportReader` in `src/Dotwright.Playwright/Services/IPlaywrightReportReader.cs`
- [x] T005 [US1] Implement `FromJson`, `FromFileAsync`, `TryParse` in `src/Dotwright.Playwright/Services/PlaywrightReportReader.cs`
- [x] T006 [P] [US1] Tests for parse, file read, directory read, and invalid JSON in `tests/Dotwright.Playwright.Tests/PlaywrightReportReaderTests.cs` (note: these currently target the host copy `Dotwright.Web.Services.PlaywrightReportReader` via a reference to `src/Dotwright.Web/Dotwright.Web.csproj`, not the library; see T012)

## Phase 3: User Story 2 - DI registration (P2)

- [x] T007 [US2] Add `AddPlaywrightReporting()` in `src/Dotwright.Playwright/PlaywrightReportingServiceCollectionExtensions.cs`
- [x] T008 [US2] Document server-side-only file I/O in `src/Dotwright.Web/Program.cs` and XML docs

## Phase 4: User Story 3 - NuGet package (P3)

- [x] T009 [US3] Add package metadata to `src/Dotwright.Playwright/Dotwright.Playwright.csproj`
- [x] T010 [US3] Add pack + publish steps (main only) to `.github/workflows/dotnet-tests.yml`
- [x] T011 [P] [US3] Write `docs/PACKAGING.md` and `docs/GITHUB_PACKAGES_SETUP.md`

## Phase 5: Open Follow-ups

- [ ] T012 [US1] Move `FromPathAsync` (file / directory / cloud prefix) into the library and delete the duplicate host copies `src/Dotwright.Web/Services/PlaywrightReportReader.cs` and `src/Dotwright.Web/Models/PlaywrightReport.cs`, then point `tests/Dotwright.Playwright.Tests` at `src/Dotwright.Playwright` instead of `src/Dotwright.Web/Dotwright.Web.csproj`
- [ ] T013 [P] [US2] Add a test that `AddPlaywrightReporting()` resolves `IPlaywrightReportReader` as a singleton
- [ ] T014 [P] Delete the placeholder `tests/Dotwright.Playwright.Tests/UnitTest1.cs`
- [x] T015 [US3] Add a solution file (`dotwright.slnx`) that includes the app, library, and test projects, so root-level `dotnet test` in `.github/workflows/dotnet-tests.yml` runs every test project (previously it resolved only the app project and CI ran zero tests)
