using Fubaza.Application.Core.Contracts;
using Fubaza.Application.DTO.DTO;

namespace Fubaza.Application.Core.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<IResult<PlayerDashboardSocialStatsDto>> GetSocialStatsAsync(Guid userId);
    }
}
