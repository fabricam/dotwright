using System.Threading.Tasks;

namespace Dotwright.Web.Services
{
    public interface ISettingsService
    {
        Task<string?> GetPlaywrightReportPathAsync();
        Task SetPlaywrightReportPathAsync(string path);
    }
}