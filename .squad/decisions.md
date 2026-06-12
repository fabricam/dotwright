# Squad Decisions

## Active Decisions

### 1. Persistence Approach for MVP
- **Decision:** Use file-backed SQLite database (EF Core provider) for the MVP persistence layer.
- **Rationale:** Simple to run locally, minimal infra, cross-platform, supports queries and migrations later. Enables straightforward development and CI experience without external services.
- **Next Steps:** Implement EF Core models with single connection string defaulting to local file (e.g., dotwright.db) in development. Use unique artifact checksum or provider-run-id to deduplicate imports.

### 2. Domain Model Shape (High Level)
- **Decision:** Implement normalized domain model with primary entities: TestRun, TestSuite, TestFile, TestCase, TestResult, Attachment, ProviderSource.
- **Rationale:** Normalized model supports queries for failures, flaky detection, and linking attachments without duplicating data across runs.
- **Notes:** Attachment binary payloads not stored in DB for MVP. Store references only (provider URL, file path, signed URL). Ingestion pipeline records original artifact metadata for traceability.

### 3. Artifact Handling
- **Decision:** Treat traces, screenshots, and videos as external references (links or browser previews). Local development uses simple file paths; CI-provider imports record provider artifact metadata and temporary signed URLs when supported.
- **Rationale:** Avoids large binary storage complexity for MVP, keeps ingestion pipeline lightweight.

### 4. Flaky Detection Heuristic
- **Decision:** MVP implementation: test is flaky if it has both pass and fail outcomes within last N runs (configurable window, default N=10).
- **Rationale:** Easy to compute and sufficient for initial UX. Refinements can be added later (rate, regression windows, weighted scoring).

## Build Triage (2026-06-12)

### Issue Summary
`dotnet build` fails with 13 errors due to conflicting top-level Program.cs files and duplicate assembly metadata.

### Error Classification & Owners

**1. Architecture / Code Issue → fenster (Backend Dev)**
- **Error:** CS8802 - Only one compilation unit can have top-level statements
- **Root Cause:** Two `Program.cs` files with top-level statements (main Blazor WASM: `C:\repos\dotwright\Program.cs` net10.0, Ingest service: `C:\repos\dotwright\src\Dotwright.Ingest\Program.cs` net7.0)
- **Fix:** Separate projects must be built independently. Recommendation: Remove top-level statements from Ingest, use traditional Program class.

**2. Missing ASP.NET Core References → fenster (Backend Dev)**
- **Error:** CS0234 - Missing `Microsoft.AspNetCore.Builder`, `Microsoft.AspNetCore.Hosting`, `Microsoft.Extensions.Hosting`
- **Root Cause:** Dotwright.Ingest.csproj (net7.0 Web SDK) missing explicit ASP.NET Core package references
- **Fix:** Add packages to Dotwright.Ingest.csproj or verify Web SDK ImplicitUsings/package references are correct

**3. Duplicate Assembly Attributes → boltz (DevOps)**
- **Error:** 8x CS0579 - Duplicate global/System.Reflection attributes in build artifacts
- **Root Cause:** Csproj generates duplicates in both `.NETCoreApp,Version=v10.0.AssemblyAttributes.cs` and `dotwright.AssemblyInfo.cs`
- **Fix:** Clean build artifacts (`dotnet clean && dotnet build`). Check for duplicate `<GenerateAssemblyInfo>` or conflicting properties in csproj.

### Blocked Work
- **#7 MVP: Implement Playwright report artifact ingestion** (depends on clean build)
- **#8 MVP: Add persistence layer** (depends on Ingest service compiling)

---

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
