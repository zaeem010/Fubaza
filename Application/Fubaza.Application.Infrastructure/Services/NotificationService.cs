using Fubaza.Application.Core.Common;

using Fubaza.Application.Core.Contracts;

using Fubaza.Application.Core.Interfaces.Repositories;
using Fubaza.Application.Core.Interfaces.Services;

using Fubaza.Application.Core.Wrapper;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Services;

using Microsoft.Extensions.Logging;

namespace Fubaza.Application.Infrastructure.Services
{
    internal class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly INotificationRepository _repository;
        

        public NotificationService(ILogger<NotificationService> logger,
            INotificationRepository repository
            )
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task<IResult<PaginatedResponse<UserNotificationsDto>>> GetNotificationsAsync(PaginationRequest request)
        {
            try
            {
                var result = await _repository.GetNotificationsAsync(request);

                if (result != null)
                {
                    return await Result<PaginatedResponse<UserNotificationsDto>>.SuccessAsync(result);
                }

                const string message = "Unable to get the Notification";

                _logger.LogError(message);
                return await Result<PaginatedResponse<UserNotificationsDto>>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<PaginatedResponse<UserNotificationsDto>>.FailAsync(e.Message);
            }
        }
        public async Task<IResult<bool>> MarkAllReadAsync(Guid userId)
        {
            const string message = "Unable to Mark All Read the Notification";
            try
            {

                var response = await _repository.MarkAllReadAsync(userId);

                if (response)
                {
                    return await Result<bool>.SuccessAsync(response);
                }

                _logger.LogError(message);
                return await Result<bool>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<bool>.FailAsync(e.Message);
            }
        }
        public async Task<IResult<bool>> UpdateNotificationStatus(NotificationPreferenceRequest request)
        {
            const string message = "Unable to update Notification status";
            try
            {
                var response = await _repository.UpdateNotificationStatus(request);

                if (response)
                {
                    return await Result<bool>.SuccessAsync(response);
                }

                _logger.LogError(message);
                return await Result<bool>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<bool>.FailAsync(e.Message);
            }
        }
    }
}
