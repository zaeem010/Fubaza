using AutoMapper;
using Fubaza.Application.Core.Constants;
using Fubaza.Application.Core.Contracts;
using Fubaza.Application.Core.Entities;
using Fubaza.Application.Core.Extensions;
using Fubaza.Application.Core.Interfaces.Services;
using Fubaza.Application.Core.Wrapper;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data.Entity.Core;

namespace Fubaza.Application.Infrastructure.Services
{
	public class RoleService : IRoleService
    {
		private readonly RoleManager<Role> _roleManager;
        private readonly IMapper _mapper;
        private readonly ILogger<RoleService> _logger;
        private readonly ApplicationDbContext _db;
        public RoleService(
			RoleManager<Role> roleManager,
            IMapper mapper,
            ApplicationDbContext db,
            ILogger<RoleService> logger
            )
		{
            _logger = logger;
            _roleManager = roleManager;
            _mapper = mapper;
            _db = db;
        }

        public async Task<IResult<List<RoleDto>>> GetRoleAsync()
        {
            try
            {


                var roles = await _db.Roles.Where(r => r.Name == "Club" || r.Name == "Player").ToListAsync();

                foreach (var role in roles)
                {
                    if (role.Name == "Club")
                        role.NameDe = "Verein";

                    if (role.Name == "Player")
                        role.NameDe = "Spieler";
                }

                await _db.SaveChangesAsync();

                var rolelist = await _db.Roles
                    .Where(r => r.Name == "Club" || r.Name == "Player")
                    .ToListAsync();

                var map = _mapper.Map<List<RoleDto>>(rolelist);

                return await Result<List<RoleDto>>.SuccessAsync(map);
            }
            catch (Exception ex)
            {
                return await Result<List<RoleDto>>.FailAsync($"An error occurred while fetching roles: {ex.Message}");
            }
        }

        public async Task<IResult<List<RoleDto>>> GetAdminRoleAsync()
        {
            try
            {
                var roles = await _roleManager.Roles
                    .Where(r => r.Name != "Club"
                             && r.Name != "Player")
                    .ToListAsync();

                var map = _mapper.Map<List<RoleDto>>(roles);

                return await Result<List<RoleDto>>.SuccessAsync(map);
            }
            catch (Exception ex)
            {
                return await Result<List<RoleDto>>.FailAsync($"An error occurred while fetching roles: {ex.Message}");
            }
        }

        public async Task<IResult<Role>> GetRoleByIdAsync(Guid id)
        {
            try
            {
                var roles = await _roleManager.Roles.SingleOrDefaultAsync(x => x.Id == id);

                if (roles == null)
                {
                    return await Result<Role>.FailAsync();
                }

                return await Result<Role>.SuccessAsync(roles);

            }
            catch (Exception e)
            {
                _logger.LogError(e, e.GetMessage());
                return await Result<Role>.FailAsync(e.Message);
            }
        }

        public async Task<IResult<bool>> AddorUpdateRoleAsync(Role role)
        {

            if (role.Id == Guid.Empty)
            {
                var existingRole = await _roleManager.FindByNameAsync(role.Name);

                if (existingRole != null)
                {
                    return await Result<bool>.FailAsync("unable to fetch role");
                }

                var response = await _roleManager.CreateAsync(new Role(role.Name, role.Description));
                if (response.Succeeded)
                {
                    return await Result<bool>.SuccessAsync($"Role {role.Name} Created.");
                }
                else
                {
                    return await Result<bool>.FailAsync(response.Errors.Select(e => e.Description.ToString()).ToList());
                }
            }
            else
            {
                var existingRole = await _roleManager.FindByIdAsync(role.Id.ToString());
                if (existingRole == null)
                {
                    return await Result<bool>.FailAsync("Role does not exist.");
                }

                if (DefaultRoles().Contains(existingRole.Name))
                {
                    return await Result<bool>.SuccessAsync($"Not allowed to modify {existingRole.Name} Role.");
                }

                existingRole.Name = role.Name;
                existingRole.NormalizedName = role.Name.ToUpper();
                existingRole.Description = role.Description;

                await _roleManager.UpdateAsync(existingRole);

                return await Result<bool>.SuccessAsync($"Role {existingRole.Name} Updated.");
            }
        }

        private static List<string> DefaultRoles()
        {
            return typeof(RoleConstants).GetAllPublicConstantValues<string>();
        }
    }
}
