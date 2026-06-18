using Fubaza.Application.Core.Contracts;
using Fubaza.Application.Core.Entities;
using Fubaza.Application.DTO.DTO;


namespace Fubaza.Application.Core.Interfaces.Services
{
	public interface IRoleService
	{
		Task<IResult<List<RoleDto>>> GetRoleAsync();
        Task<IResult<List<RoleDto>>> GetAdminRoleAsync();
        Task<IResult<Role>> GetRoleByIdAsync(Guid id);
        Task<IResult<bool>> AddorUpdateRoleAsync(Role role);
    }
}
