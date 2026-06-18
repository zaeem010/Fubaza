using Fubaza.Application.Core.Common;
using Fubaza.Application.Core.Entities;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Services;


namespace Fubaza.Application.Core.Interfaces.Repositories
{
    public interface IClubRepository
    {
        Task<PaginatedResponse<ClubDTO>> GetClubAsync(PaginationRequest request);
        Task<ClubProfileDTO?> GetClubProfileAsync(Guid userId);
        Task<ClubProfileDTO> EditClubProfileAsync(Club updatedClub);
        Task<bool> AddClubAsync(Club club);
        Task<ClubStatDto> GetClubStatsAsync(Guid clubId);
        Task<User> UpdateClubProfileAsync(Club club);
        Task<bool> AddOrUpdateClubOfficialAsync(ClubOfficial clubOfficial , Notification notification);
        Task<Dictionary<string, List<DepartmentOfficialDto>>> GetClubOfficialsByDepartmentAsync(Guid userId);
        Task<bool> RemoveClubOfficialAsync(Guid clubOfficialId , Notification notification);
        Task<List<LineupPlayerInfoDto>> GetMatchLinupbyCludAsync(Guid clubId, Guid organizerClubId);
    }
}
