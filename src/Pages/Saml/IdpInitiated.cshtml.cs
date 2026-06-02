using Duende.IdentityServer.Saml;
using Duende.IdentityServer.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Duende.IdentityServer.Demo.Pages.Saml;

[AllowAnonymous]
public class IdpInitiatedModel : PageModel
{
    private readonly IIdpInitiatedSsoService _idpInitiatedSsoService;
    private readonly ISamlServiceProviderStore _spStore;

    public IdpInitiatedModel(
        IIdpInitiatedSsoService idpInitiatedSsoService,
        ISamlServiceProviderStore spStore)
    {
        _idpInitiatedSsoService = idpInitiatedSsoService;
        _spStore = spStore;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var spEntityId = HttpContext.Request.Query["spEntityId"].ToString();
        if (string.IsNullOrEmpty(spEntityId))
        {
            return BadRequest("spEntityId is required");
        }

        // Validate SP exists and allows IdP-initiated before checking auth
        var sp = await _spStore.FindByEntityIdAsync(spEntityId, cancellationToken);
        if (sp == null)
        {
            return BadRequest("Unknown service provider");
        }
        if (!sp.AllowIdpInitiated)
        {
            return BadRequest("Service provider does not allow IdP-initiated SSO");
        }

        // If user is not authenticated, redirect to login
        if (User.Identity?.IsAuthenticated != true)
        {
            return Challenge();
        }

        var result = await _idpInitiatedSsoService.CreateResponseAsync(HttpContext, spEntityId, cancellationToken);
        if (result.IsError)
        {
            return BadRequest(result.Error);
        }
        await result.Response.ExecuteAsync(HttpContext);
        return new EmptyResult();
    }
}
