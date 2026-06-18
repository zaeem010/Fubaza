using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Fubaza.Application.Core.Constants
{
    public static class ValidatePermission
    {
        public static bool IsPermissionValidated(ClaimsPrincipal user, IAuthorizationService _authorizationService, string module)
        {
            if ((_authorizationService.AuthorizeAsync(user, "Permissions." + module + ".Create")).Result.Succeeded
            || (_authorizationService.AuthorizeAsync(user, "Permissions." + module + ".Edit")).Result.Succeeded
            || (_authorizationService.AuthorizeAsync(user, "Permissions." + module + ".Delete")).Result.Succeeded
            || (_authorizationService.AuthorizeAsync(user, "Permissions." + module + ".View")).Result.Succeeded
            )
                return true;
            return false;
        }
    }
}
