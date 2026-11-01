import { test, expect } from '@playwright/test';

const BASE = process.env.BASE_URL || 'http://localhost:5174';

test('settings save to localStorage', async ({ page }) => {
  await page.goto(`${BASE}/settings`);
  // adjust selectors to match the app's inputs
  await page.fill('input[name="filepath"]', 'C:\\temp\\myfile.txt');
  await page.click('button:has-text("Save")');
  const stored = await page.evaluate(() => localStorage.getItem('settings.filepath'));
  expect(stored).toBe('C:\\temp\\myfile.txt');
});
