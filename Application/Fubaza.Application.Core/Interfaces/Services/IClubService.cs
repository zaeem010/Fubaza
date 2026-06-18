using Fubaza.Application.Core.Common;
using Fubaza.Application.Core.Contracts;
using Fubaza.Application.Core.Entities;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Services;


namespace Fubaza.Application.Core.Interfaces.Services
{
    public interface IClubService
    {
        Task<IResult<PaginatedResponse<ClubDTO>>> GetClubAsync(PaginationRequest request);
        Task<IResult<bool>> AddClubAsync(Club club);
        Task<IResult<ClubStatDto>> GetClubStatsAsync(Guid clubId);
        Task<IResult<ClubProfileDTO>> GetClubProfileAsync(Guid userId);
        Task<IResult<ClubProfileDTO>> EditClubProfileAsync(Club updatedClub);
        Task<IResult<UserDto>> UpdateClubProfileAsync(Club club);
        Task<IResult<bool>> AddOrUpdateClubOfficialAsync(ClubOfficial clubOfficial, Guid userId);
        Task<IResult<Dictionary<string, List<DepartmentOfficialDto>>>> GetClubOfficialsByDepartmentAsync(Guid userId);
        Task<IResult<bool>> RemoveClubOfficialAsync(Guid clubOfficialId, Guid userId);
        Task<IResult<List<LineupPlayerInfoDto>>> GetMatchLinupbyCludAsync(Guid matchdayId, Guid organizerClubId);
    }
}
