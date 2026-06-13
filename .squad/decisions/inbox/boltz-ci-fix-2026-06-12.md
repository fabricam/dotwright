# Boltz: CI Workflow Diagnostics & Remediation

**Date:** 2026-06-12T13:30:54.908-04:00  
**Component:** .github/workflows/squad-playwright.yml (Playwright test & ingestion pipeline)

---

## Executive Summary

**Failure Root Cause:** Playwright CI workflow fails due to missing `package-lock.json` in `tests/playwright/` directory. The workflow uses `npm ci` (clean install), which requires a pre-generated lockfile; without it, CI hangs or errors out.

**Classification:** REPO CODE ISSUE (missing dependency lock file, not CI environment issue)

**Impact:** CI cannot run Playwright tests or process ingestion results. Blocks #7 (Playwright report artifact ingestion).

---

## Detailed Diagnosis

### Workflow: squad-playwright.yml

**Current behavior (FAILING):**
```yaml
- name: Install dependencies
  working-directory: tests/playwright
  run: |
    npm ci  # ← FAILS: requires package-lock.json
    npx playwright install --with-deps
```

**Error reproduced locally:**
```
npm error code EUSAGE
npm error npm ci requires an existing package-lock.json or npm-shrinkwrap.json
```

**Why this happens:**
- `package-lock.json` is NOT committed to the repo (not in .gitignore exclusion)
- `npm ci` enforces reproducible, deterministic installs (correct for CI)
- Without lockfile, CI cannot proceed

---

## Root Cause Classification

| Category | Finding |
|----------|---------|
| **Environment?** | ✗ No. Node 18, npm version, runner (ubuntu-latest) all correct |
| **Runner image?** | ✗ No. Ubuntu runner has npm/Node installed correctly |
| **Secrets/credentials?** | ✗ No. npm registry access not needed for @playwright/test |
| **Repo code?** | ✓ **YES** — missing `package-lock.json` in version control |

---

## Recommended Fix (MINIMAL CI CHANGE)

### Option 1: Use `npm install` instead of `npm ci` (Quick workaround)
**Pros:** No repo changes needed; CI runs immediately  
**Cons:** Loses reproducibility guarantees; lockfile can drift over time  

**CI change (squad-playwright.yml):**
```yaml
- name: Install dependencies
  working-directory: tests/playwright
  run: |
    npm install  # Generates lockfile if missing, installs deps
    npx playwright install --with-deps
```

### Option 2: Commit `package-lock.json` (Best practice, recommended)
**Pros:** Reproducible builds; follows npm best practices; future-proof  
**Cons:** Requires one-time repo change  

**Steps:**
1. Run `npm install` locally in `tests/playwright/` to generate lockfile
2. Commit `tests/playwright/package-lock.json` to repo
3. Restore original CI config (keep `npm ci`)

---

## Validation & Remediation Plan

### Step 1: Generate & Commit Lock File (RECOMMENDED)
```powershell
# Local developer machine (one-time)
cd tests/playwright
npm install
# This generates package-lock.json

# Commit to repo
git add tests/playwright/package-lock.json
git commit -m "Add npm lock file for reproducible Playwright CI builds"
git push
```

### Step 2: Verify CI Passes
```
Push to any squad/* branch or PR → GitHub Actions runs squad-playwright.yml
Expected: ✓ npm ci succeeds
Expected: ✓ Playwright tests run
Expected: ✓ Ingestion script processes results
Expected: ✓ ingestion-db artifact uploaded
```

### Step 3: Local Validation (Before/After)
```powershell
# Before fix:
cd tests/playwright
npm ci  # ← ERROR: no package-lock.json

# After fix (with lockfile committed):
npm ci  # ← SUCCESS: installs from lock
npx playwright test
```

---

## Related Issues & Blockers

- **#7 MVP: Implement Playwright report artifact ingestion** — BLOCKED by CI failure
- **C# build error (separate issue)** — Owned by fenster + boltz (duplicate assembly attrs)
  - This does NOT block Playwright CI; it's a separate `dotnet build` problem
  - Triage: See keaton-triage-build-errors-2026-06-12.md

---

## CI Config Changes (If Recommending Quick Workaround)

If choosing **Option 1** (use `npm install`), apply this single change:

**File:** `.github/workflows/squad-playwright.yml`

```diff
      - name: Install dependencies
        working-directory: tests/playwright
        run: |
-         npm ci
+         npm install
          npx playwright install --with-deps
```

---

## Summary

| Aspect | Status |
|--------|--------|
| **Root cause identified?** | ✓ Yes — missing `package-lock.json` |
| **CI config fault?** | ✗ No — config is correct (npm ci is right choice) |
| **Repo code fault?** | ✓ Yes — lockfile not committed |
| **Minimal fix proposed?** | ✓ Yes — add lockfile OR use npm install |
| **Remediation steps clear?** | ✓ Yes — 3 steps outlined above |
| **Blocking impact?** | ✓ Blocks Playwright test pipeline |

---

**Recommendation:** Commit `package-lock.json` to repo (Option 2) for reproducible, resilient CI. Keep `npm ci` in workflow. One-time setup; solves permanently.

**Co-authored-by:** Boltz (DevOps)  
**Status:** READY FOR IMPLEMENTATION
