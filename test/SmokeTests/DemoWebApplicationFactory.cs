using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace SmokeTests;

public sealed class DemoWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Replace Serilog with the default ASP.NET Core logger so that
        // parallel WebApplicationFactory instances don't hit the
        // ReloadableLogger.Freeze() "already frozen" exception.
        builder.ConfigureServices(services =>
        {
            services.AddSerilog(_ => { }, preserveStaticLogger: true);

            // Override IdentityServer options for test isolation
            services.PostConfigure<Duende.IdentityServer.Configuration.IdentityServerOptions>(options =>
            {
                // Use a temp directory for key material so tests don't
                // interfere with each other or with real key storage.
                var keyPath = Path.Combine(Path.GetTempPath(), "demo-smoke-tests", Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(keyPath);
                options.KeyManagement.KeyPath = keyPath;
            });
        });

        // Set environment to Development so developer exception page is active
        builder.UseEnvironment("Development");
    }
}
