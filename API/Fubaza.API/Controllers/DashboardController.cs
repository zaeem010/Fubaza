using Fubaza.API.Resources;
using Fubaza.Application.Core.Contracts.Services.Identity;
using Fubaza.Application.Core.Interfaces.Services;
using Fubaza.Application.DTO.DTO;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Fubaza.API.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly IDashboardService _dashboardService;
        private readonly ICurrentUser _currentUser;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public DashboardController(
            ILogger<DashboardController> logger,
            IDashboardService dashboardService,
            ICurrentUser currentUser,
            IStringLocalizer<SharedResource> localizer)
        {
            _logger = logger;
            _dashboardService = dashboardService;
            _currentUser = currentUser;
            _localizer = localizer;
        }

        [HttpGet("social-stats")]
        public async Task<IActionResult> GetSocialStatsAsync()
        {
            var userId = _currentUser.GetUserId();

            if (!Guid.TryParse(userId.ToString(), out Guid uid))
            {
                return BadRequest(new
                {
                    success = false,
                    message = _localizer["Common.Message.InvalidOrMissingUserId"].Value,
                    Error = _localizer["Common.Error.InvalidToken"].Value
                });
            }

            var result = await _dashboardService.GetSocialStatsAsync(uid);

            if (!result.Succeeded)
            {
                _logger.LogWarning("GetSocialStatsAsync failed for user {UserId}: {Messages}",
                    uid, string.Join("; ", result.Messages));
                return Ok(new
                {
                    success = false,
                    message = _localizer["Controller.Dashboard.Message.Failed.GetSocialStats"].Value,
                    Data = new PlayerDashboardSocialStatsDto(),
                    Error = result.Messages
                });
            }

            return Ok(new
            {
                success = true,
                message = _localizer["Controller.Dashboard.Message.Succeeded.GetSocialStats"].Value,
                Data = result.Data,
                Error = result.Messages
            });
        }
    }
}
