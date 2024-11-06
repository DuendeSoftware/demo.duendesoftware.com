using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IdentityModel;

namespace Duende.IdentityServer.Demo
{
    public class TestController : ControllerBase
    {
        [Route("/api/test")]
        [Authorize(AuthenticationSchemes = IdentityServerConstants.LocalApi.AuthenticationScheme)]
        public IActionResult Get()
        {
            var scheme = GetAuthorizationScheme(Request);
            var proofToken = GetDPoPProofToken(Request);

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
            var scheme = GetAuthorizationScheme(Request);
            var proofToken = GetDPoPProofToken(Request);

            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            claims.Add(new { Type = "authorization_scheme", Value = scheme });
            claims.Add(new { Type = "proof_token", Value = proofToken });

            return new JsonResult(claims);
        }

        private static string GetAuthorizationScheme(HttpRequest request)
        {
            return request.Headers.Authorization.FirstOrDefault()?.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
        }

        private static string GetDPoPProofToken(HttpRequest request)
        {
            return request.Headers[OidcConstants.HttpHeaders.DPoP].FirstOrDefault();
        }
    }
}