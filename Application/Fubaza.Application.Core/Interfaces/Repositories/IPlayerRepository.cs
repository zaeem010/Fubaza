using Fubaza.Application.Core.Entities;
using Fubaza.Application.DTO.DTO;



namespace Fubaza.Application.Core.Interfaces.Repositories
{
    public interface IPlayerRepository
    {
        Task<bool> AddOrUpdatePlayerAsync(Player player , Notification notification);
        Task<bool> CheckPlayerJerseyAsync(Player player);
        Task<User?> UpdatePlayerProfileAsync(Player player);
        Task<PlayerProfileDTO?> EditPlayerProfileAsync(Player updatedPlayer);
        Task<PlayerProfileDTO?> GetPlayerProfileAsync(Guid userId);
        Task<Dictionary<string, CategoryPlayerDto>> GetPlayersByCategoryAsync(Guid clubId);
        Task<bool> RemovePlayerAsync(Guid playerId, Notification notification);
        Task<Player?> GetPlayerAsync(Guid playerId);
        Task<Dictionary<object, DocumentTypeGroupDto>> GetMediaGalleryAsync(Guid clubid);
    }
}
