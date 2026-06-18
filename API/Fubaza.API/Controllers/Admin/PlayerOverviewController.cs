using Fubaza.Application.Core.Interfaces.Services;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fubaza.API.Controllers.Admin
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/admin/[controller]")]
    [ApiController]
    public class PlayerOverviewController : ControllerBase
    {
        
        private readonly IPlayerOverviewService _playerOverviewService;
        public PlayerOverviewController(
             IPlayerOverviewService playerOverviewService
               
            )
        {
            _playerOverviewService = playerOverviewService;
        }

        [HttpGet("PlayerCountBySport")]
        public async Task<IActionResult> PlayerCountBySportAsync()
        {
            var result = await _playerOverviewService.GetPlayerCountBySportAsync();

            var sports = new List<PlayerCountBySportDto>();

            if (result.Succeeded)
            {
                sports = result.Data.ToList();
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = "Player Count By Sport List Not fetched successfully", Data = sports, Error = result.Messages });
            }

            return Ok(new { success = true, message = "Player Count By Sport List fetched successfully", Data = sports, Error = result.Messages });
        }


        [HttpPost("Players")]
        public async Task<IActionResult> GetPaginatedPlayersAsync([FromBody] PaginationRequest request)
        {
            var result = await _playerOverviewService.GetPaginatedPlayersAsync(request);

            if (result.Succeeded)
            {
                return Ok(new
                {
                    success = true,
                    message = "Players fetched successfully",
                    Data = result.Data,
                });
            }

            return Ok(new
            {
                success = false,
                message = "Failed to fetch players",
                Error = result.Messages
            });
        }

        [HttpGet("PlayerInfo/{playerId}")]
        public async Task<IActionResult> GetPlayerInfoAsync(Guid playerId)
        {
            var result = await _playerOverviewService.GetPlayerInfoAsync(playerId);

            var sports = new PlayerInfoDto();

            if (result.Succeeded)
            {
                sports = result.Data;
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = "Player Info  Not fetched successfully", Data = sports, Error = result.Messages });
            }

            return Ok(new { success = true, message = "Player Info fetched successfully", Data = sports, Error = result.Messages });
        }
    }
}

