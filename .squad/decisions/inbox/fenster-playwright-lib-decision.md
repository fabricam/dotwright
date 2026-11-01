Decision: Create Dotwright.Playwright class library for Playwright report parsing

- Target framework: net10.0 (matches Blazor WASM host)
- Namespace: Dotwright.Playwright for library types; models under Dotwright.Playwright.Models
- Lifetime: IPlaywrightReportReader registered as Singleton (stateless, thread-safe, performs file I/O)
- Registration: library exposes AddPlaywrightReporting(IServiceCollection) extension in Microsoft.Extensions.DependencyInjection namespace.

Rationale:
- net10.0 keeps parity with host project. The reader is stateless and holds only JsonSerializerOptions; singleton avoids repeated allocation.
- The reader uses System.IO (File.OpenRead) and cannot read host filesystem in Blazor WebAssembly; register on server-side host only. The extension is provided for server registration; calling AddPlaywrightReporting in WASM will compile but file APIs won't work at runtime.

Next steps:
- Package the library into NuGet or reference project in server app. Ensure server-side startup calls services.AddPlaywrightReporting().
