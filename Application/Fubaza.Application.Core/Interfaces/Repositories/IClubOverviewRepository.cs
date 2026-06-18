using Fubaza.Application.Core.Common;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Services;


namespace Fubaza.Application.Core.Interfaces.Repositories
{
    public interface IClubOverviewRepository
    {
        Task<List<ClubCountBySportDto>> GetClubCountBySportAsync();
        Task<PaginatedResponse<PaginatedClubsDto>> GetPaginatedClubsAsync(PaginationRequest request);
        Task<ClubInfoDto> GetClubInfoAsync(Guid clubId);
    }
}
