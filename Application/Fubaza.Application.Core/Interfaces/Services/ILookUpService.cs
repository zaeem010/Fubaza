using Fubaza.Application.Core.Contracts;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Services;



namespace Fubaza.Application.Core.Interfaces.Services
{
    
    public interface ILookUpService 
    {
        Task<IResult<List<SportDto>>> GetSportsAsync();
        Task<IResult<List<PlayingPositionDto>>> GetPlayingPositionsAsync(Guid sportId);
        Task<IResult<List<DesignationDto>>> GetDesignationsAsync();
        Task<IResult<List<TempleteTypeDTO>>> GetTempleteTypeAsync();
        Task<IResult<List<StrongFootDTO>>> GetStrongFootOptionsAsync();
        Task<IResult<List<ThrowingHandDTO>>> GetThrowingHandOptionsAsync();
        Task<IResult<List<EventTypeDTO>>> GetEventTypeAsync(Guid sportId);
        Task<IResult<WritePostCaptionDTO>> WritePostCaptionAsync(WritePostCaptionRequest request);
        Task<IResult<List<TempleteDto>>> GetTempletesAsync(TempleteRequest request);
        Task<IResult<AIImangeEnhancementDTO>> AIImangeEnhancementAsync(TempleteGenerationRequest request);
        Task<IResult<List<CompetitionTypeDto>>> GetCompetitionTypesAsync();
    }
}
