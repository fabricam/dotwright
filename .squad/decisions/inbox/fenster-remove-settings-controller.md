Title: Remove server-side settings API

Rationale:
- The server-side SettingsController duplicates client-side settings handling and increases surface area for bugs and maintenance.
- Removing the controller simplifies the codebase and centralizes settings logic on the client.

Branch: squad/remove-settings-api

See PR/branch for implementation details and tests.