Playwright tests and runner for dotwright

Overview
- Tests live under tests/playwright/tests
- runner.js executes Playwright tests and emits a JSON payload suitable for ingestion by the backend.

Requirements
- Node.js (16+)
- Playwright (install via `npm i -D @playwright/test` and run `npx playwright install`)

Local run
1. Install dev deps: npm i -D @playwright/test uuid
2. Run tests and emit ingestion payload:
   node tests/playwright/runner.js > test-results.json

CI
- CI should install Playwright and run the runner script. The runner prints a single JSON payload to stdout that can be posted to the ingestion API.

Notes
- The runner attempts to collect git branch and commit sha when available.
- If Fenster provides a strict ingestion schema, update runner.js to map fields exactly.
