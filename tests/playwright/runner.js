#!/usr/bin/env node
// Simple runner that executes Playwright tests and emits an ingestion-friendly
// JSON payload. If Playwright is not installed, instruct the user via README.

const cp = require('child_process');
const fs = require('fs');
const path = require('path');
const { v4: uuidv4 } = require('uuid');

function runPlaywright() {
  try {
    const out = cp.execSync('npx playwright test --reporter=json', { encoding: 'utf8', stdio: ['ignore', 'pipe', 'pipe'] });
    return out;
  } catch (err) {
    // Playwright may exit non-zero; try to capture stdout if present
    if (err.stdout) return err.stdout.toString();
    throw err;
  }
}

function parsePlaywrightJson(raw) {
  // Playwright's json reporter prints a single JSON object. Try parse directly.
  try {
    return JSON.parse(raw);
  } catch (e) {
    // Fallback: sometimes reporter emits lines; take last line that is JSON
    const lines = raw.trim().split(/\r?\n/).map(l=>l.trim()).filter(Boolean);
    for (let i = lines.length - 1; i >= 0; i--) {
      try { return JSON.parse(lines[i]); } catch (__) {}
    }
    throw new Error('Unable to parse Playwright JSON output');
  }
}

function mapToIngestion(playwrightJson) {
  const repo = (function(){
    try { return cp.execSync('git rev-parse --abbrev-ref HEAD', {encoding:'utf8'}).trim(); } catch(_) { return null; }
  })();
  const sha = (function(){
    try { return cp.execSync('git rev-parse HEAD', {encoding:'utf8'}).trim(); } catch(_) { return null; }
  })();

  const payload = {
    runId: process.env.RUN_ID || uuidv4(),
    timestamp: new Date().toISOString(),
    branch: repo,
    commit: sha,
    ci: !!process.env.CI,
    results: []
  };

  // Playwright JSON shape includes "suites" and "suites"->"specs"; adapt
  // be defensive in parsing.
  const tests = playwrightJson?.suites?.flatMap(s => s.specs?.flatMap(sp => sp.tests || []) ) || playwrightJson?.tests || [];

  for (const t of tests) {
    payload.results.push({
      title: t.title || t.name || 'unknown',
      ok: t.ok === true || t.status === 'passed',
      status: t.status || (t.ok ? 'passed' : 'failed'),
      durationMs: t.duration || (t.runs && t.runs[0] && t.runs[0].duration) || 0,
      error: t.error || (t.runs && t.runs[0] && t.runs[0].error) || null,
      trace: t.trace || null,
      location: t.location || null,
    });
  }

  return payload;
}

function main() {
  const raw = runPlaywright();
  const parsed = parsePlaywrightJson(raw);
  const payload = mapToIngestion(parsed);
  // Output to stdout as JSON for CI ingestion
  console.log(JSON.stringify(payload, null, 2));
}

main();
