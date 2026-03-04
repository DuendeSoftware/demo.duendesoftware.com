using System.Net;
using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using Xunit;

namespace SmokeTests;

public sealed class SamlSsoTests : IClassFixture<DemoWebApplicationFactory>
{
    private static readonly XNamespace Samlp = "urn:oasis:names:tc:SAML:2.0:protocol";
    private static readonly XNamespace Saml = "urn:oasis:names:tc:SAML:2.0:assertion";

    private readonly DemoWebApplicationFactory _factory;

    public SamlSsoTests(DemoWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AuthenticatedIdpInitiatedReturnsSamlResponseWithValidAssertion()
    {
        // Arrange
        var client = CreateClient();
        var cookies = await LoginAsync(client, "alice", "alice");

        // Act — Step 1: Hit IdP-initiated endpoint
        var idpInitiatedRequest = CreateRequest(
            HttpMethod.Get,
            "/saml/idp-initiated?spEntityId=https://saml-sp3.example.com",
            cookies);
        var idpInitiatedResponse = await client.SendAsync(idpInitiatedRequest);

        // Should redirect to signin_callback
        idpInitiatedResponse.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        var callbackLocation = idpInitiatedResponse.Headers.Location?.ToString();
        callbackLocation.ShouldNotBeNull();
        callbackLocation.ShouldContain("/saml/signin_callback");

        // Collect cookies from this response (SAML state cookie)
        var allCookies = MergeCookies(cookies, ExtractCookies(idpInitiatedResponse));

        // Act — Step 2: Follow redirect to signin_callback
        var callbackRequest = CreateRequest(HttpMethod.Get, callbackLocation, allCookies);
        var callbackResponse = await client.SendAsync(callbackRequest);

        callbackResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Parse the SAML response from the HTML form
        var samlXml = await ExtractSamlResponseFromHtmlAsync(callbackResponse);

        // Assert — SAML Response structure
        var response = samlXml.Root;
        response.ShouldNotBeNull();
        response.Name.ShouldBe(Samlp + "Response");
        response.Attribute("Version")?.Value.ShouldBe("2.0");
        response.Attribute("Destination")?.Value.ShouldBe("https://saml-sp3.example.com/acs");

        // Issuer
        var responseIssuer = response.Element(Saml + "Issuer");
        responseIssuer.ShouldNotBeNull();
        responseIssuer.Value.ShouldNotBeNullOrEmpty();

        // Status
        var status = response.Element(Samlp + "Status");
        status.ShouldNotBeNull();
        var statusCode = status.Element(Samlp + "StatusCode");
        statusCode.ShouldNotBeNull();
        statusCode.Attribute("Value")?.Value.ShouldBe("urn:oasis:names:tc:SAML:2.0:status:Success");

        // Assertion
        var assertion = response.Element(Saml + "Assertion");
        assertion.ShouldNotBeNull();

        // Assertion Issuer matches Response Issuer
        var assertionIssuer = assertion.Element(Saml + "Issuer");
        assertionIssuer.ShouldNotBeNull();
        assertionIssuer.Value.ShouldBe(responseIssuer.Value);

        // Subject/NameID — alice's SubjectId is "1"
        var subject = assertion.Element(Saml + "Subject");
        subject.ShouldNotBeNull();
        var nameId = subject.Element(Saml + "NameID");
        nameId.ShouldNotBeNull();
        nameId.Value.ShouldBe("1");

        // Conditions/AudienceRestriction
        var conditions = assertion.Element(Saml + "Conditions");
        conditions.ShouldNotBeNull();
        var audienceRestriction = conditions.Element(Saml + "AudienceRestriction");
        audienceRestriction.ShouldNotBeNull();
        var audience = audienceRestriction.Element(Saml + "Audience");
        audience.ShouldNotBeNull();
        audience.Value.ShouldBe("https://saml-sp3.example.com");

        // AuthnStatement
        var authnStatement = assertion.Element(Saml + "AuthnStatement");
        authnStatement.ShouldNotBeNull();
        authnStatement.Attribute("SessionIndex")?.Value.ShouldNotBeNullOrEmpty();

        // AttributeStatement — verify alice's name claim.
        // Note: the login flow only puts sub + name on the ClaimsPrincipal,
        // so only the "name" claim (which is in the default SAML mapping) appears
        // in the assertion. Claims like "email" are on the TestUser but not on
        // the auth cookie principal, so the profile service never issues them.
        GetAttributeValue(assertion, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
            .ShouldBe("Alice Smith");

        // Verify the HTML form posts to the correct ACS URL
        var document = await callbackResponse.ParseHtmlAsync();
        var form = document.QuerySelector("form[method='post']");
        form.ShouldNotBeNull();
        form.GetAttribute("action").ShouldBe("https://saml-sp3.example.com/acs");
    }

    [Fact]
    public async Task AuthenticatedIdpInitiatedWithBobReturnsBobsClaims()
    {
        // Arrange
        var client = CreateClient();
        var cookies = await LoginAsync(client, "bob", "bob");

        // Act — follow the full IdP-initiated redirect chain
        var samlXml = await ExecuteIdpInitiatedFlowAsync(client, cookies, "https://saml-sp3.example.com");

        // Assert — bob's claims
        var assertion = samlXml.Root?.Element(Saml + "Assertion");
        assertion.ShouldNotBeNull();

        var nameId = assertion.Element(Saml + "Subject")?.Element(Saml + "NameID");
        nameId.ShouldNotBeNull();
        nameId.Value.ShouldBe("2");

        GetAttributeValue(assertion, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
            .ShouldBe("Bob Smith");
    }

    [Fact]
    public async Task IdpInitiatedUnauthenticatedRedirectsToLogin()
    {
        // Arrange
        var client = CreateClient();

        // Act — hit IdP-initiated endpoint with no auth cookies
        var response = await client.GetAsync("/saml/idp-initiated?spEntityId=https://saml-sp3.example.com");

        // Assert — should redirect to login
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        var location = response.Headers.Location?.ToString();
        location.ShouldNotBeNull();
        location.ShouldContain("/account/login", Case.Insensitive);
    }

    [Fact]
    public async Task IdpInitiatedNonIdpInitiatedSpReturnsError()
    {
        // Arrange — SP1 does not allow IdP-initiated SSO
        var client = CreateClient();

        // Act
        var response = await client.GetAsync("/saml/idp-initiated?spEntityId=https://saml-sp1.example.com");

        // Assert — should return an error (400-level)
        ((int)response.StatusCode).ShouldBeInRange(400, 499);
    }

    [Fact]
    public async Task IdpInitiatedUnknownSpReturnsError()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.GetAsync("/saml/idp-initiated?spEntityId=https://unknown.example.com");

        // Assert — should return an error (400-level)
        ((int)response.StatusCode).ShouldBeInRange(400, 499);
    }

    [Fact]
    public async Task IdpInitiatedMissingSpEntityIdReturnsError()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.GetAsync("/saml/idp-initiated");

        // Assert — should return an error (400-level)
        ((int)response.StatusCode).ShouldBeInRange(400, 499);
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    private static HttpRequestMessage CreateRequest(HttpMethod method, string uri, Dictionary<string, string> cookies)
    {
        var request = new HttpRequestMessage(method, uri);
        if (cookies.Count > 0)
        {
            request.Headers.Add("Cookie", string.Join("; ", cookies.Select(c => $"{c.Key}={c.Value}")));
        }

        return request;
    }

    /// <summary>
    /// Authenticates a test user by performing the login form POST with anti-forgery token handling.
    /// Returns a dictionary of cookies from the login flow for use in subsequent authenticated requests.
    /// </summary>
    private static async Task<Dictionary<string, string>> LoginAsync(HttpClient client, string username, string password)
    {
        // Step 1: GET login page to obtain anti-forgery token and cookies
        var loginPageRequest = new HttpRequestMessage(HttpMethod.Get, "/account/login");
        var loginPageResponse = await client.SendAsync(loginPageRequest);
        loginPageResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var cookies = ExtractCookies(loginPageResponse);

        // Parse the HTML to get the anti-forgery token
        var document = await loginPageResponse.ParseHtmlAsync();
        var tokenInput = document.QuerySelector("input[name='__RequestVerificationToken']");
        tokenInput.ShouldNotBeNull("Login page should contain an anti-forgery token");
        var antiForgeryToken = tokenInput.GetAttribute("value")!;

        // Step 2: POST login credentials
        var formContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Username"] = username,
            ["Input.Password"] = password,
            ["Input.Button"] = "login",
            ["__RequestVerificationToken"] = antiForgeryToken
        });

        var loginPostRequest = new HttpRequestMessage(HttpMethod.Post, "/account/login")
        {
            Content = formContent
        };
        loginPostRequest.Headers.Add("Cookie", string.Join("; ", cookies.Select(c => $"{c.Key}={c.Value}")));

        var loginPostResponse = await client.SendAsync(loginPostRequest);
        loginPostResponse.StatusCode.ShouldBe(HttpStatusCode.Redirect, "Login POST should redirect on success");

        // Merge cookies from both responses (the POST response sets auth cookies)
        return MergeCookies(cookies, ExtractCookies(loginPostResponse));
    }

