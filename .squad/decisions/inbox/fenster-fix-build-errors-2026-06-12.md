Title: Convert top-level Program to explicit Program class
Date: 2026-06-12
Author: Fenster (fabricam)

Decision

To address solution-wide build conflicts caused by top-level statements in the Dotwright.Ingest project when building alongside other projects (different target frameworks), I converted the project’s top-level statements into an explicit internal static Program class with a Main method and static helpers.

Rationale

- Top-level statements introduce a single compilation unit with top-level state; when mixing projects/target frameworks in this solution the SDK can generate duplicate attributes or compilation conflicts. An explicit Program class avoids unexpected top-level state across the solution.
- Behavior preserved: the application still creates the WebApplication, maps the same endpoints, and ensures the SQLite database schema on startup.

Impact

- Minimal code layout change; runtime behavior unchanged.
- Improves compatibility when building the full solution under different SDK versions/target frameworks.

Alternatives considered

- Adjust global build settings or multi-targeting; more invasive.

