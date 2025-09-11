namespace Duende.IdentityServer.Demo;

public static class RunningEnvironmentConfiguration
{
    public static string ApplicationVersion => Environment.GetEnvironmentVariable("APPLICATION_VERSION") ?? "unknown";
}