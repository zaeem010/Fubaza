using Fubaza.Application.Core.Constants;
using Fubaza.Application.Core.Entities;
using Fubaza.Application.Core.Interfaces.Services;
using Fubaza.Application.Core.Settings;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace Fubaza.Application.Infrastructure.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly JwtSettings _settings;

        public JwtTokenService(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IOptions<JwtSettings> settings)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _settings = settings.Value;
        }
        public async Task<string> GenerateTokenAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            // Load User Roles
            var userRoles = await _userManager.GetRolesAsync(user);
            var roleName = userRoles.FirstOrDefault();

            var role = !string.IsNullOrWhiteSpace(roleName)
                ? await _roleManager.FindByNameAsync(roleName)
                : null;

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.UserName ?? "null"),
        new Claim(ClaimTypes.Email, user.Email ?? "null"),

        new Claim("FullName", user.FullName ?? "null"),
        new Claim("Email", user.Email ?? "null"),

        new Claim("UserId", user.Id.ToString()),
        new Claim("EmailConfirmed", user.EmailConfirmed.ToString().ToLowerInvariant()),
        new Claim("IsPasswordRequired", user.IsPasswordRequired.ToString().ToLowerInvariant()),
        new Claim("IsConnectedFacebook", user.IsConnectedFacebook.ToString().ToLowerInvariant()),
        new Claim("FacebookTokenExpiresAt", user.FacebookTokenExpiresAt?.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? "null"),
        new Claim("IsConnectedInstagram", user.IsConnectedInstagram.ToString().ToLowerInvariant()),
        new Claim("InstagramTokenExpiresAt", user.InstagramTokenExpiresAt?.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? "null"),
        new Claim("IsNotificationEnabled", user.IsNotificationEnabled.ToString().ToLowerInvariant()),

        new Claim("RoleName", role?.Name ?? "null"),
        new Claim("RoleId", role?.Id.ToString() ?? "null")
    };

            if (role != null)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(role);

                foreach (var rc in roleClaims)
                {
                    claims.Add(new Claim(ApplicationClaimTypes.Permission, rc.Value));
                }
            }

            if (role?.Name == "Player")
            {
                claims.Add(new Claim("SportId", user.Player?.SportId?.ToString() ?? "null"));
                claims.Add(new Claim("SportType", user.Player?.Sport?.NormalizedName ?? "null"));
                claims.Add(new Claim("PlayerCurrentClubId", user.Player?.CurrentClubId?.ToString() ?? "null"));
                claims.Add(new Claim("PlayingPositionId", user.Player?.PlayingPositionId?.ToString() ?? "null"));
                claims.Add(new Claim("HeightCm", user.Player?.HeightCm?.ToString() ?? "null"));
                claims.Add(new Claim("WeightKg", user.Player?.WeightKg?.ToString() ?? "null"));
                claims.Add(new Claim("Gender", user.Player?.Gender?.ToString() ?? "null"));
            }
            else if (role?.Name == "Club")
            {
                claims.Add(new Claim("SportId", user.Club?.SportId?.ToString() ?? "null"));
                claims.Add(new Claim("SportType", user.Club?.Sport?.NormalizedName ?? "null"));
                claims.Add(new Claim("ClubId", user.Club?.Id.ToString() ?? "null"));
                claims.Add(new Claim("ClubPhoneNumber", user.PhoneNumber ?? "null"));
                claims.Add(new Claim("ClubAddress", user.Club?.Address ?? "null"));
            }

            // ============================
            // Build JWT Token
            // ============================
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_settings.TokenExpirationInMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
