using Fubaza.API.Resources;
using Fubaza.Application.Core.Contracts.Services.Identity;
using Fubaza.Application.Core.Entities;
using Fubaza.Application.Core.Interfaces.Services;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Enums;
using Fubaza.Application.DTO.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Fubaza.API.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/[controller]")]
    [ApiController]
    public class ClubController : ControllerBase
    {
        private readonly ILogger<ClubController> _logger;
        private readonly IClubService _clubService;
        private readonly ICurrentUser _currentUser;
        private readonly IStringLocalizer<SharedResource> _localizer;
        public ClubController(
           ILogger<ClubController> logger,
           IClubService clubService,
           ICurrentUser currentUser,
           IStringLocalizer<SharedResource> localizer
          )
        {
            _logger = logger;
            _clubService = clubService;
            _currentUser = currentUser;
            _localizer = localizer;
        }
       
       
        [HttpPost("Clubs")]
        public async Task<IActionResult> GetClub([FromBody] PaginationRequest request)
        {
            request.ClubId = _currentUser.GetClubId();
            request.UserId = _currentUser.GetUserId();
            var result = await _clubService.GetClubAsync(request);

            if (result.Succeeded)
            {
                return Ok(new
                {
                    success = true,
                    message = _localizer["Controller.Club.Message.Succeeded.GetClub"].Value,
                    Data = result.Data,
                });
            }

            return Ok(new
            {
                success = false,
                message = _localizer["Controller.Club.Message.Failed.GetClub"].Value,
                Error = result.Messages
            });
        }

        [HttpPost("AddClub")]
        public async Task<IActionResult> AddClubAsync([FromForm] AddClubRequest request)
        {
            try
            {
                var club = new Club
                {
                    FullName = request.FullName,
                    SportId = request.SportId,
                    CreatorClubId = _currentUser.GetClubId(),
                    CreatedByUserId = _currentUser.GetUserId(),
                    Document = new ClubDocument()
                };

                if (request.File != null && request.File.Length > 0)
                {
                    var filename = request.File.FileName;
                    var extension = Path.GetExtension(filename);

                    await using var ms = new MemoryStream();
                    await request.File.CopyToAsync(ms);
                    club.Document.FileContent = ms.ToArray();
                    club.Document.FileName = filename;
                    club.Document.Extension = extension;
                }

                var profileUpdateResult = await _clubService.AddClubAsync(club);

                if (!profileUpdateResult.Succeeded)
                {
                    return Ok(new { success = false, message = _localizer["Controller.Club.Message.Failed.AddClubAsync"].Value, Error = profileUpdateResult.Messages });
                }

                return Ok(new { success = true, message = _localizer["Controller.Club.Message.Succeeded.AddClubAsync"].Value, Error = profileUpdateResult.Messages });

            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                ModelState.AddModelError(string.Empty, e.Message);
            }

            return Ok(new { success = false, message = _localizer["Common.Message.ExceptionOccurred"].Value });
        }

        [HttpGet("GetClubProfile")]
        public async Task<IActionResult> GetClubProfileAsync()
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

            var result = await _clubService.GetClubProfileAsync(userid);

            var club = new ClubProfileDTO();

            if (result.Succeeded)
            {
                club = result.Data;
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = _localizer["Controller.Club.Message.Failed.GetClubProfileAsync"].Value, Data = club, Error = result.Messages });
            }

            return Ok(new { success = true, message = _localizer["Controller.Club.Message.Succeeded.GetClubProfileAsync"].Value, Data = club, Error = result.Messages });
        }

        [HttpPost("EditClubProfile")]
        public async Task<IActionResult> EditClubProfileAsync([FromForm] EditClubProfileRequest request)
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

                var club = new Club
                {
                    UserId = userid,
                    FullName = request.FullName,
                    Address = request.Address,
                    PhoneNumber = request.PhoneNumber,
                    Nationality = request.Nationality,
                    League = request.League,
                    Division = request.Division,
                    Description = request.Description,
                    Document = new ClubDocument(),
                };

                if (request.File != null && request.File.Length > 0)
                {
                    var filename = request.File.FileName;
                    var extension = Path.GetExtension(filename);

                    await using var ms = new MemoryStream();
                    await request.File.CopyToAsync(ms);
                    club.Document.FileContent = ms.ToArray();
                    club.Document.FileName = filename;
                    club.Document.Extension = extension;
                }

                var profileUpdateResult = await _clubService.EditClubProfileAsync(club);

                if (!profileUpdateResult.Succeeded)
                {
                    return Ok(new
                    {
                        success = false,
                        message = _localizer["Controller.Club.Message.Failed.EditClubProfileAsync"].Value,
                        Data = profileUpdateResult.Data,
                        Error = profileUpdateResult.Messages
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = _localizer["Controller.Club.Message.Succeeded.EditClubProfileAsync"].Value,
                    Data = profileUpdateResult.Data,
                    Error = profileUpdateResult.Messages
                });
                
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                ModelState.AddModelError(string.Empty, e.Message);
            }

            return Ok(new { success = false, message = _localizer["Common.Message.ExceptionOccurred"].Value });
        }


        [HttpPost("UpdateClubProfile")]
        public async Task<IActionResult> UpdateClubProfileAsync([FromForm] ClubProfileRequest request)
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

                var club = new Club
                {
                    UserId = userid,
                    FullName = request.FullName,
                    SportId = request.SportId,
                    RoleId = request.RoleId,
                    Address = request.Address,
                    PhoneNumber = request.PhoneNumber,
                    Nationality = request.Nationality,
                    League = request.League,
                    Division = request.Division,
                    Description = request.Description,
                    Document = new ClubDocument(),
                };

                if (request.File != null && request.File.Length > 0)
                {
                    var filename = request.File.FileName;
                    var extension = Path.GetExtension(filename);

                    await using var ms = new MemoryStream();
                    await request.File.CopyToAsync(ms);
                    club.Document.FileContent = ms.ToArray();
                    club.Document.FileName = filename;
                    club.Document.Extension = extension;
                }

                var profileUpdateResult = await _clubService.UpdateClubProfileAsync(club);

                if (!profileUpdateResult.Succeeded)
                {
                    return Ok(new { success = false, message = _localizer["Controller.Club.Message.Failed.UpdateClubProfileAsync"].Value, Data = profileUpdateResult.Data, Error = profileUpdateResult.Messages });
                }

                return Ok(new { success = true, message = _localizer["Controller.Club.Message.Succeeded.UpdateClubProfileAsync"].Value, Data = profileUpdateResult.Data, Error = profileUpdateResult.Messages });

            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                ModelState.AddModelError(string.Empty, e.Message);
            }

            return Ok(new { success = false, message = _localizer["Common.Message.ExceptionOccurred"].Value });
        }

        [HttpPost("AddOrUpdateClubOfficial")]
        public async Task<IActionResult> AddOrUpdateClubOfficialAsync([FromForm] AddOrUpdateOfficialRequest request)
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

                var clubOfficial = new ClubOfficial
                {
                    Id = request.OfficialId ?? Guid.Empty,
                    Name = request.Name,
                    ClubId = request.ClubId,
                    Gender = request.Gender.HasValue ? (Gender?)request.Gender.Value : null,
                    DateOfBirth = request.DateOfBirth,
                    JoiningDate = request.JoiningDate,
                    DesignationId = request.DesignationId,
                    Document = new ClubOfficialDocument()
                };

                if (request.File != null && request.File.Length > 0)
                {
                    var filename = request.File.FileName;
                    var extension = Path.GetExtension(filename);

                    await using var ms = new MemoryStream();
                    await request.File.CopyToAsync(ms);
                    clubOfficial.Document.FileContent = ms.ToArray();
                    clubOfficial.Document.FileName = filename;
                    clubOfficial.Document.Extension = extension;
                }

                var result = await _clubService.AddOrUpdateClubOfficialAsync(clubOfficial, userid);

                if (!result.Succeeded)
                {
                    return Ok(new { success = false, message = _localizer["Controller.Club.Message.Failed.AddOrUpdateClubOfficialAsync"].Value, Error = result.Messages });
                }

                return Ok(new { success = true, message = _localizer["Controller.Club.Message.Succeeded.AddOrUpdateClubOfficialAsync"].Value, Error = result.Messages });

            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                ModelState.AddModelError(string.Empty, e.Message);
            }

            return Ok(new { success = false, message = _localizer["Common.Message.ExceptionOccurred"].Value });
        }


        [HttpGet("GetClubOfficialsByDepartment")]
        public async Task<IActionResult> GetClubOfficialsByDepartmentAsync()
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

            var result = await _clubService.GetClubOfficialsByDepartmentAsync(userid);

            var roles = new Dictionary<string, List<DepartmentOfficialDto>>();

            if (result.Succeeded)
            {
                roles = result.Data;
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = _localizer["Controller.Club.Message.Failed.GetClubOfficialsByDepartmentAsync"].Value, Data = roles, Error = result.Messages });
            }

            return Ok(new { success = true, message = _localizer["Controller.Club.Message.Succeeded.GetClubOfficialsByDepartmentAsync"].Value, Data = roles, Error = result.Messages });
        }

        [HttpPatch("club-officials/{id}/remove")]
        public async Task<IActionResult> RemoveClubOfficialAsync(Guid id)
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

                var status = await _clubService.RemoveClubOfficialAsync(id, userid);

                if (!status.Succeeded)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = _localizer["Controller.Club.Message.Failed.RemoveClubOfficialAsync"].Value,
                        errors = status.Messages
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = _localizer["Controller.Club.Message.Succeeded.RemoveClubOfficialAsync"].Value,
                    errors = (string[]?)null
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while soft deleting a club official.");

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = _localizer["Common.Message.ExceptionOccurred"].Value,
                    error = e.Message
                });
            }
        }


        [HttpGet("GetMatchLinup/{matchId}")]
        public async Task<IActionResult> GetMatchLinupbyClud(Guid matchId)
        {
            Guid organizerId = _currentUser.GetClubId()?? Guid.Empty;
            var result = await _clubService.GetMatchLinupbyCludAsync(matchId, organizerId);

            if (result.Succeeded)
            {
                return Ok(new
                {
                    success = true,
                    message = _localizer["Controller.Club.Message.Succeeded.GetMatchLinupbyClud"].Value,
                    Data = result.Data,
                });
            }

            return Ok(new
            {
                success = false,
                message = _localizer["Controller.Club.Message.Failed.GetMatchLinupbyClud"].Value,
                Error = result.Messages
            });
        }

        [HttpGet("GetClubStats")]
        public async Task<IActionResult> GetClubStats()
        {
            Guid organizerId = _currentUser.GetClubId() ?? Guid.Empty;
            var result = await _clubService.GetClubStatsAsync(organizerId);

            if (result.Succeeded)
            {
                return Ok(new
                {
                    success = true,
                    message = _localizer["Controller.Club.Message.Succeeded.GetClubStats"].Value,
                    Data = result.Data,
                });
            }

            return Ok(new
            {
                success = false,
                message = _localizer["Controller.Club.Message.Failed.GetClubStats"].Value,
                Error = result.Messages
            });
        }

    }
}
