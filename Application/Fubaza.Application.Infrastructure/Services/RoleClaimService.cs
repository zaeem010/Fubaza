using AutoMapper;
using Fubaza.Application.Core.Contracts;
using Fubaza.Application.Core.Entities;

using Fubaza.Application.Core.Interfaces.Services;
using Fubaza.Application.Core.Wrapper;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System.Security.Claims;

namespace Fubaza.Application.Infrastructure.Services
{
    public class RoleClaimService : IRoleClaimService
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly ILogger<RoleClaimService> _logger;
        private readonly ApplicationDbContext _db;

        public RoleClaimService(
            RoleManager<Role> roleManager,
            ILogger<RoleClaimService> logger,
            ApplicationDbContext db
            )
        {
            _roleManager = roleManager;
            _logger = logger;
            _db = db;
        }
        
        public async Task<IResult<PermissionModel>> GetPermissions(Guid roleId)
        {
            string message = "Unable to get Permissions";

            var result = new PermissionModel();

            try
            {
                // Load current role
                var currentRole = await _roleManager.FindByIdAsync(roleId.ToString());
                if (currentRole == null)
                    return await Result<PermissionModel>.FailAsync("Role not found");

                // Load SuperAdmin
                var superAdminRole = await _roleManager.FindByNameAsync("SuperAdmin");
                if (superAdminRole == null)
                    return await Result<PermissionModel>.FailAsync("SuperAdmin role not found");

                // Get current role permissions
                var currentRolePermissions = await _db.RoleClaims
                    .Where(x => x.RoleId == currentRole.Id)
                    .ToListAsync();

                // Get SuperAdmin permissions
                var superAdminClaims = await _db.RoleClaims
                    .Where(x => x.RoleId == superAdminRole.Id)
                    .ToListAsync();

                // Excluded permissions
                var excludedPermissions = new List<string>
                {
                   "Permissions.UserTemepletes.View"
                };

                // Filtered SuperAdmin permissions
                var filteredPermissions = superAdminClaims
                    .Where(c => !excludedPermissions.Contains(c.ClaimValue))
                    .ToList();

                result.RoleId = roleId;

                // Build final permission list
                var allPermissions = filteredPermissions.Select(c => new RoleClaimsModel
                {
                    RoleId = currentRole.Id,      // IMPORTANT FIX: Must be current role, not superadmin
                    Type = c.ClaimType,
                    Value = c.ClaimValue,
                    Group = c.Group,
                    Description = c.Description,
                    Selected = currentRolePermissions.Any(p => p.ClaimValue == c.ClaimValue)
                }).ToList();

                result.RoleClaims = allPermissions;

                return await Result<PermissionModel>.SuccessAsync(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return await Result<PermissionModel>.FailAsync(message);
            }
        }
        public async Task<IResult<bool>> UpdatePermission(PermissionModel model)
        {
            const string message = "Unable to update Permissions";

            try
            {
                var role = await _roleManager.FindByIdAsync(model.RoleId.ToString());
                if (role == null)
                    return await Result<bool>.FailAsync("Role not found");

                // Remove old claims
                var existingClaims = await _roleManager.GetClaimsAsync(role);
                foreach (var claim in existingClaims)
                {
                    await _roleManager.RemoveClaimAsync(role, claim);
                }

                // Selected claims from model
                var selectedClaims = model.RoleClaims.Where(x => x.Selected).ToList();

                foreach (var c in selectedClaims)
                {
                    // Add actual claim
                    var claim = new Claim(c.Type!, c.Value!);
                    await _roleManager.AddClaimAsync(role, claim);

                    // Optional: Save extended data in database manually
                    var roleClaim = new RoleClaim
                    {
                        RoleId = c.RoleId,
                        ClaimType = c.Type,
                        ClaimValue = c.Value,
                        Description = c.Description,
                        Group = c.Group
                    };

                    _db.RoleClaims.Add(roleClaim);
                }

                await _db.SaveChangesAsync();

                return await Result<bool>.SuccessAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return await Result<bool>.FailAsync(message);
            }
        }

    }
}
