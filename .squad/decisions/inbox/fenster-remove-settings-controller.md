# Remove server-side SettingsController and FileSettingsService

Decision: Remove the optional server-side settings API (SettingsController, ISettingsService, FileSettingsService) from the Dotwright.Playwright library.

Why:
- Users requested a simple client-only path string stored in localStorage; the server-side API added unnecessary complexity.
- Keeping settings client-side avoids requiring hosts to run an ASP.NET Core server or grant filesystem access to persist a path.

What changed:
- Deleted SettingsController.cs, FileSettingsService.cs, and the server-side ISettingsService in src/Dotwright.Playwright/Services.
- PlaywrightReportingServiceCollectionExtensions no longer registers FileSettingsService by default; hosts may register their own ISettingsService implementation if desired.

Branch: squad/remove-settings-api

See branch/PR for implementation details and tests.