Title: Fenster Settings API decision

Summary:
The repository adds an optional server-side Settings API to persist the Playwright report file path on disk under the repository's data folder (%REPO_ROOT%\\data\\settings.json). The client-side still uses localStorage by default; enabling server persistence requires hosting an ASP.NET Core server that registers the ISettingsService and exposes the controller endpoints.

Details:
- New API endpoints (server-side, optional):
  - GET /api/settings/filepath -> { "path": "..." }
  - POST /api/settings/filepath { "path": "..." } -> 204 No Content
- Storage location: <repo root>/data/settings.json
- Default behavior: client continues to use localStorage. Server persistence is opt-in and must be enabled by the host.

Migration / Usage:
- Hosts that run an ASP.NET Core server should call services.AddPlaywrightReporting() (or register ISettingsService manually) to enable the API and file-backed storage.

Author: Fenster (automated)
Date: 2026-06-15
