# Implementation Plan: Test Run Dashboard

**Branch**: `002-report-dashboard` | **Date**: 2026-06-13 (back-filled 2026-11-01) | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/002-report-dashboard/spec.md`

## Summary

A Blazor WASM page at `/` that reads the configured report path from `ISettingsService`, loads
runs through `PlaywrightReportReader.FromPathAsync`, and renders them with Blazor.Bootstrap
`Accordion` components. Theme switching uses a small JS helper (`window.dotwrightTheme`) in
`wwwroot/index.html`.

## Technical Context

**Language/Version**: C# / .NET 10, Razor components

**Primary Dependencies**: Microsoft.AspNetCore.Components.WebAssembly 10.0.x, Blazor.Bootstrap 3.5.0, Bootstrap Icons

**Storage**: Browser `localStorage` (`dotwright.theme`, `dotwright.playwrightReportPath`)

**Testing**: Playwright browser tests (`tests/playwright`)

**Target Platform**: Modern browsers (WASM)

**Project Type**: Single-page web app

**Performance Goals**: Render a dozen runs with ~100 results each without noticeable lag

**Constraints**: No server. Everything loads from the browser.

**Scale/Scope**: One page plus a layout

## Constitution Check

- [x] I. Parsing comes from the reader; the page only composes it
- [x] II. Fully client-side
- [ ] III. No Playwright browser tests for the dashboard yet (tasks T010–T012)
- [x] IV. No added abstractions
- [x] V. N/A

## Project Structure

### Documentation (this feature)

```text
specs/002-report-dashboard/
├── spec.md
├── plan.md
└── tasks.md
```

### Source Code (repository root)

```text
Pages/Home.razor            # dashboard, mock data, theme toggle
Layout/MainLayout.razor
Models/PlaywrightReportSource.cs
wwwroot/index.html          # dotwrightTheme JS helper, pre-render theme apply
wwwroot/css/app.css         # metric-circle styles
```

**Structure Decision**: Standard Blazor WASM layout at the repository root.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Sample data hard-coded in `Pages/Home.razor` | Gives first-run users a preview | Acceptable for now; could move to `wwwroot/sample-data/` later |
