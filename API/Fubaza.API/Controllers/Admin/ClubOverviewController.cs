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
    public class ClubOverviewController : ControllerBase
    {
        private readonly IClubOverviewService _clubOverviewService;
        public ClubOverviewController(
             IClubOverviewService clubOverviewService

            )
        {
            _clubOverviewService = clubOverviewService;
        }

        [HttpGet("ClubCountBySport")]
        public async Task<IActionResult> GetClubCountBySportAsync()
        {
            var result = await _clubOverviewService.GetClubCountBySportAsync();

            var sports = new List<ClubCountBySportDto>();

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


        [HttpPost("Clubs")]
        public async Task<IActionResult> GetPaginatedClubsAsync([FromBody] PaginationRequest request)
        {
            var result = await _clubOverviewService.GetPaginatedClubsAsync(request);

            if (result.Succeeded)
            {
                return Ok(new
                {
                    success = true,
                    message = "Clubs fetched successfully",
                    Data = result.Data,
                });
            }

            return Ok(new
            {
                success = false,
                message = "Failed to fetch Clubs",
                Error = result.Messages
            });
        }


        [HttpGet("ClubInfo/{clubId}")]
        public async Task<IActionResult> GetPlayerInfoAsync(Guid clubId)
        {
            var result = await _clubOverviewService.GetClubInfoAsync(clubId);

            var sports = new ClubInfoDto();

            if (result.Succeeded)
            {
                sports = result.Data;
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = "Club Info  Not fetched successfully", Data = sports, Error = result.Messages });
            }

            return Ok(new { success = true, message = "Club Info fetched successfully", Data = sports, Error = result.Messages });
        }
    }
}
