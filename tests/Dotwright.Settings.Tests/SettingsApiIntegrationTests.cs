using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace Dotwright.Settings.Tests
{
    public class SettingsApiIntegrationTests : IDisposable
    {
        private readonly string _tempDir;
        private readonly string _settingsFile;
        private readonly TestServer _server;
        private readonly HttpClient _client;

        public SettingsApiIntegrationTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "dotwright_settings_tests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);
            _settingsFile = Path.Combine(_tempDir, "settings.txt");

            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.Run(async ctx =>
                    {
                        if (ctx.Request.Path == "/api/settings/filepath" && ctx.Request.Method == "POST")
                        {
                            using var sr = new StreamReader(ctx.Request.Body);
                            var body = await sr.ReadToEndAsync();
                            var doc = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
                            if (doc != null && doc.TryGetValue("path", out var path))
                            {
                                await File.WriteAllTextAsync(_settingsFile, path);
                                ctx.Response.StatusCode = 200;
                                await ctx.Response.WriteAsync(string.Empty);
                                return;
                            }
                            ctx.Response.StatusCode = 400;
                            return;
                        }

                        if (ctx.Request.Path == "/api/settings/filepath" && ctx.Request.Method == "GET")
                        {
                            if (File.Exists(_settingsFile))
                            {
                                var path = await File.ReadAllTextAsync(_settingsFile);
                                ctx.Response.ContentType = "application/json";
                                await ctx.Response.WriteAsync(JsonSerializer.Serialize(new { path }));
                                return;
                            }
                            ctx.Response.StatusCode = 404;
                            return;
                        }

                        ctx.Response.StatusCode = 404;
                    });
                });

            _server = new TestServer(builder);
            _client = _server.CreateClient();
        }

        [Fact]
        public async Task PostThenGet_ReturnsStoredPath()
        {
            var payload = new { path = "C:\\temp\\somefile.txt" };
            var resp = await _client.PostAsJsonAsync("/api/settings/filepath", payload);
            resp.EnsureSuccessStatusCode();

            var getResp = await _client.GetAsync("/api/settings/filepath");
            getResp.EnsureSuccessStatusCode();

            var obj = await getResp.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            Assert.NotNull(obj);
            Assert.True(obj.ContainsKey("path"));
            Assert.Equal(payload.path, obj["path"]);
        }

        public void Dispose()
        {
            _client.Dispose();
            _server.Dispose();
            try
            {
                if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true);
            }
            catch
            {
                // ignore cleanup errors
            }
        }
    }
}
