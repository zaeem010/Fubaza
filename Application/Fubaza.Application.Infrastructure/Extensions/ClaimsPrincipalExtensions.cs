using System.Security.Claims;

namespace Fubaza.Application.Infrastructure.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal principal)
        {
            ArgumentNullException.ThrowIfNull(principal, nameof(principal));

            var claim = principal.FindFirst(ClaimTypes.NameIdentifier);
            return Convert.ToInt32(claim?.Value);
        }

        public static string? GetUserEmail(this ClaimsPrincipal principal)
        {
            ArgumentNullException.ThrowIfNull(principal, nameof(principal));

            return principal.FindFirst(ClaimTypes.Email)?.Value;
        }

        public static string? GetFirstName(this ClaimsPrincipal principal)
        {
            ArgumentNullException.ThrowIfNull(principal, nameof(principal));

            return principal.FindFirst("FirstName")?.Value;
        }

        public static string? GetLastName(this ClaimsPrincipal principal)
        {
            ArgumentNullException.ThrowIfNull(principal, nameof(principal));

            return principal.FindFirst("LastName")?.Value;
        }

        public static string? GetFullName(this ClaimsPrincipal principal)
        {
            ArgumentNullException.ThrowIfNull(principal, nameof(principal));

            return principal.FindFirst("FullName")?.Value;
        }

        public static string? GetPhoneNumber(this ClaimsPrincipal principal)
        {
            ArgumentNullException.ThrowIfNull(principal, nameof(principal));

            return principal.FindFirst("PhoneNumber")?.Value;
        }
    }
}
