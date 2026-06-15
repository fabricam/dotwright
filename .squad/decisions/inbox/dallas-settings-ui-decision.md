Decision: Settings UI storage and navigation

- Chosen to implement a dedicated /settings page (navigation) rather than a modal for simplicity and clarity.
- Settings are stored in browser localStorage under key `dotwright.playwrightReportPath` because Blazor WebAssembly cannot perform arbitrary server-side filesystem I/O.
- Server-side persistence is optional: implement a server API (e.g., POST /api/settings/report-path) to store the path centrally if needed.

Rationale: Navigation reduces UI complexity. localStorage provides straightforward persistence for client-only scenarios and keeps the WASM app self-contained.
