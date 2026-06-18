using Fubaza.Application.Core.Contracts;

using Fubaza.Application.DTO.DTO;

namespace Fubaza.Application.Core.Interfaces.Services
{
    public interface IRoleClaimService
    {
        Task<IResult<PermissionModel>> GetPermissions(Guid roleId);
        Task<IResult<bool>> UpdatePermission(PermissionModel model);
    }
}
