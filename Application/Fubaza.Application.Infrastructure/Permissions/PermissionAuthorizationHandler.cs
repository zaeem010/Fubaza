using Fubaza.Application.Core.Constants;

using Microsoft.AspNetCore.Authorization;


namespace Fubaza.Application.Infrastructure.Permissions
{
    internal class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        public PermissionAuthorizationHandler() { }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User?.Claims == null)
                return Task.CompletedTask;

            var hasPermission = context.User.Claims.Any(x =>
                x.Type == ApplicationClaimTypes.Permission &&
                x.Value == requirement.Permission &&
                x.Issuer == "LOCAL AUTHORITY");

            if (hasPermission)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
