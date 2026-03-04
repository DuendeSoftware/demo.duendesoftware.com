using System.Net;
using System.Text.Json;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using Xunit;

namespace SmokeTests;

public class EndpointSmokeTests : IClassFixture<DemoWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EndpointSmokeTests(DemoWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Homepage_Returns_Ok_With_Expected_Content()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var document = await response.ParseHtmlAsync();

        // Verify key headings exist
        document.QuerySelector("h2#demo-clients")?.TextContent
            .ShouldContain("Demo OAuth/OIDC Clients");
        document.QuerySelector("h2#saml-service-providers")?.TextContent
            .ShouldContain("Demo SAML Service Providers");

        // Verify discovery and SAML metadata links
        document.QuerySelector("a[href='/.well-known/openid-configuration']")
            .ShouldNotBeNull("Discovery document link should exist");
        document.QuerySelector("a[href='/saml/metadata']")
            .ShouldNotBeNull("SAML metadata link should exist");
    }

    [Fact]
    public async Task OidcDiscovery_Returns_Valid_Json()
    {
        // Act
        var response = await _client.GetAsync("/.well-known/openid-configuration");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("issuer").GetString().ShouldNotBeNullOrEmpty();
        doc.RootElement.GetProperty("authorization_endpoint").GetString().ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task SamlMetadata_Returns_Valid_Xml()
    {
        // Act
        var response = await _client.GetAsync("/saml/metadata");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var contentType = response.Content.Headers.ContentType?.MediaType;
        contentType.ShouldNotBeNull();
        contentType.ShouldContain("xml");

        var xml = await response.Content.ReadAsStringAsync();
        xml.ShouldContain("EntityDescriptor");
    }

    [Fact]
    public async Task LoginPage_Returns_Ok_With_Form()
    {
        // Act
        var response = await _client.GetAsync("/account/login");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var document = await response.ParseHtmlAsync();

        // Verify login form structure
        document.QuerySelector("h1")?.TextContent.ShouldContain("Login");
        document.QuerySelector("input[name='Input.Username']").ShouldNotBeNull();
        document.QuerySelector("input[name='Input.Password']").ShouldNotBeNull();
        document.QuerySelector("button[value='login']").ShouldNotBeNull();
    }

    [Fact]
    public async Task DiagnosticsPage_Redirects_To_Login_When_Unauthenticated()
    {
        // Act
        var response = await _client.GetAsync("/diagnostics");

        // Assert — expect redirect to login
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().ShouldContain("/account/login");
    }

    [Fact]
    public async Task GrantsPage_Redirects_To_Login_When_Unauthenticated()
    {
        // Act
        var response = await _client.GetAsync("/grants");

        // Assert — expect redirect to login
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().ShouldContain("/account/login");
    }
}
