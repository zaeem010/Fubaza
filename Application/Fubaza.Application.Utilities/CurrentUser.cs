using Fubaza.Application.Core.Constants;
using Fubaza.Application.Core.Contracts.Services.Identity;
using Fubaza.Application.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;


namespace Fubaza.Application.Utilities
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _accessor;

        public CurrentUser(IHttpContextAccessor accessor)
        {
            _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
        }

        public string? Username => _accessor.HttpContext?.User?.Identity?.Name;

        public Guid? GetUserId()
        {
            if (!IsAuthenticated()) return null;

            // Safely retrieve the claim value as a string
            string? userIdString = _accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Try to parse it as a Guid
            return Guid.TryParse(userIdString, out var userId) ? userId : null;
        }

        public Guid? GetClubId()
        {
            if (!IsAuthenticated()) return null;

            string? clubIdString = _accessor.HttpContext?.User.FindFirst("ClubId")?.Value;

            return Guid.TryParse(clubIdString, out var clubId) ? clubId : null;
        }

        public Guid? GetSportId()
        {
            if (!IsAuthenticated()) return null;

            string? sportIdString = _accessor.HttpContext?.User.FindFirst("SportId")?.Value;

            return Guid.TryParse(sportIdString, out var sportId) ? sportId : null;
        }

        public string? GetRoleName()
        {
            if (!IsAuthenticated()) return null;

            return _accessor.HttpContext?.User.FindFirst("RoleName")?.Value;
        }

        public string? GetUserEmail()
        {
            return IsAuthenticated() ? _accessor.HttpContext?.User.GetUserEmail() : null;
        }

        public string? GetFullName()
        {
            return IsAuthenticated() ? _accessor.HttpContext?.User.GetFullName() : null;
        }

        public bool IsAuthenticated()
        {
            return _accessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }

        public bool IsInRole(string role)
        {
            return _accessor.HttpContext?.User?.IsInRole(role) ?? false;
        }

        public IEnumerable<Claim>? GetUserClaims()
        {
            return _accessor.HttpContext?.User?.Claims;
        }

        public HttpContext? GetHttpContext()
        {
            return _accessor.HttpContext;
        }

        public string? GetPhoneNumber()
        {
            return IsAuthenticated() ? _accessor.HttpContext?.User.GetPhoneNumber() : null;
        }

        public IEnumerable<string> GetPermissions()
        {
            if (!IsAuthenticated())
                return Enumerable.Empty<string>();

            return _accessor.HttpContext?.User?
                .Claims
                .Where(c => c.Type == ApplicationClaimTypes.Permission)
                .Select(c => c.Value)
                ?? Enumerable.Empty<string>();
        }
    }
}
