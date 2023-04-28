using DPoPApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Duende.IdentityServer.Demo
{
    public class TestController : ControllerBase
    {
        [Route("/api/test")]
        [Authorize(AuthenticationSchemes = IdentityServerConstants.LocalApi.AuthenticationScheme)]
        public IActionResult Get()
        {
            var scheme = Request.GetAuthorizationScheme();
            var proofToken = Request.GetDPoPProofToken();

            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            
            claims.Add(new { Type = "authorization_scheme", Value = scheme });
            if (proofToken != null)
            {
                claims.Add(new { Type = "proof_token", Value = proofToken });
            }

            return new JsonResult(claims);
        }

        [Route("/api/dpop/test")]
        [Authorize(AuthenticationSchemes = "dpop")]
        public IActionResult GetDPoP()
        {
            var scheme = Request.GetAuthorizationScheme();
            var proofToken = Request.GetDPoPProofToken();

            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            claims.Add(new { Type = "authorization_scheme", Value = scheme });
            claims.Add(new { Type = "proof_token", Value = proofToken });

            return new JsonResult(claims);
        }
    }
}