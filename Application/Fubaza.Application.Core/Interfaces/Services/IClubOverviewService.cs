using Fubaza.Application.Core.Common;
using Fubaza.Application.Core.Contracts;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Services;


namespace Fubaza.Application.Core.Interfaces.Services
{
    public interface IClubOverviewService
    {
        Task<IResult<List<ClubCountBySportDto>>> GetClubCountBySportAsync();
        Task<IResult<PaginatedResponse<PaginatedClubsDto>>> GetPaginatedClubsAsync(PaginationRequest request);

        Task<IResult<ClubInfoDto>> GetClubInfoAsync(Guid clubId);
    }
}
