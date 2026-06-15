using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Dotwright.Services
{
    public class SettingsService : ISettingsService
    {
        private const string Key = "dotwright.playwrightReportPath";
        private readonly IJSRuntime _js;

        public SettingsService(IJSRuntime js) => _js = js;

        public async Task<string?> GetPlaywrightReportPathAsync()
        {
            return await _js.InvokeAsync<string?>("localStorage.getItem", Key);
        }

        public async Task SetPlaywrightReportPathAsync(string path)
        {
            await _js.InvokeVoidAsync("localStorage.setItem", Key, path ?? string.Empty);
        }
    }
}