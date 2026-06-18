using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using System.Security.Claims;

using Fubaza.Application.Core.Entities;


namespace Fubaza.Application.Infrastructure.Factories
{
    internal class AdditionalUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<User, Role>
    {
        public AdditionalUserClaimsPrincipalFactory(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        { }

        public override async Task<ClaimsPrincipal> CreateAsync(User user)
        {
            var principal = await base.CreateAsync(user);
            var identity = principal.Identity as ClaimsIdentity
                           ?? throw new InvalidOperationException("Identity not found.");

            //AddClaimIfMissing(identity, "FullName", user.FullName ?? string.Empty);
            AddClaimIfMissing(identity, ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty);

            return principal;
        }

        private static void AddClaimIfMissing(ClaimsIdentity identity, string claimType, string value)
        {
            if (!identity.HasClaim(claim => claim.Type == claimType))
            {
                identity.AddClaim(new Claim(claimType, value));
            }
        }
    }
}
