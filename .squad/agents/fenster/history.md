# Fenster History

## Seed
- Project: dotwright
- Stack: Blazor WASM, .NET backend, Playwright
- Seeded by: Chris Martin on 2026-06-12T11:34:01.540-04:00

## Learnings

### Session: Register Playwright Reader & Prepare Library (2026-06-15)
**Task:** Create class library for Playwright report parsing with DI registration.

**Outcome:** 
- Scaffolded Dotwright.Playwright class library (net10.0)
- Implemented PlaywrightReportReader with JsonSerializer and File I/O
- Created IPlaywrightReportReader interface and PlaywrightReportingServiceCollectionExtensions
- Updated main project to reference new library
- Verified: dotnet build succeeds; decision documented in squad/decisions.md
- **Branch:** squad/add-playwright-library

**Key Decision:** Server-side registration only (File I/O won't work in WASM); singleton lifetime for stateless reader.

### Session: Add Settings API & File Persistence (2026-06-15)
**Task:** Extend Dotwright.Playwright with optional server-side Settings API for file-backed path persistence.

**Outcome:**
- Added Services/ISettingsService.cs interface (GET/POST filepath)
- Implemented Services/FileSettingsService.cs (stores at data/settings.json)
- Added Services/SettingsController.cs with endpoints: GET /api/settings/filepath, POST /api/settings/filepath
- Updated PlaywrightReportingServiceCollectionExtensions to register FileSettingsService when AddPlaywrightReporting() called
- Decision documented: Settings API is optional; client-side defaults to localStorage
- **Branch:** squad/add-settings-api

**Key Learning:** Optional server-side features can layer cleanly via DI extensions; host projects control whether to enable file persistence or use client-only storage.

