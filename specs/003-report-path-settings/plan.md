# Implementation Plan: Report Path Settings

**Branch**: `003-report-path-settings` | **Date**: 2026-06-15 (back-filled 2026-11-01) | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/003-report-path-settings/spec.md`

## Summary

A Razor page at `/settings` using `EditForm` with DataAnnotations validation. A scoped
`SettingsService` stores the value through JS interop (`localStorage.getItem`/`setItem`).

## Technical Context

**Language/Version**: C# / .NET 10, Razor components

**Primary Dependencies**: Microsoft.JSInterop, Blazor forms

**Storage**: Browser `localStorage`, key `dotwright.playwrightReportPath`

**Testing**: Playwright browser test (`tests/playwright/tests/settings.spec.ts`)

**Target Platform**: Browser (WASM)

**Project Type**: Single-page web app

**Performance Goals**: N/A

**Constraints**: No server dependency

**Scale/Scope**: One page, one service

## Constitution Check

- [x] I. N/A (UI-only preference)
- [x] II. `localStorage`, no server
- [ ] III. The browser test doesn't match the implementation, and the xUnit "Settings API" test exercises a stub of an API that no longer exists (T008, T009)
- [x] IV. Server API removed as unnecessary complexity
- [ ] V. `tests/playwright` has no committed `package.json` / `package-lock.json` (T010)

## Decisions (migrated from team decision log)

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Page vs modal | Dedicated `/settings` page | Simpler; easy to deep-link |
| Storage | `localStorage` key `dotwright.playwrightReportPath` | WASM can't write server files; keeps the app self-contained |
| Server Settings API | Built, then **removed** | Users wanted a client-only path; the API forced an ASP.NET Core host |
| Playwright lock file | Commit `tests/playwright/package-lock.json`, install with `npm ci` | Reproducible CI installs (Constitution V) |

## Project Structure

### Documentation (this feature)

```text
specs/003-report-path-settings/
├── spec.md
├── plan.md
└── tasks.md
```

### Source Code (repository root)

```text
Pages/Settings.razor
Services/ISettingsService.cs
Services/SettingsService.cs
Program.cs                              # DI registration
tests/playwright/tests/settings.spec.ts
tests/Dotwright.Settings.Tests/         # obsolete; see T009
```

**Structure Decision**: Client-only page and service in the host app.

## Complexity Tracking

None.
