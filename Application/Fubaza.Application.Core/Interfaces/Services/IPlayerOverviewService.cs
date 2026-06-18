using Fubaza.Application.Core.Common;
using Fubaza.Application.Core.Contracts;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Services;


namespace Fubaza.Application.Core.Interfaces.Services
{
    public interface IPlayerOverviewService
    {
        
        Task<IResult<List<PlayerCountBySportDto>>> GetPlayerCountBySportAsync();
        Task<IResult<PaginatedResponse<PaginatedPlayersDto>>> GetPaginatedPlayersAsync(PaginationRequest request);
        Task<IResult<PlayerInfoDto>> GetPlayerInfoAsync(Guid playerId);
    }
}
