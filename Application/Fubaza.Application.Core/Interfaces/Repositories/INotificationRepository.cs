using Fubaza.Application.Core.Common;
using Fubaza.Application.Core.Entities;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Services;


namespace Fubaza.Application.Core.Interfaces.Repositories
{
    public  interface INotificationRepository
    {
        Task<PaginatedResponse<UserNotificationsDto>> GetNotificationsAsync(PaginationRequest request);
        Task<bool> MarkAllReadAsync(Guid userId);
        Task AddNotificationAsync(Notification notification, CancellationToken cancellationToken);

        Task<bool> UpdateNotificationStatus(NotificationPreferenceRequest request);
    }
}
