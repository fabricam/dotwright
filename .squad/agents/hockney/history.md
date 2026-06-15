# Hockney History

## Seed
- Project: dotwright
- Stack: Blazor WASM, .NET backend, Playwright
- Seeded by: Chris Martin on 2026-06-12T11:34:01.540-04:00

## Learnings

### Session: Add Unit Tests for Reader (2026-06-15)
**Task:** Create comprehensive xUnit test suite for PlaywrightReportReader.

**Outcome:**
- Created tests/Dotwright.Playwright.Tests xUnit project
- Implemented PlaywrightReportReaderTests covering:
  - FromJson serialization/deserialization
  - FromFileAsync file loading
  - Invalid JSON parsing (error handling)
  - Field mapping validation (key fields present & correctly mapped)
- Added .github/workflows/dotnet-tests.yml for automated CI test runs
- Verified: All tests pass locally; dotnet test runs successfully
- **Branch:** squad/add-playwright-reader-tests

**Key Learning:** Full test coverage ensures reader resilience; CI integration prevents regressions.

### Session: Add Settings Tests (2026-06-15)
**Task:** Add backend integration and frontend Playwright tests for Settings UI/API feature.

**Outcome:**
- Created tests/Dotwright.Settings.Tests xUnit project with SettingsApiIntegrationTests
  - Tests POST to /api/settings/filepath; verifies file storage at data/settings.json
  - Tests GET to /api/settings/filepath; verifies persisted value retrieval
  - Uses TestHost for in-memory hosting; temp file for isolation
- Created tests/playwright/tests/settings.spec.ts browser test
  - Navigates to /settings page, fills filepath input, clicks Save
  - Validates localStorage entry `dotwright.playwrightReportPath`
- Verified: dotnet test passes (1/1); Playwright test ready (requires BASE_URL + npm install)
- **Branch:** squad/add-settings-tests

**Key Learning:** Full-stack testing (API integration + browser) ensures Settings feature works end-to-end; xUnit + TestHost enables fast backend validation without external host required.

