# Dotwright

Blazor WebAssembly dashboard for Playwright test results, plus the `Dotwright.Playwright`
library (published as `Fabricam.Dotwright.Playwright`) that parses Playwright JSON reports from
files, directories, and cloud storage.

## Spec-driven development

This repo uses [Spec Kit](https://github.com/github/spec-kit). Project principles and quality
gates are in [.specify/memory/constitution.md](.specify/memory/constitution.md). Read it before
planning any change.

New features go through the Spec Kit skills, in order:

1. `/speckit-specify <feature description>` creates `specs/NNN-short-name/spec.md`
2. `/speckit-clarify` resolves `[NEEDS CLARIFICATION]` markers (optional)
3. `/speckit-plan <tech guidance>` produces the plan, research, data model, and contracts
4. `/speckit-tasks` produces `tasks.md`
5. `/speckit-analyze` checks consistency across the artifacts (recommended)
6. `/speckit-implement` executes the tasks

Specs for shipped features (001–004) were back-filled from project history. Their `tasks.md`
files list open follow-ups. `specs/005-test-run-history` is a draft that still needs clarifying
and planning.

## Repository layout

```text
dotwright.slnx                  # solution: all app, library, and test projects
src/
├── Dotwright.Web/              # Blazor WASM dashboard (namespace: Dotwright.Web)
│   ├── Pages/ Layout/ Services/ Models/ wwwroot/
│   └── Program.cs
└── Dotwright.Playwright/       # report parsing library (NuGet: Fabricam.Dotwright.Playwright)
tests/
├── Dotwright.Playwright.Tests/ # xUnit
├── Dotwright.Settings.Tests/   # xUnit
└── playwright/                 # Playwright browser tests
docs/                           # packaging and publishing guides
specs/                          # Spec Kit feature specs
.specify/                       # Spec Kit constitution, templates, scripts
```

## Build and test

```powershell
dotnet build
dotnet test
dotnet run --project src/Dotwright.Web   # WASM dev server
```
