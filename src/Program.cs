using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;

namespace Duende.IdentityServer.Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "IdentityServer";

            BuildWebHostBuilder(args).Build().Run();
        }

        public static IHostBuilder BuildWebHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseSerilog((ctx, config) =>
                {
                    config.MinimumLevel.Debug()
                        .MinimumLevel.Debug()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .MinimumLevel.Override("System", LogEventLevel.Warning)
                        .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                        .Enrich.FromLogContext();

                    if (ctx.HostingEnvironment.IsDevelopment())
                    {
                        config.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}");
                    }
                    else if (ctx.HostingEnvironment.IsProduction())
                    {
                        config.WriteTo.File(@"/home/LogFiles/Application/identityserver.txt",
                            fileSizeLimitBytes: 1_000_000,
                            rollOnFileSizeLimit: true,
                            shared: true,
                            flushToDiskInterval: TimeSpan.FromSeconds(1));
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}
