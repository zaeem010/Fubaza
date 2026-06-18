using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using Fubaza.Application.Core.Contracts.Services;
using Fubaza.Application.Core.Helpers;
using Fubaza.Application.Core.Constants;
using Fubaza.Application.Core.Entities;

namespace Fubaza.Application.Infrastructure.Persistence
{
    internal class IdentityDbSeeder : IDatabaseSeeder
    {
        private readonly ILogger<IdentityDbSeeder> _logger;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;

        public IdentityDbSeeder(
            ILogger<IdentityDbSeeder> logger,
            ApplicationDbContext db,
            RoleManager<Role> roleManager,
            UserManager<User> userManager)
        {
            _logger = logger;
            _db = db;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public void Initialize()
        {
            AddDefaultRoles();
            AddSuperAdmin();
            _db.SaveChanges();
        }

        private void AddDefaultRoles()
        {
            Task.Run(async () =>
            {
                var roleList = new List<string> { RoleConstants.SuperAdmin, RoleConstants.Player, RoleConstants.Club , RoleConstants.GraphicDesigner };
                foreach (var roleName in roleList)
                {
                    var role = new Role(roleName);

                    var roleInDb = await _roleManager.FindByNameAsync(roleName);
                    if (roleInDb == null)
                    {
                        await _roleManager.CreateAsync(role);
                        _logger.LogInformation($"Added '{roleName}' to Roles");
                    }
                }
            }).GetAwaiter().GetResult();
        }

        private void AddSuperAdmin()
        {
            Task.Run(async () =>
            {
                //Check if Role Exists
                var superAdminRole = new Role(RoleConstants.SuperAdmin);
                var superAdminRoleInDb = await _roleManager.FindByNameAsync(RoleConstants.SuperAdmin);
                if (superAdminRoleInDb == null)
                {
                    await _roleManager.CreateAsync(superAdminRole);
                    superAdminRoleInDb = await _roleManager.FindByNameAsync(RoleConstants.SuperAdmin);
                }

                if (superAdminRoleInDb == null)
                {
                    _logger.LogError("SuperAdmin role creation failed.");
                    return;
                }
                //Check if User Exists
                var superUser = new User()
                {
                    FullName = "Mustafa Cayli",
                    Email = "Mustafa.cayli@fubaza.de",
                    UserName = "Mustafa.cayli@fubaza.de",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    IsActive = true,
                    Created = DateTime.Now,
                };
                var superUserInDb = await _userManager.FindByEmailAsync(superUser.Email);
                if (superUserInDb == null)
                {
                    await _userManager.CreateAsync(superUser, UserConstants.DefaultPassword);
                    var result = await _userManager.AddToRoleAsync(superUser, RoleConstants.SuperAdmin);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Seeded Default SuperAdmin User.");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            _logger.LogError(error.Description);
                        }
                    }
                }

                var permissions = typeof(Core.Constants.Permissions).GetPermissionsWithGroup();

                foreach (var (value, group, description) in permissions)
                {
                    // 1. Add identity claim
                    await _roleManager.AddPermissionClaim(superAdminRoleInDb, value);

                    // 2. Add group + description in RoleClaims table
                    var rc = _db.RoleClaims
                        .FirstOrDefault(x => x.RoleId == superAdminRoleInDb.Id && x.ClaimValue == value);

                    if (rc != null)
                    {
                        rc.Group = group;
                        rc.Description = description;
                    }
                }

                await _db.SaveChangesAsync();

            }).GetAwaiter().GetResult();
        }


    }
}
