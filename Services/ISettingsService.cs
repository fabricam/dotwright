using System.Threading.Tasks;

namespace Dotwright.Services
{
    public interface ISettingsService
    {
        Task<string?> GetPlaywrightReportPathAsync();
        Task SetPlaywrightReportPathAsync(string path);
    }
}