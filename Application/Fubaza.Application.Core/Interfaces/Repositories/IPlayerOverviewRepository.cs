using Fubaza.Application.Core.Common;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Services;


namespace Fubaza.Application.Core.Interfaces.Repositories
{
    public interface IPlayerOverviewRepository
    {
        Task<List<PlayerCountBySportDto>> GetPlayerCountBySportAsync();
        Task<PaginatedResponse<PaginatedPlayersDto>> GetPaginatedPlayersAsync(PaginationRequest request);
        Task<PlayerInfoDto> GetPlayerInfoAsync(Guid playerId);
    }
}
