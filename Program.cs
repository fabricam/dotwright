using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using dotwright;
using Microsoft.Extensions.DependencyInjection; // for AddPlaywrightReporting extension when registered server-side

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddBlazorBootstrap();

// Settings stored in browser localStorage (WASM). Service registered here for DI.
builder.Services.AddScoped<Dotwright.Services.ISettingsService, Dotwright.Services.SettingsService>();

// NOTE: Playwright report reader performs filesystem I/O and is intended for server-side hosting.
// If you have a server host (ASP.NET Core) register the service there with:
//    services.AddPlaywrightReporting();
// Do NOT call AddPlaywrightReporting() in Blazor WebAssembly if you expect FromFileAsync to work at runtime.

await builder.Build().RunAsync();
