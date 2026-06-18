using Microsoft.AspNetCore.Http;

using System.Security.Claims;

namespace Fubaza.Application.Core.Contracts.Services.Identity
{
    public interface ICurrentUser
    {
        string? Username { get; }

        Guid? GetUserId();

        Guid? GetClubId();

        Guid? GetSportId();

        string? GetUserEmail();

        string? GetPhoneNumber();

        string? GetFullName();

        string? GetRoleName();

        bool IsAuthenticated();

        bool IsInRole(string role);

        IEnumerable<Claim>? GetUserClaims();

        HttpContext? GetHttpContext();

        IEnumerable<string> GetPermissions();

    }
}
