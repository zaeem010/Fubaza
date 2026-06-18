using Fubaza.API.Extensions;
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
    public class PlayerController : ControllerBase
    {
        private readonly ILogger<PlayerController> _logger;
        private readonly IPlayerService _playerService;
        private readonly ICurrentUser _currentUser;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public PlayerController(
            ILogger<PlayerController> logger,
            IPlayerService playerService,
            ICurrentUser currentUser,
            IStringLocalizer<SharedResource> localizer
           )
        {
            _logger = logger;
            _playerService = playerService;
            _currentUser = currentUser;
            _localizer = localizer;
        }

        [HttpPost("AddOrUpdatePlayer")]
        public async Task<IActionResult> AddOrUpdatePlayerAsync([FromForm] AddOrUpdatePlayerRequest request)
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

                var player = new Player
                {
                    Id = request.PlayerId ?? Guid.Empty,
                    FullName = request.FullName,
                    DateOfBirth = request.DateOfBirth,
                    PlayingPositionId = request.PlayingPositionId,
                    Gender = request.Gender.HasValue ? (Gender?)request.Gender.Value : null,
                    CurrentClubId = request.ClubId,
                    IsCaption = request.IsCaption,
                    JerseyNumber = request.JerseyNumber,
                    Nationality = request.Nationality,
                    StrongFoot = request.StrongFoot.HasValue ? (StrongFoot?)request.StrongFoot.Value : null,
                    ThrowingHand = request.ThrowingHand.HasValue ? (ThrowingHand?)request.ThrowingHand.Value : null,
                    UserId = userid,
                    Documents = new List<PlayerDocument>(),
                    ClubHistory = new List<PlayerClubHistory>()
                };

                if (request.Images != null && request.Images.Any())
                {
                    foreach (var doc in request.Images)
                    {
                        if (doc.File != null && doc.File.Length > 0)
                        {
                            var fileName = doc.File.FileName;
                            var extension = Path.GetExtension(fileName);

                            await using var ms = new MemoryStream();
                            await doc.File.CopyToAsync(ms);

                            player.Documents.Add(new PlayerDocument
                            {
                                DocumentType = doc.ImageType,
                                FileContent = ms.ToArray(),
                                FileName = fileName,
                                Extension = extension
                            });
                        }
                    }
                }

                var result = await _playerService.AddOrUpdatePlayerAsync(player, userid);

                return Ok(new
                {
                    success = result.Succeeded,
                    message = result.Succeeded ? _localizer["Controller.Player.Message.Succeeded.AddOrUpdatePlayerAsync"].Value : _localizer["Controller.Player.Message.Failed.AddOrUpdatePlayerAsync"].Value,
                    error = result.Messages.GetCulturedList<SharedResource>(_localizer)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Ok(new
                {
                    success = false,
                    message = _localizer["Common.Message.ExceptionOccurred"].Value,
                    error = ex.Message
                });
            }
        }

        [HttpPost("UpdatePlayerProfile")]
        public async Task<IActionResult> UpdatePlayerProfileAsync([FromForm] PlayerProfileRequest request)
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

                var player = new Player
                {
                    UserId = userid,
                    FullName = request.FullName,
                    DateOfBirth = request.DateOfBirth,
                    SportId = request.SportId,
                    RoleId = request.RoleId,
                    PlayingPositionId = request.PlayingPositionId,
                    WeightKg = request.WeightKg,
                    HeightCm = request.HeightCm,
                    Nationality = request.Nationality,
                    StrongFoot = request.StrongFoot.HasValue ? (StrongFoot?)request.StrongFoot.Value : null,
                    ThrowingHand = request.ThrowingHand.HasValue ? (ThrowingHand?)request.ThrowingHand.Value : null,
                    Gender = request.Gender.HasValue ? (Gender?)request.Gender.Value : null,
                    Documents = new List<PlayerDocument>() // ✅ initialize collection
                };

                if (request.File != null && request.File.Length > 0)
                {
                    var filename = request.File.FileName;
                    var extension = Path.GetExtension(filename);

                    await using var ms = new MemoryStream();
                    await request.File.CopyToAsync(ms);

                    var profileDocument = new PlayerDocument
                    {
                        FileContent = ms.ToArray(),
                        FileName = filename,
                        Extension = extension,
                        DocumentType = PlayerDocumentType.Profile
                    };

                    player.Documents.Add(profileDocument); 
                }
                
                if (request.ClubHistory != null && request.ClubHistory.Any())
                {
                    foreach (var history in request.ClubHistory)
                    {
                        var clubHistory = new PlayerClubHistory
                        {
                            ClubId = history.ClubId,
                            StartYear = history.StartYear,
                            EndYear = history.EndYear,
                            IsCurrentClub = history.IsCurrentClub
                        };

                        player.ClubHistory.Add(clubHistory);

                        if (history.IsCurrentClub)
                        {
                            player.CurrentClubId = history.ClubId;
                        }
                    }
                }

                var profileUpdateResult = await _playerService.UpdatePlayerProfileAsync(player);

                if (!profileUpdateResult.Succeeded)
                {
                    return Ok(new
                    {
                        success = false,
                        message = _localizer["Controller.Player.Message.Failed.UpdatePlayerProfileAsync"].Value,
                        Data = profileUpdateResult.Data,
                        Error = profileUpdateResult.Messages
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = _localizer["Controller.Player.Message.Succeeded.UpdatePlayerProfileAsync"].Value,
                    Data = profileUpdateResult.Data,
                    Error = profileUpdateResult.Messages
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                ModelState.AddModelError(string.Empty, e.Message);
            }

            return Ok(new
            {
                success = false,
                message = _localizer["Common.Message.ExceptionOccurred"].Value,
            });
        }

        [HttpPost("EditPlayerProfile")]
        public async Task<IActionResult> EditPlayerProfileAsync([FromForm] EditPlayerProfileRequest request)
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

                var player = new Player
                {
                    UserId = userid,
                    FullName = request.FullName,
                    DateOfBirth = request.DateOfBirth,
                    CurrentClubId = request.CurrentClubId,
                    WeightKg = request.WeightKg,
                    HeightCm = request.HeightCm,
                    Nationality = request.Nationality,
                    StrongFoot = request.StrongFoot.HasValue ? (StrongFoot?)request.StrongFoot.Value : null,
                    ThrowingHand = request.ThrowingHand.HasValue ? (ThrowingHand?)request.ThrowingHand.Value : null,
                    Gender = request.Gender.HasValue ? (Gender?)request.Gender.Value : null,
                    PlayingPositionId = request.PlayingPositionId,
                    ClubHistory = new List<PlayerClubHistory>(),
                    Documents = new List<PlayerDocument>() // ✅ use document list
                };

                if (request.ClubHistory != null && request.ClubHistory.Any())
                {
                    foreach (var history in request.ClubHistory)
                    {
                        var clubHistory = new PlayerClubHistory
                        {
                            ClubId = history.ClubId,
                            StartYear = history.StartYear,
                            EndYear = history.EndYear,
                            IsCurrentClub = history.IsCurrentClub
                        };

                        player.ClubHistory.Add(clubHistory);

                        if (history.IsCurrentClub)
                        {
                            player.CurrentClubId = history.ClubId;
                        }
                    }
                }

                if (request.File != null && request.File.Length > 0)
                {
                    var filename = request.File.FileName;
                    var extension = Path.GetExtension(filename);

                    await using var ms = new MemoryStream();
                    await request.File.CopyToAsync(ms);

                    var document = new PlayerDocument
                    {
                        FileContent = ms.ToArray(),
                        FileName = filename,
                        Extension = extension,
                        DocumentType = PlayerDocumentType.Profile
                    };

                    player.Documents.Add(document); 
                }
               
                var profileUpdateResult = await _playerService.EditPlayerProfileAsync(player);

                if (!profileUpdateResult.Succeeded)
                {
                    return Ok(new
                    {
                        success = false,
                        message = _localizer["Controller.Player.Message.Failed.EditPlayerProfileAsync"].Value,
                        Data = profileUpdateResult.Data,
                        Error = profileUpdateResult.Messages
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = _localizer["Controller.Player.Message.Succeeded.EditPlayerProfileAsync"].Value,
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

        [HttpGet("GetPlayerProfile")]
        public async Task<IActionResult> GetPlayerProfileAsync()
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

            var result = await _playerService.GetPlayerProfileAsync(userid);

            var player = new PlayerProfileDTO();

            if (result.Succeeded)
            {
                player = result.Data;
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = _localizer["Controller.Player.Message.Failed.GetPlayerProfileAsync"].Value, Data = player, Error = result.Messages });
            }

            return Ok(new { success = true, message = _localizer["Controller.Player.Message.Succeeded.GetPlayerProfileAsync"].Value, Data = player, Error = result.Messages });
        }

        [HttpGet("GetPlayersByCategory")]
        public async Task<IActionResult> GetPlayersByCategoryAsync()
        {
            var clubId = _currentUser.GetClubId();

            if (!Guid.TryParse(clubId.ToString(), out Guid clubid))
            {
                return BadRequest(
                        new
                        {
                            success = false,
                            message = _localizer["Common.Message.InvalidOrMissingUserId"].Value,
                            Error = _localizer["Common.Error.InvalidToken"].Value
                        });
            }

            var result = await _playerService.GetPlayersByCategoryAsync(clubid);

            var roles = new Dictionary<string, CategoryPlayerDto>();

            if (result.Succeeded)
            {
                roles = result.Data;
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = _localizer["Controller.Player.Message.Failed.GetPlayersByCategoryAsync"].Value, Data = roles, Error = result.Messages });
            }

            return Ok(new { success = true, message = _localizer["Controller.Player.Message.Succeeded.GetPlayersByCategoryAsync"].Value, Data = roles, Error = result.Messages });
        }

        [HttpPatch("player/{id}/remove")]
        public async Task<IActionResult> RemovePlayerAsync(Guid id)
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


                var status = await _playerService.RemovePlayerAsync(id , userid);

                if (!status.Succeeded)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = _localizer["Controller.Player.Message.Failed.RemovePlayerAsync"].Value,
                        errors = status.Messages
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = _localizer["Controller.Player.Message.Succeeded.RemovePlayerAsync"].Value,
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

        [HttpGet("GetMediaGallery")]
        public async Task<IActionResult> GetMediaGalleryAsync()
        {
            var clubId = _currentUser.GetClubId();

            if (!Guid.TryParse(clubId.ToString(), out Guid clubid))
            {
                return BadRequest(
                        new
                        {
                            success = false,
                            message = _localizer["Common.Message.InvalidOrMissingUserId"].Value,
                            Error = _localizer["Common.Error.InvalidToken"].Value
                        });
            }

            var result = await _playerService.GetMediaGalleryAsync(clubid);

            var roles = new Dictionary<object, DocumentTypeGroupDto>();

            if (result.Succeeded)
            {
                roles = result.Data;
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = _localizer["Controller.Player.Message.Failed.GetMediaGalleryAsync"].Value, Data = roles, Error = result.Messages });
            }

            return Ok(new { success = true, message = _localizer["Controller.Player.Message.Succeeded.GetMediaGalleryAsync"].Value, Data = roles, Error = result.Messages });
        }
    }
}
