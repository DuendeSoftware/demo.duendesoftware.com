using Duende.IdentityServer.Services;
using Duende.IdentityServer.Validation;
using IdentityServerHost;
using Serilog;

namespace Duende.IdentityServer.Demo;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages();
        builder.Services.AddControllers();

        // cookie policy to deal with temporary browser incompatibilities
        builder.Services.AddSameSiteCookiePolicy();

        builder.Services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // options.KeyManagement.SigningAlgorithms = new[]
                // {
                //     new SigningAlgorithmOptions("RS256")
                //     {
                //         UseX509Certificate = true
                //     }
                // };
            })
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiResources(Config.ApiResources)
            .AddInMemoryClients(Config.Clients)
            .AddTestUsers(TestUsers.Users)
            .AddJwtBearerClientAuthentication();

        builder.Services.AddAuthentication()
            .AddLocalApi()
            .AddOpenIdConnect("Google", "Sign-in with Google", options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.ForwardSignOut = IdentityServerConstants.DefaultCookieAuthenticationScheme;

                options.Authority = "https://accounts.google.com/";
                options.ClientId = "708778530804-rhu8gc4kged3he14tbmonhmhe7a43hlp.apps.googleusercontent.com";

                options.CallbackPath = "/signin-google";
                options.Scope.Add("email");
            });

        // add CORS policy for non-IdentityServer endpoints
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("allow_all",
                policy => { policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); });
        });

        // demo versions (never use in production)
        builder.Services.AddTransient<IRedirectUriValidator, DemoRedirectValidator>();
        builder.Services.AddTransient<ICorsPolicyService, DemoCorsPolicy>();

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        app.UseCookiePolicy();
        app.UseDeveloperExceptionPage();

        app.UseCors("allow_all");

        app.UseStaticFiles();

        app.UseRouting();
        app.UseIdentityServer();
        app.UseAuthorization();


        app.MapRazorPages()
            .RequireAuthorization();
        app.MapControllers();

        return app;
    }
}