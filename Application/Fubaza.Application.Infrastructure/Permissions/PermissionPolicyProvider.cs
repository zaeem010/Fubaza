using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

using Fubaza.Application.Core.Constants;

namespace Fubaza.Application.Infrastructure.Permissions
{
    internal class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            FallbackPolicyProvider = options is null
                ? throw new ArgumentNullException(nameof(options))
                : new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
            => FallbackPolicyProvider.GetDefaultPolicyAsync();

        public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(ApplicationClaimTypes.Permission, StringComparison.OrdinalIgnoreCase))
            {
                var policy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new PermissionRequirement(policyName))
                    .Build();

                return await Task.FromResult(policy);
            }
            return await FallbackPolicyProvider.GetPolicyAsync(policyName);
        }

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
            => Task.FromResult<AuthorizationPolicy?>(null);
    }
}
