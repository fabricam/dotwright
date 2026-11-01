Orchestration: Remove server-side settings API

Branch: squad/remove-settings-api
Fenster agent: fenster-4 (completed)

Files removed:
- src/Dotwright.Playwright/Services/SettingsController.cs
- src/Dotwright.Playwright/Services/FileSettingsService.cs
- src/Dotwright.Playwright/Services/ISettingsService.cs

Files edited:
- src/Dotwright.Playwright/PlaywrightReportingServiceCollectionExtensions.cs (removed FileSettingsService registration)
- PACKAGING.md (note about settings API removed)
- .squad/decisions/inbox/fenster-remove-settings-controller.md (decision record)

Tests & build:
- dotnet build: succeeded
- dotnet test tests/Dotwright.Playwright.Tests: 4 passed, 0 failed

Commit and push:
- Commit message by fenster: "Remove server-side SettingsController and FileSettingsService; keep client-only settings storage"
- Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
- Branch pushed: squad/remove-settings-api

Notes:
- Fenster adjusted tests to use a local TestServer stub for settings tests.
- Ready for PR creation when reviewers are available.
