using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Duende.IdentityServer.Models;

namespace Duende.IdentityServer.Demo
{
    public class Config
    {
        private static List<string> AllIdentityScopes =>
            IdentityResources.Select(s => s.Name).ToList();

        private static List<string> AllApiScopes =>
            ApiScopes.Select(s => s.Name).ToList();

        private static List<string> AllScopes =>
            AllApiScopes.Concat(AllIdentityScopes).ToList();
        
        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                // backward compat
                new ApiScope("api"),
                
                // resource specific scopes
                new ApiScope("resource1.scope1"),
                new ApiScope("resource1.scope2"),
                
                new ApiScope("resource2.scope1"),
                new ApiScope("resource2.scope2"),
                
                new ApiScope("resource3.scope1"),
                new ApiScope("resource3.scope2"),
                
                // a scope without resource association
                new ApiScope("scope3"),
                new ApiScope("scope4"),
                
                // a scope shared by multiple resources
                new ApiScope("shared.scope"),

                // a parameterized scope
                new ApiScope("transaction", "Transaction")
                {
                    Description = "Some Transaction"
                }
            };

        public static IEnumerable<ApiResource> ApiResources =>
            new List<ApiResource>
            {
                new ApiResource("api", "Demo API", new[] { "name", "email" })
                {
                    ApiSecrets = { new Secret("secret".Sha256()) },
                    
                    Scopes = { "api" }
                },
                
                new ApiResource("urn:resource1", "Resource 1")
                {
                    ApiSecrets = { new Secret("secret".Sha256()) },

                    Scopes = { "resource1.scope1", "resource1.scope2", "shared.scope" }
                },
                
                new ApiResource("urn:resource2", "Resource 2")
                {
                    ApiSecrets = { new Secret("secret".Sha256()) },

                    Scopes = { "resource2.scope1", "resource2.scope2", "shared.scope" }
                },
                
                new ApiResource("urn:resource3", "Resource 3 (isolated)")
                {
                    ApiSecrets = { new Secret("secret".Sha256()) },
                    RequireResourceIndicator = true,
                    
                    Scopes = { "resource3.scope1", "resource3.scope2", "shared.scope" }
                }
            };

        /// <summary>
        /// A well-known test certificate for SAML SP signing (e.g. SLO requests).
        /// The corresponding private key is published on the demo home page so that
        /// anyone testing a SAML SP against this IdP can sign their logout requests.
        /// This is a base64-encoded PFX with password "demo".
        /// </summary>
        private static readonly X509Certificate2 SamlSpSigningCertificate = X509CertificateLoader.LoadPkcs12(
            Convert.FromBase64String(
                "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"),
            "demo");

        private static Secret PublicKey = new Secret
        {
            Type = IdentityServerConstants.SecretTypes.JsonWebKey,
            Value = 
                """
                {
                    "e":"AQAB",
                    "kid":"ZzAjSnraU3bkWGnnAqLapYGpTyNfLbjbzgAPbbW2GEA",
                    "kty":"RSA",
                    "n":"wWwQFtSzeRjjerpEM5Rmqz_DsNaZ9S1Bw6UbZkDLowuuTCjBWUax0vBMMxdy6XjEEK4Oq9lKMvx9JzjmeJf1knoqSNrox3Ka0rnxXpNAz6sATvme8p9mTXyp0cX4lF4U2J54xa2_S9NF5QWvpXvBeC4GAJx7QaSw4zrUkrc6XyaAiFnLhQEwKJCwUw4NOqIuYvYp_IXhw-5Ti_icDlZS-282PcccnBeOcX7vc21pozibIdmZJKqXNsL1Ibx5Nkx1F1jLnekJAmdaACDjYRLL_6n3W4wUp19UvzB1lGtXcJKLLkqB6YDiZNu16OSiSprfmrRXvYmvD8m6Fnl5aetgKw"
                }
                """
        };

