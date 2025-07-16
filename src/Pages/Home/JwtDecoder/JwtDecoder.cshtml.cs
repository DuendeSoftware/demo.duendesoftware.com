using IdentityModel.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;

namespace IdentityServerHost.Pages.Home;

[AllowAnonymous]
public class JwtDecoder(IHttpClientFactory clientFactory, IMemoryCache cache) : PageModel
{
    private const string CacheKey = "jwt_decoder::access_token";
    
    public ViewModel View { get; set; } = default!;

    public async Task<IActionResult> OnGet()
    {
        var token = await GetOrCreateToken();
        View = new ViewModel { Token = token };
        
        return Page();
    }

    private async Task<string> GetOrCreateToken()
    {
        try
        {
            if (cache.TryGetValue(CacheKey, out string cachedToken))
            {
                return cachedToken;
            }
            
            var request = HttpContext.Request;
            var authority = request.Scheme + "://" + request.Host.ToUriComponent();
            
            var client = clientFactory.CreateClient();
            var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = $"{authority}/connect/token",

                ClientId = "m2m",
                ClientSecret = "secret",

                Scope = "api"
            });

            if (!response.IsError)
            {
                var token = response.AccessToken;
                cache.Set(CacheKey, token, TimeSpan.FromMinutes(20));
                
                return token;
            }
        }
        catch
        {
            return null;
        }
        
        return null;
    }
}