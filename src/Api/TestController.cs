using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Duende.IdentityServer.Demo
{
    [Route("/api/test")]
    [Authorize(AuthenticationSchemes = IdentityServerConstants.LocalApi.AuthenticationScheme)]
    public class TestController : ControllerBase
    {
        public IActionResult Get()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value });
            return new JsonResult(claims);
        }
    }
}