        public static IEnumerable<Client> Clients =>
            new List<Client>
            {
                // non-interactive
                new Client
                {
                    ClientId = "m2m",
                    ClientName = "Machine to machine (client credentials)",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = AllApiScopes,
                },
                new Client
                {
                    ClientId = "m2m.jwt",
                    ClientName = "Machine to machine (client credentials with JWT)",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = AllApiScopes,

                    ClientSecrets = { PublicKey }
                },
                new Client
                {
                    ClientId = "m2m.dpop",
                    ClientName = "Machine to machine (client credentials)",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = AllApiScopes,

                    RequireDPoP = true,
                },
                new Client
                {
                    ClientId = "m2m.dpop.nonce",
                    ClientName = "Machine to machine (client credentials)",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = AllApiScopes,

                    RequireDPoP = true,
                    DPoPValidationMode = DPoPTokenExpirationValidationMode.Nonce,
                },
                new Client
                {
                    ClientId = "m2m.short",
                    ClientName = "Machine to machine with short access token lifetime (client credentials)",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = AllApiScopes,
                    AccessTokenLifetime = 75
                },
                new Client
                {
                    ClientId = "m2m.short.jwt",
                    ClientName = "Machine to machine (client credentials with JWT)",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = AllApiScopes,
                    AccessTokenLifetime = 75,

                    ClientSecrets = { PublicKey }
                },

                // interactive
                new Client
                {
                    ClientId = "interactive.confidential",
                    ClientName = "Interactive client (Code with PKCE)",

                    RedirectUris = { "https://notused" },
                    PostLogoutRedirectUris = { "https://notused" },

                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                    AllowedScopes = AllScopes,

                    AllowOfflineAccess = true,
                    RefreshTokenUsage = TokenUsage.ReUse,
                    RefreshTokenExpiration = TokenExpiration.Sliding
                },

                new Client
                {
                    ClientId = "interactive.confidential.jwt",
                    ClientName = "Interactive client (Code with PKCE) using private key JWT authentication",

                    RedirectUris = { "https://notused" },
                    PostLogoutRedirectUris = { "https://notused" },

                    ClientSecrets = { PublicKey },

                    AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                    RequireRequestObject = false,
                    AllowedScopes = AllScopes,

                    AllowOfflineAccess = true,
                    RefreshTokenUsage = TokenUsage.ReUse,
                    RefreshTokenExpiration = TokenExpiration.Sliding
                },

                new Client
                {
                    ClientId = "interactive.confidential.jwt.dpop",
                    ClientName = "Interactive client (Code with PKCE) using private key JWT authentication and requiring DPoP with server-issued nonces",

                    RedirectUris = { "https://notused" },
                    PostLogoutRedirectUris = { "https://notused" },

                    ClientSecrets = { PublicKey },

                    AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                    RequireRequestObject = false,
                    AllowedScopes = AllScopes,

                    AllowOfflineAccess = true,
                    RefreshTokenUsage = TokenUsage.ReUse,
                    RefreshTokenExpiration = TokenExpiration.Sliding,

                    RequireDPoP = true,
                    DPoPValidationMode = DPoPTokenExpirationValidationMode.Nonce,

                    AccessTokenLifetime = 75
                },

                new Client
                {
                    ClientId = "interactive.confidential.jar.jwt",
                    ClientName = "Interactive client (Code with PKCE) using JAR and private key JWT",

                    RedirectUris = { "https://notused" },
                    PostLogoutRedirectUris = { "https://notused" },

                    ClientSecrets = { PublicKey },

                    AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                    RequireRequestObject = true,
                    AllowedScopes = AllScopes,

                    AllowOfflineAccess = true,
                    RefreshTokenUsage = TokenUsage.ReUse,
                    RefreshTokenExpiration = TokenExpiration.Sliding
                },

                new Client
                {
                    ClientId = "interactive.confidential.short",
                    ClientName = "Interactive client with short token lifetime (Code with PKCE)",

                    RedirectUris = { "https://notused" },
                    PostLogoutRedirectUris = { "https://notused" },

                    ClientSecrets = { new Secret("secret".Sha256()) },
                    RequireConsent = false,

                    AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                    RequirePkce = true,
                    AllowedScopes = AllScopes,

                    AllowOfflineAccess = true,
                    RefreshTokenUsage = TokenUsage.ReUse,
                    RefreshTokenExpiration = TokenExpiration.Sliding,

                    AccessTokenLifetime = 75
                },

                new Client
                {
                    ClientId = "interactive.confidential.short.jwt",
                    ClientName = "Interactive client (Code with PKCE) using private key JWT authentication with short access token lifetime",

                    RedirectUris = { "https://notused" },
                    PostLogoutRedirectUris = { "https://notused" },

                    ClientSecrets = { PublicKey },

                    AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                    RequireRequestObject = false,
                    AllowedScopes = AllScopes,

                    AllowOfflineAccess = true,
                    RefreshTokenUsage = TokenUsage.ReUse,
                    RefreshTokenExpiration = TokenExpiration.Sliding,

                    AccessTokenLifetime = 75
                },

                new Client
                {
                    ClientId = "interactive.confidential.short.jar.jwt",
                    ClientName = "Interactive client (Code with PKCE) using JAR and private key JWT",

                    RedirectUris = { "https://notused" },
                    PostLogoutRedirectUris = { "https://notused" },

                    ClientSecrets = { PublicKey },

                    AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                    RequireRequestObject = true,
                    AllowedScopes = AllScopes,

                    AllowOfflineAccess = true,
                    RefreshTokenUsage = TokenUsage.ReUse,
                    RefreshTokenExpiration = TokenExpiration.Sliding,

                    AccessTokenLifetime = 75
                },

                new Client
                {
                    ClientId = "interactive.public",
                    ClientName = "Interactive client (Code with PKCE)",

                    RedirectUris = { "https://notused" },
                    PostLogoutRedirectUris = { "https://notused" },

                    RequireClientSecret = false,

                    AllowedGrantTypes = GrantTypes.Code,
                    AllowedScopes = AllScopes,

                    AllowOfflineAccess = true,
                    RefreshTokenUsage = TokenUsage.OneTimeOnly,
                    RefreshTokenExpiration = TokenExpiration.Sliding
                },

                new Client
                {
                    ClientId = "native.dpop",
                    ClientName = "Native client (Code with PKCE + DPop)",

                    RedirectUris = { "https://notused" },
                    PostLogoutRedirectUris = { "https://notused" },

                    RequireClientSecret = false,

                    AllowedGrantTypes = GrantTypes.Code,
                    AllowedScopes = AllScopes,

                    AllowOfflineAccess = true,
                    RefreshTokenUsage = TokenUsage.ReUse,
                    RefreshTokenExpiration = TokenExpiration.Sliding,

                    RequireDPoP = true,
                    DPoPValidationMode = DPoPTokenExpirationValidationMode.Nonce
                },

                new Client
                {
                    ClientId = "interactive.public.short",
                    ClientName = "Interactive client with short token lifetime (Code with PKCE)",

                    RedirectUris = { "https://notused" },
                    PostLogoutRedirectUris = { "https://notused" },

                    RequireClientSecret = false,

                    AllowedGrantTypes = GrantTypes.Code,
                    AllowedScopes = AllScopes,

                    AllowOfflineAccess = true,
                    RefreshTokenUsage = TokenUsage.OneTimeOnly,
                    RefreshTokenExpiration = TokenExpiration.Sliding,

                    AccessTokenLifetime = 75
                },

                // not recommended - only for clients that do not support PKCE
                new Client
                {
                    ClientId = "interactive.confidential.nopkce",
                    ClientName = "Interactive client (Code without PKCE)",

                    RedirectUris = { "https://notused" },
                    PostLogoutRedirectUris = { "https://notused" },

                    ClientSecrets = { new Secret("secret".Sha256()) },
                    RequirePkce = false,

                    AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                    AllowedScopes = AllScopes,

                    AllowOfflineAccess = true,
                    RefreshTokenUsage = TokenUsage.ReUse,
                    RefreshTokenExpiration = TokenExpiration.Sliding
                },

                // hybrid as alternative to PKCE
                new Client
                {
                    ClientId = "interactive.confidential.hybrid",
                    ClientName = "Interactive client (Code with Hybrid Flow)",

                    RedirectUris = { "https://notused" },
                    PostLogoutRedirectUris = { "https://notused" },

                    ClientSecrets = { new Secret("secret".Sha256()) },
                    RequirePkce = false,

                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                    AllowedScopes = AllScopes,

                    AllowOfflineAccess = true,
                    RefreshTokenUsage = TokenUsage.ReUse,
                    RefreshTokenExpiration = TokenExpiration.Sliding
                },

                new Client
                {
                    ClientId = "device",
                    ClientName = "Device Flow Client",

                    AllowedGrantTypes = GrantTypes.DeviceFlow,
                    RequireClientSecret = false,

                    AllowOfflineAccess = true,
                    RefreshTokenUsage = TokenUsage.OneTimeOnly,
                    RefreshTokenExpiration = TokenExpiration.Sliding,

                    AllowedScopes = AllScopes,
                },
                
                // oidc login only
                new Client
                {
                    ClientId = "interactive.implicit",

                    RedirectUris = { "https://notused" },
                    PostLogoutRedirectUris = { "https://notused" },

                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowedScopes = AllIdentityScopes,
                },

                // oidc login only - legacy client name. Keep until end of 2026
                // to not break existing demos.
                new Client
                {
                    ClientId = "login",

                    RedirectUris = { "https://notused" },
                    PostLogoutRedirectUris = { "https://notused" },

                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowedScopes = AllIdentityScopes,
                }
            };

