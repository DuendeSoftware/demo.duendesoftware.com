using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServerHost.Pages.Home;

[AllowAnonymous]
public class JwtDecoder : PageModel
{
}