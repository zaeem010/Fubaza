using Fubaza.Application.Core.Common;
using Fubaza.Application.Core.Contracts;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Services;

namespace Fubaza.Application.Core.Interfaces.Services
{
    public interface INotificationService
    {
        Task<IResult<PaginatedResponse<UserNotificationsDto>>> GetNotificationsAsync(PaginationRequest request);
        Task<IResult<bool>> MarkAllReadAsync(Guid userId);
        Task<IResult<bool>> UpdateNotificationStatus(NotificationPreferenceRequest request);
    }
}
