# Decision: Add Playwright Package Lock File for Reproducible Installs

**Date:** 2026-06-12  
**Author:** Boltz (DevOps)  
**Status:** Proposed

## Summary
Added `tests/playwright/package-lock.json` to the repository to enable reproducible, locked dependency installs in CI pipelines.

## Rationale
Lock files ensure consistent dependency versions across all environments (local development, CI, production). This prevents unexpected version mismatches and improves CI reliability.

## Changes
- Generated `package-lock.json` using `npm install` (Node v24.15.0, npm 11.12.1)
- Verified that `npm ci` successfully installs from the lockfile

## Recommendation
Keep `npm ci` in CI pipelines instead of `npm install`. The `npm ci` command is purpose-built for CI/CD environments and will:
- Use locked versions from `package-lock.json`
- Fail faster on dependency conflicts
- Provide consistent, reproducible builds

## Implementation
Branch: `squad/add-playwright-lockfile` (51dec4d)  
Commit: "Add Playwright package-lock.json for reproducible CI installs"
