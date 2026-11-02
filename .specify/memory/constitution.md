# Dotwright Constitution

Dotwright is a Blazor WebAssembly dashboard for browsing Playwright test results, backed by a
reusable .NET library (`Dotwright.Playwright`) that parses Playwright JSON reports from local
files, directories, and cloud storage.

## Core Principles

### I. Library-First Core

Report models, parsing, and storage access MUST live in the `Dotwright.Playwright` class library
(`src/Dotwright.Playwright`), not in UI components. The library MUST stay host-agnostic: it is
consumable by the Blazor WASM app, by an ASP.NET Core host, and by third parties through the
`Fabricam.Dotwright.Playwright` NuGet package. Services are exposed through interfaces and
registered with a single `AddPlaywrightReporting()` extension. Stateless services SHOULD be
singletons.

*Rationale*: The reader is the reusable asset; keeping it out of the UI lets it ship as a
package and be tested without a browser.

### II. Browser-Safe by Default

The dashboard runs entirely in the browser as Blazor WebAssembly. Features MUST work without a
companion server unless a spec explicitly justifies one. Concretely:

- User preferences persist in browser `localStorage` under a `dotwright.*` key.
- Code that needs the host filesystem MUST be documented as server-side only.
- Remote reads from the browser MUST avoid triggering CORS preflight where possible (plain
  HTTP GET against public or SAS/pre-signed URLs) and MUST degrade gracefully when listing is
  not permitted.

*Rationale*: A self-contained static app is the simplest thing to host and run. An earlier
optional server-side settings API was built and then removed because it added hosting
requirements nobody needed.

### III. Tests Accompany Every Change

Every feature MUST ship with automated tests that cover its acceptance scenarios:

- Library behavior: xUnit tests under `tests/Dotwright.Playwright.Tests`.
- UI behavior: Playwright browser tests under `tests/playwright`.
- Tests MUST exercise real code paths. A test MUST NOT stub out the very component it claims to
  verify.
- `dotnet build` and `dotnet test` MUST pass in CI (`.github/workflows/dotnet-tests.yml`) before
  merge to `main`.

*Rationale*: CI is the only reviewer that runs on every change.

### IV. Simplicity and Opt-In Complexity

Start with the smallest design that meets the spec (YAGNI). Optional capabilities (cloud
providers, server persistence) MUST be opt-in through DI registration and MUST NOT burden hosts
that don't use them. Remove features that lose their justification instead of keeping them as
dead code. Any added project, dependency, or abstraction layer MUST be justified in the plan's
Complexity Tracking table.

*Rationale*: Each optional layer is code someone has to maintain, so it has to earn its place.

### V. Reproducible Builds and Versioned Releases

- Dependency lock files MUST be committed (e.g., `package-lock.json` for Playwright tests), and
  CI MUST install with locked commands (`npm ci`, `dotnet restore`).
- The NuGet package follows Semantic Versioning (MAJOR.MINOR.PATCH). Breaking public API changes
  require a MAJOR bump (or MINOR while < 1.0.0) and a note in `PACKAGING.md`.
- Packages are published to GitHub Packages only from `main`, using the built-in `GITHUB_TOKEN`.
  No long-lived secrets in the repository.

*Rationale*: A build that can't be reproduced can't be debugged.

## Technology Constraints

- **Runtime**: .NET 10 (`net10.0`) for the app, the library, and the test projects.
- **UI**: Blazor WebAssembly with Blazor.Bootstrap and Bootstrap Icons; light/dark theme via
  Bootstrap `data-bs-theme`.
- **Report format**: Playwright JSON reporter output (`config`, `suites`, `stats`).
- **Cloud storage**: AWS S3, Azure Blob Storage, and Google Cloud Storage behind
  `ICloudStorageProvider`. New providers are added by implementing that interface.
- **Credentials**: Never hard-code or commit credentials. Use environment variables (e.g.,
  `AZURE_STORAGE_CONNECTION_STRING`), SAS/pre-signed URLs, or platform identity.

## Development Workflow (Spec-Driven)

All new features and significant changes follow Spec Kit:

1. `/speckit-specify <description>` creates `specs/NNN-short-name/spec.md` (what and why, no
   implementation detail).
2. `/speckit-clarify` (optional) resolves `[NEEDS CLARIFICATION]` markers before planning.
3. `/speckit-plan` produces `plan.md`, `research.md`, `data-model.md`, `contracts/`, and
   `quickstart.md`, and MUST pass the Constitution Check below.
4. `/speckit-tasks` produces `tasks.md`, ordered by user story priority.
5. `/speckit-analyze` (recommended) checks consistency across spec, plan, and tasks.
6. `/speckit-implement` executes tasks. Work happens on a branch named `NNN-short-name` and
   merges to `main` through a pull request.

Architecture decisions are recorded in the feature's `plan.md` / `research.md`, not in a
separate decision log. Specs for shipped features are updated when their behavior changes.

**Constitution Check gates** (every plan):

- [ ] Core logic lives in `Dotwright.Playwright`; UI only composes it (I)
- [ ] Works in the browser with no new required server, or the server is justified (II)
- [ ] Each acceptance scenario has a planned automated test (III)
- [ ] New projects, dependencies, or abstractions are justified in Complexity Tracking (IV)
- [ ] Lock files and versioning impact are addressed (V)

## Governance

This constitution supersedes ad-hoc practices. Pull requests MUST be checked against it, and
violations MUST be fixed or justified in the plan's Complexity Tracking table. Amendments are
made with `/speckit-constitution`, documented in the pull request, and versioned: MAJOR for
removing or redefining a principle, MINOR for adding a principle or section, PATCH for
clarifications.

**Version**: 1.0.0 | **Ratified**: 2026-11-01 | **Last Amended**: 2026-11-01
