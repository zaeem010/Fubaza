using Fubaza.Application.Core.Contracts.Services.Identity;
using Fubaza.Application.Core.Entities;
using Fubaza.Application.Core.Interfaces.Services;
using Fubaza.Application.Dto.Services;
using Fubaza.Application.DTO.Contracts;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Fubaza.API.Controllers.Admin
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/admin/[controller]")]
    [ApiController]
    public class UserManagementController : ControllerBase
    {
        private readonly ILogger<UserManagementController> _logger;
        private readonly IIdentityService _identityService;
        private readonly IRoleService _roleService;
        private readonly IRoleClaimService _roleClaimService;
        private readonly ICurrentUser _currentUser;
        
        public UserManagementController(
            ILogger<UserManagementController> logger,
            IIdentityService IdentityService,
            IRoleService roleService,
            IRoleClaimService roleClaimService,
            ICurrentUser currentUser
            
           )
        {
            _logger = logger;
            _identityService = IdentityService;
            _currentUser = currentUser;
            _roleService = roleService;
            _roleClaimService = roleClaimService;
        }

        [HttpPost("GetUsers")]
        public async Task<IActionResult> GetUsersAsync(PaginationRequest request)
        {
            var result = await _identityService.GetUsersAsync(request);

            var sports = new List<UserListItemDto>();

            if (result.Succeeded)
            {
                sports = result.Data.ToList();
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = "User List Not fetched successfully", Data = sports, Error = result.Messages });
            }

            return Ok(new { success = true, message = "User  List fetched successfully", Data = sports, Error = result.Messages });
        }

        [HttpGet("GetAdminRole")]
        public async Task<IActionResult> GetAdminRoleAsync()
        {
            var result = await _roleService.GetAdminRoleAsync();

            var roles = new List<RoleDto>();

            if (result.Succeeded)
            {
                roles = result.Data.ToList();
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = "Role List Not fetched successfully", Data = roles, Error = result.Messages });
            }

            return Ok(new { success = true, message = "Role  List fetched successfully", Data = roles, Error = result.Messages });
        }

        [HttpPost("AddOrUpdateUser")]
        public async Task<IActionResult> AddOrUpdateUserAsync(AddOrUpdateUserRequest request)
        {
            var isCreate =  request.UserId == Guid.Empty;
            request.IsAdminPanel = true;

            var result = await _identityService.AddOrUpdateUserAsync(request);

            if (!result.Succeeded)
            {
                return Ok(new
                {
                    success = false,
                    message = isCreate ? "Failed to create user." : "Failed to update user.",
                    error = result.Messages
                });
            }

            return Ok(new
            {
                success = true,
                message = isCreate ? "User created successfully." : "User updated successfully.",
                error = result.Messages
            });
        }

        [HttpGet("GetUserById/{userId}")]
        public async Task<IActionResult> GetUsersAsync(Guid userId)
        {
            var result = await _identityService.GetUserByIdAsync(userId);

            var sports = new UserListItemDto();

            if (result.Succeeded)
            {
                sports = result.Data;
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = "User  Not fetched successfully", Data = sports, Error = result.Messages });
            }

            return Ok(new { success = true, message = "User   fetched successfully", Data = sports, Error = result.Messages });
        }

        [HttpPost("ResetUserPassword")]
        public async Task<IActionResult> ResetUserPasswordAsync(ResetPasswordRequest request)
        {
            var result = await _identityService.ResetUserPasswordAsync(request);

            if (!result.Succeeded)
            {
                return Ok(new
                {
                    success = false,
                    message = "Failed to reset user password.",
                    errors = result.Messages
                });
            }

            return Ok(new
            {
                success = true,
                message = "Password has been reset successfully.",
                errors = result.Messages
            });
        }

        [HttpGet("GetGraphicDesignerUsers")]
        public async Task<IActionResult> GetGraphicDesignerUsersAsync()
        {
            var result = await _identityService.GetGraphicDesignerUsersAsync();

            var users = new List<UserListItemDto>();

            if (result.Succeeded)
            {
                users = result.Data.ToList();
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = "User List Not fetched successfully", Data = users, Error = result.Messages });
            }

            return Ok(new { success = true, message = "User  List fetched successfully", Data = users, Error = result.Messages });
        }

        [HttpGet("GetRoleById/{roleId}")]
        public async Task<IActionResult> GetRoleByIdAsync(Guid roleId)
        {
            var result = await _roleService.GetRoleByIdAsync(roleId);

            var role = new Role();

            if (result.Succeeded)
            {
                role = result.Data;
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = "Role  Not fetched successfully", Data = role, Error = result.Messages });
            }

            return Ok(new { success = true, message = "Role   fetched successfully", Data = role, Error = result.Messages });
        }

        [HttpPost("AddorUpdateRole")]
        public async Task<IActionResult> AddOrUpdateUserAsync(Role role)
        {

            var result = await _roleService.AddorUpdateRoleAsync(role);

            if (!result.Succeeded)
            {
                return Ok(new
                {
                    success = false,
                    message = "Failed to add or update role.",
                    error = result.Messages
                });
            }

            return Ok(new
            {
                success = true,
                message = "Role added or updated successfully.",
                error = result.Messages
            });
        }

        [HttpGet("GetPermissions/{roleId}")]
        public async Task<IActionResult> GetPermissionsAsync(Guid roleId)
        {
            var result = await _roleClaimService.GetPermissions(roleId);

            var role = new PermissionModel();

            if (result.Succeeded)
            {
                role = result.Data;
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = "Role Permission  Not fetched successfully", Data = role, Error = result.Messages });
            }

            return Ok(new { success = true, message = "Role Permission  fetched successfully", Data = role, Error = result.Messages });
        }

        [HttpPost("UpdatePermission")]
        public async Task<IActionResult> UpdatePermissionAsync(PermissionModel model)
        {

            var result = await _roleClaimService.UpdatePermission(model);

            if (!result.Succeeded)
            {
                return Ok(new
                {
                    success = false,
                    message = "Failed to add or update Role Permission.",
                    error = result.Messages
                });
            }

            return Ok(new
            {
                success = true,
                message = "Role Permission added or updated successfully.",
                error = result.Messages
            });
        }

    }
}
