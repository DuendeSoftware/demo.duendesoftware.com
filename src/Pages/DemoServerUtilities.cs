using Duende.IdentityServer.Models;

namespace Duende.IdentityServer.Demo.Pages;

public static class DemoServerUtilities
{
    public static string HumanizeAllowedGrantTypes(ICollection<string> grantTypes)
    {
        if (grantTypes.SequenceEqual(GrantTypes.Implicit))
            return "implicit";

        if (grantTypes.SequenceEqual(GrantTypes.ImplicitAndClientCredentials))
            return "implicit and client credentials";

        if (grantTypes.SequenceEqual(GrantTypes.Code))
            return "authorization code";

        if (grantTypes.SequenceEqual(GrantTypes.CodeAndClientCredentials))
            return "authorization code and client credentials";

        if (grantTypes.SequenceEqual(GrantTypes.Hybrid))
            return "hybrid";

        if (grantTypes.SequenceEqual(GrantTypes.HybridAndClientCredentials))
            return "hybrid and client credentials";

        if (grantTypes.SequenceEqual(GrantTypes.ClientCredentials))
            return "client credentials";

        if (grantTypes.SequenceEqual(GrantTypes.ResourceOwnerPassword))
            return "resource owner password";

        if (grantTypes.SequenceEqual(GrantTypes.ResourceOwnerPasswordAndClientCredentials))
            return "resource owner password and client credentials";

        if (grantTypes.SequenceEqual(GrantTypes.DeviceFlow))
            return "device flow";

        if (grantTypes.SequenceEqual(GrantTypes.Ciba))
            return "ciba";

        return string.Join(" ", grantTypes);
    }

    public static string HumanizeLifetime(TimeSpan lifetime)
    {
        var components = new List<string>();
        if (lifetime.TotalHours > 0)
        {
            components.Add(Math.Floor(lifetime.TotalHours) + "h");
        }
        if (Math.Floor(lifetime.TotalMinutes % 60) > 0)
        {
            components.Add(Math.Floor(lifetime.TotalMinutes % 60) + "m");
        }
        if (Math.Floor(lifetime.TotalSeconds % 60) > 0)
        {
            components.Add(Math.Floor(lifetime.TotalSeconds % 60) + "s");
        }

        return string.Join(" ", components);
    }

    public static string? HumanizeClientIdPrefix(string clientIdPrefix) =>
        clientIdPrefix switch
        {
            "m2m" => "Machine-to-Machine",
            "interactive" => "Interactive clients",
            "native" => "Native clients",
            "device" => "Device flow",
            "login" => "Login",
            _ => clientIdPrefix
        };
}