    /// <summary>
    /// Executes the full IdP-initiated SSO flow (redirect chain) and returns the parsed SAML response XML.
    /// </summary>
    private static async Task<XDocument> ExecuteIdpInitiatedFlowAsync(
        HttpClient client,
        Dictionary<string, string> cookies,
        string spEntityId)
    {
        // Step 1: IdP-initiated endpoint
        var idpRequest = CreateRequest(
            HttpMethod.Get,
            $"/saml/idp-initiated?spEntityId={Uri.EscapeDataString(spEntityId)}",
            cookies);
        var idpResponse = await client.SendAsync(idpRequest);

        idpResponse.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        var callbackLocation = idpResponse.Headers.Location?.ToString();
        callbackLocation.ShouldNotBeNull();

        var allCookies = MergeCookies(cookies, ExtractCookies(idpResponse));

        // Step 2: Follow redirect to signin_callback
        var callbackRequest = CreateRequest(HttpMethod.Get, callbackLocation, allCookies);
        var callbackResponse = await client.SendAsync(callbackRequest);

        callbackResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        return await ExtractSamlResponseFromHtmlAsync(callbackResponse);
    }

    /// <summary>
    /// Parses the HTML response to extract the base64-encoded SAMLResponse and returns it as an <see cref="XDocument"/>.
    /// </summary>
    private static async Task<XDocument> ExtractSamlResponseFromHtmlAsync(HttpResponseMessage response)
    {
        var document = await response.ParseHtmlAsync();

        var samlInput = document.QuerySelector("input[name='SAMLResponse']");
        samlInput.ShouldNotBeNull("Response HTML should contain a SAMLResponse hidden input");

        var base64Value = samlInput.GetAttribute("value");
        base64Value.ShouldNotBeNullOrEmpty("SAMLResponse value should not be empty");

        var xmlBytes = Convert.FromBase64String(base64Value);
        var xmlString = Encoding.UTF8.GetString(xmlBytes);

        return XDocument.Parse(xmlString);
    }

