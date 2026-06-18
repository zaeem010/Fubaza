using Fubaza.Application.Core.Contracts;
using Fubaza.Application.Core.Entities;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Enums;


namespace Fubaza.Application.Core.Interfaces.Services
{
    public interface IPlayerService
    {
        Task<IResult<bool>> AddOrUpdatePlayerAsync(Player player , Guid userid);
        Task<IResult<UserDto>> UpdatePlayerProfileAsync(Player player);
        Task<IResult<PlayerProfileDTO>> EditPlayerProfileAsync(Player updatedPlayer);
        Task<IResult<PlayerProfileDTO>> GetPlayerProfileAsync(Guid userId);
        Task<IResult<Dictionary<string, CategoryPlayerDto>>> GetPlayersByCategoryAsync(Guid clubId);
        Task<IResult<bool>> RemovePlayerAsync(Guid playerId, Guid userid);
        Task<IResult<Dictionary<object, DocumentTypeGroupDto>>> GetMediaGalleryAsync(Guid clubId);
    }
}
