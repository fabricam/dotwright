# Dallas History

## Seed
- Project: dotwright
- Stack: Blazor WASM, .NET backend, Playwright
- Seeded by: Chris Martin on 2026-06-12T11:34:01.540-04:00

## Learnings

### Session: Add Settings UI (2026-06-15)
**Task:** Implement client-side Settings page with localStorage persistence and Dashboard navigation.

**Outcome:**
- Created Pages/Settings.razor with file path input and Save button
- Implemented ISettingsService (client-only in Services/) with localStorage integration
- Added cog button to Dashboard (Home.razor) linking to Settings
- Updated NavMenu.razor with Settings navigation link
- Registered ISettingsService in Program.cs DI
- Decision documented: Settings stored in localStorage under `dotwright.playwrightReportPath`
- **Branch:** squad/add-settings-ui

**Key Learning:** Blazor WASM cannot access server-side filesystem; localStorage provides self-contained persistence. Server-side API is optional and can be layered on later via Dotwright.Playwright Settings endpoints.
