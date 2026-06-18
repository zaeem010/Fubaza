using Fubaza.API.Resources;
using Fubaza.Application.Core.Contracts.Services.Identity;
using Fubaza.Application.Core.Interfaces.Services;
using Fubaza.Application.DTO.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Fubaza.API.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly ILogger<NotificationController> _logger;
        private readonly INotificationService _notificationService;
        private readonly ICurrentUser _currentUser;
        private readonly IStringLocalizer<SharedResource> _localizer;
        public NotificationController(
             ILogger<NotificationController> logger,
             INotificationService notificationService,
             ICurrentUser currentUser,
             IStringLocalizer<SharedResource> localizer
            )
        {
            _logger = logger;
            _notificationService = notificationService;
            _currentUser = currentUser;
            _localizer = localizer;
        }

        [HttpPost("GetNotifications")]
        public async Task<IActionResult> GetNotificationsAsync([FromBody] PaginationRequest request)
        {

            var userId = _currentUser.GetUserId();

            if (!Guid.TryParse(userId.ToString(), out Guid userid))
            {
                return BadRequest(
                        new
                        {
                            success = false,
                            message = _localizer["Common.Message.InvalidOrMissingUserId"].Value,
                            Error = _localizer["Common.Error.InvalidToken"].Value
                        });
            }

            request.UserId = userid;

            var result = await _notificationService.GetNotificationsAsync(request);

            if (result.Succeeded)
            {
                return Ok(new
                {
                    success = true,
                    message = _localizer["Controller.Notification.Message.Succeeded.GetNotificationsAsync"].Value,
                    Data = result.Data,
                });
            }

            return Ok(new
            {
                success = false,
                message = _localizer["Controller.Notification.Message.Failed.GetNotificationsAsync"].Value,
                Error = result.Messages
            });
        }

        [HttpPost("markallread")]
        public async Task<IActionResult> MarkAllReadAsync()
        {
            try
            {
                var userId = _currentUser.GetUserId();

                if (!Guid.TryParse(userId.ToString(), out Guid userid))
                {
                    return BadRequest(
                        new
                        {
                            success = false,
                            message = _localizer["Common.Message.InvalidOrMissingUserId"].Value,
                            Error = _localizer["Common.Error.InvalidToken"].Value
                        });
                }


                var updateunread = await _notificationService.MarkAllReadAsync(userid);

                if (!updateunread.Succeeded)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = _localizer["Controller.Notification.Message.Failed.MarkAllReadAsync"].Value,
                        errors = updateunread.Messages
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = _localizer["Controller.Notification.Message.Succeeded.MarkAllReadAsync"].Value,
                    errors = (string[]?)null
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while soft deleting a Player.");

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = _localizer["Common.Message.ExceptionOccurred"].Value,
                    error = e.Message
                });
            }
        }

        [HttpPost("UpdateNotificationStatus")]
        public async Task<IActionResult> UpdateNotificationStatus(NotificationPreferenceRequest request)
        {
            try
            {
                var userId = _currentUser.GetUserId();

                if (!Guid.TryParse(userId.ToString(), out Guid userid))
                {
                    return BadRequest(
                        new
                        {
                            success = false,
                            message = _localizer["Common.Message.InvalidOrMissingUserId"].Value,
                            Error = _localizer["Common.Error.InvalidToken"].Value
                        });
                }

                request.UserId = userid; 

                var update = await _notificationService.UpdateNotificationStatus(request);

                if (!update.Succeeded)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = _localizer["Controller.Notification.Message.Failed.UpdateNotificationStatus"].Value,
                        errors = update.Messages
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = _localizer["Controller.Notification.Message.Succeeded.UpdateNotificationStatus"].Value,
                    errors = (string[]?)null
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while soft deleting a Player.");

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = _localizer["Common.Message.ExceptionOccurred"].Value,
                    error = e.Message
                });
            }
        }
    }
}