        public static IEnumerable<SamlServiceProvider> SamlServiceProviders =>
            new List<SamlServiceProvider>
            {
                // basic SP - minimal config with HTTP-POST binding
                new SamlServiceProvider
                {
                    EntityId = "https://saml-sp1.example.com",
                    DisplayName = "Simple SAML SP",
                    AssertionConsumerServiceUrls = new List<IndexedEndpoint>
                    {
                        new IndexedEndpoint { Location = "https://saml-sp1.example.com/acs", Binding = SamlBinding.HttpPost, Index = 0, IsDefault = true }
                    },
                    AllowedScopes = { "openid", "profile" },
                },

                // SP with Single Logout
                new SamlServiceProvider
                {
                    EntityId = "https://saml-sp2.example.com",
                    DisplayName = "SAML SP with SLO",
                    AssertionConsumerServiceUrls = new List<IndexedEndpoint>
                    {
                        new IndexedEndpoint { Location = "https://saml-sp2.example.com/acs", Binding = SamlBinding.HttpPost, Index = 0, IsDefault = true }
                    },
                    AllowedScopes = { "openid", "profile" },
                    SingleLogoutServiceUrls = new List<SamlEndpointType>
                    {
                        new SamlEndpointType { Location = "https://saml-sp2.example.com/saml/slo", Binding = SamlBinding.HttpRedirect }
                    },
                    Certificates = new List<ServiceProviderCertificate>
                    {
                        new ServiceProviderCertificate { Certificate = SamlSpSigningCertificate, Use = KeyUse.Signing }
                    },
                },

                // SP with IdP-initiated SSO enabled
                new SamlServiceProvider
                {
                    EntityId = "https://saml-sp3.example.com",
                    DisplayName = "SAML SP (IdP-initiated)",
                    AssertionConsumerServiceUrls = new List<IndexedEndpoint>
                    {
                        new IndexedEndpoint { Location = "https://saml-sp3.example.com/acs", Binding = SamlBinding.HttpPost, Index = 0, IsDefault = true }
                    },
                    AllowIdpInitiated = true,
                    AllowedScopes = { "openid", "profile" },
                    SingleLogoutServiceUrls = new List<SamlEndpointType>
                    {
                        new SamlEndpointType { Location = "https://saml-sp3.example.com/saml/slo", Binding = SamlBinding.HttpRedirect }
                    },
                    Certificates = new List<ServiceProviderCertificate>
                    {
                        new ServiceProviderCertificate { Certificate = SamlSpSigningCertificate, Use = KeyUse.Signing }
                    },
                },
            };
    }
}
