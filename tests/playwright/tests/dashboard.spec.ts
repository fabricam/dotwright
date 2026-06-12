import { test, expect } from '@playwright/test';

// Dashboard flow: try to hit a running local dashboard (if available) and
// validate a couple of simple interactions. The base URL can be overridden
// with PLAYWRIGHT_BASE_URL env var (default http://localhost:5000).

const base = process.env.PLAYWRIGHT_BASE_URL ?? 'http://localhost:5000';

test('dashboard smoke - loads homepage', async ({ page }) => {
  await page.goto(base);
  // If the app isn't running, the test will fail quickly in CI; locally set
  // PLAYWRIGHT_BASE_URL to a running instance to exercise the full flow.
  await expect(page).toHaveTitle(/dotwright/i);
});
