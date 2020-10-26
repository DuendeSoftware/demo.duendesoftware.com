using System.Threading.Tasks;
using Duende.IdentityServer.Services;

namespace Duende.IdentityServer.Demo
{
    // allows arbitrary CORS origins - only for demo purposes. NEVER USE IN PRODUCTION
    public class DemoCorsPolicy : ICorsPolicyService
    {
        public Task<bool> IsOriginAllowedAsync(string origin)
        {
            return Task.FromResult(true);
        }
    }
}
