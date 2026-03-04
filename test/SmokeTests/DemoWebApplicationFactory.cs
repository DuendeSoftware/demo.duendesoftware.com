using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace SmokeTests;

public class DemoWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
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