    /// <summary>
    /// Extracts cookie name/value pairs from Set-Cookie response headers.
    /// </summary>
    private static Dictionary<string, string> ExtractCookies(HttpResponseMessage response)
    {
        var cookies = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (!response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
        {
            return cookies;
        }

        foreach (var header in setCookieHeaders)
        {
            // Set-Cookie header format: name=value; path=/; ...
            var nameValue = header.Split(';', 2)[0];
            var eqIndex = nameValue.IndexOf('=');
            if (eqIndex > 0)
            {
                var name = nameValue[..eqIndex].Trim();
                var value = nameValue[(eqIndex + 1)..].Trim();
                cookies[name] = value;
            }
        }

        return cookies;
    }

    /// <summary>
    /// Merges two cookie dictionaries, with values from <paramref name="newer"/> overriding <paramref name="older"/>.
    /// </summary>
    private static Dictionary<string, string> MergeCookies(
        Dictionary<string, string> older,
        Dictionary<string, string> newer)
    {
        var merged = new Dictionary<string, string>(older, StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in newer)
        {
            merged[key] = value;
        }

        return merged;
    }

    /// <summary>
    /// Finds a SAML attribute by name within an assertion and returns its first value.
    /// </summary>
    private static string? GetAttributeValue(XElement assertion, string attributeName)
    {
        var attributeStatement = assertion.Element(Saml + "AttributeStatement");
        if (attributeStatement is null)
        {
            return null;
        }

        var attribute = attributeStatement
            .Elements(Saml + "Attribute")
            .FirstOrDefault(a => a.Attribute("Name")?.Value == attributeName);

        return attribute?.Element(Saml + "AttributeValue")?.Value;
    }
}
