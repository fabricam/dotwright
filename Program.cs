using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using dotwright;
using Microsoft.Extensions.DependencyInjection; // for AddPlaywrightReporting extension when registered server-side

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddBlazorBootstrap();

// Register Playwright reporting and settings helpers. Intended for server-side hosts; in Blazor WebAssembly this will only register types
// — file-backed ISettingsService requires a server host that can access the repository filesystem to be useful.
// If you host an ASP.NET Core server, call services.AddPlaywrightReporting() in that host's Program.cs instead.
// For quick local testing in a server-enabled host, you may uncomment the following line to register server services here:
// builder.Services.AddPlaywrightReporting();


// Settings stored in browser localStorage (WASM). Service registered here for DI.
builder.Services.AddScoped<Dotwright.Services.ISettingsService, Dotwright.Services.SettingsService>();

// NOTE: Playwright report reader performs filesystem I/O and is intended for server-side hosting.
// If you have a server host (ASP.NET Core) register the service there with:
//    services.AddPlaywrightReporting();
// Do NOT call AddPlaywrightReporting() in Blazor WebAssembly if you expect FromFileAsync to work at runtime.

await builder.Build().RunAsync();
