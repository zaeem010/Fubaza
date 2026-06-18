using Fubaza.Application.Core.Entities;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Services;

namespace Fubaza.Application.Core.Interfaces.Repositories
{
    public interface ILookUpRepository
    {
        Task<List<SportDto>> GetSportsAsync();
        Task<List<PlayingPositionDto>> GetPlayingPositionsAsync(Guid sportId);
        Task<List<EventTypeDTO>> GetEventTypeAsync(Guid sportId);
        Task<List<DesignationDto>> GetDesignationsAsync();
        Task<Designation> GetDesignationAsync(Guid designationId);
        Task<ClubOfficial> GetClubOfficialAsync(Guid clubOfficialId);
        Task<List<TempleteDto>> GetTempletesAsync(TempleteRequest request);
    }
}
