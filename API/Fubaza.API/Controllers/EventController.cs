using Azure;
using Fubaza.API.Extensions;
using Fubaza.API.Resources;
using Fubaza.Application.Core.Contracts.Services.Identity;
using Fubaza.Application.Core.Entities;
using Fubaza.Application.Core.Interfaces.Services;
using Fubaza.Application.DTO.DTO;

using Fubaza.Application.DTO.Services;
using Fubaza.Application.DTO.Enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;


namespace Fubaza.API.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly ILogger<EventController> _logger;
        private readonly IEventService _eventService;
        private readonly ICurrentUser _currentUser;
        private readonly IStringLocalizer<SharedResource> _localizer;
        public EventController(
           ILogger<EventController> logger,
           IEventService eventService,
           ICurrentUser currentUser,
           IStringLocalizer<SharedResource> localizer
          )
        {
            _logger = logger;
            _eventService = eventService;
            _currentUser = currentUser;
            _localizer = localizer;
        }

        [HttpPost("SchedulePost")]
        public async Task<IActionResult> SchedulePostAsync([FromForm] PostRequest request)
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


                var post = new Post
                {
                    Id = request.PostId ?? Guid.Empty,
                    UserId = userid,
                    Caption = request.Caption,
                    ScheduleDateTime = request.ScheduleDateTime,
                    IsDraft = request.IsDraft,
                    IsFacebookLogin = request.IsFacebookLogin,
                    Document = new PostDocument()
                };

                if (request.File != null && request.File.Length > 0)
                {
                    var filename = request.File.FileName;
                    var extension = Path.GetExtension(filename);

                    await using var ms = new MemoryStream();
                    await request.File.CopyToAsync(ms);

                    var postdocument = new PostDocument
                    {
                        FileContent = ms.ToArray(),
                        FileName = filename,
                        Extension = extension,
                    };

                    post.Document = postdocument;
                }

                foreach (var target in request.PostTarget)
                {
                    var isFacebook = !string.IsNullOrEmpty(target.PageId);
                    var isInstagram = !string.IsNullOrEmpty(target.InstagramBusinessId);

                    if (string.IsNullOrEmpty(target.AccessToken))
                        continue;

                    post.Targets.Add(new PostTarget
                    {
                        PageId = target.PageId,
                        InstagramBusinessId = target.InstagramBusinessId,
                        AccessToken = target.AccessToken,
                        IsFacebook = isFacebook,
                        IsInstagram = isInstagram
                    });
                }

                var result = await _eventService.SchedulePostAsync(post);

                if (!result.Succeeded)
                {
                  
                    var failMessage = post.IsDraft ? _localizer["Controller.Event.Message.Failed.SaveDraft"].Value : _localizer["Controller.Event.Message.Failed.SchedulePost"].Value;

                    return Ok(new { success = false, message = failMessage, Error = result.Messages });
                }

                var successMessage = post.IsDraft ? _localizer["Controller.Event.Message.Succeeded.SaveDraft"].Value  : _localizer["Controller.Event.Message.Succeeded.SchedulePost"].Value;

                return Ok(new { success = true, message = successMessage, Error = result.Messages });

            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                ModelState.AddModelError(string.Empty, e.Message);
            }

            return Ok(new { success = false,  message = _localizer["Common.Message.ExceptionOccurred"].Value });
        }

        [HttpGet("GetCalendarEvents")]
        public async Task<IActionResult> GetCalendarEventsAsync()
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

            var result = await _eventService.GetCalendarEventsAsync(userid);

            var events = new List<CalendarEventDto>();

            if (result.Succeeded)
            {
                events = result.Data.ToList();
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = _localizer["Controller.Event.Message.Failed.GetCalendarEventsAsync"].Value, Data = events, Error = result.Messages });
            }

            return Ok(new { success = true, message = _localizer["Controller.Event.Message.Succeeded.GetCalendarEventsAsync"].Value, Data = events, Error = result.Messages });
        }

        [HttpGet("GetUpcomingPosts")]
        public async Task<IActionResult> GetUpcomingPostsAsync()
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

            var result = await _eventService.GetUpcomingPostsAsync(userid);

            var post = new List<UpcomingPostDto>();

            if (result.Succeeded)
            {
                post = result.Data.ToList();
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = _localizer["Controller.Event.Message.Failed.GetUpcomingPostsAsync"].Value, Data = post, Error = result.Messages });
            }

            return Ok(new { success = true, message = _localizer["Controller.Event.Message.Succeeded.GetUpcomingPostsAsync"].Value, Data = post, Error = result.Messages });
        }

        [HttpGet("GetDraftPosts")]
        public async Task<IActionResult> GetDraftPostsAsync()
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

            var result = await _eventService.GetDraftPostsAsync(userid);

            var post = new List<DraftPost>();

            if (result.Succeeded)
            {
                post = result.Data.ToList();
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = _localizer["Controller.Event.Message.Failed.GetDraftPostsAsync"].Value, Data = post, Error = result.Messages });
            }

            return Ok(new { success = true, message = _localizer["Controller.Event.Message.Succeeded.GetDraftPostsAsync"].Value, Data = post, Error = result.Messages });
        }

        [HttpDelete("DeleteDraftPost/{postId}")]
        public async Task<IActionResult> DeleteDraftPostAsync([FromRoute] Guid postId)
        {
            var result = await _eventService.DeleteDraftPostAsync(postId);

            if (!result.Succeeded)
            {
                return Ok(new
                {
                    success = false,
                    message = _localizer["Controller.Event.Message.Failed.DeleteDraftPostAsync"].Value,
                    errors = result.Messages
                });
            }

            return Ok(new
            {
                success = true,
                message = _localizer["Controller.Event.Message.Succeeded.DeleteDraftPostAsync"].Value,
                errors = result.Messages
            });
        }

        [HttpGet("CancelPost/{postId}")]
        public async Task<IActionResult> CancelPostAsync([FromRoute] Guid postId)
        {
            var result = await _eventService.CancelPostAsync(postId);

            if (!result.Succeeded)
            {
                return Ok(new
                {
                    success = false,
                    message = _localizer["Controller.Event.Message.Failed.CancelPostAsync"].Value,
                    errors = result.Messages
                });
            }

            return Ok(new
            {
                success = true,
                message = _localizer["Controller.Event.Message.Succeeded.CancelPostAsync"].Value,
                errors = ""
            });
        }

        [HttpPost("SetMatchDay")]
        public async Task<IActionResult> SetMatchDayAsync([FromForm] MatchDayRequest matchDayRequest)
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
                var matchday = new Matchday
                {
                    Id = matchDayRequest.Id ?? Guid.Empty,
                    MatchDayDateTime = matchDayRequest.MatchDayDateTime,
                    MatchdayNumber = matchDayRequest.MatchdayNumber,
                    OrganizerClubId = matchDayRequest.OrganizerClubId,
                    OpponentClubId = matchDayRequest.OpponentClubId,
                    Referee = matchDayRequest.Referee,
                    AssistantReferee1 = matchDayRequest.AssistantReferee1,
                    AssistantReferee2 = matchDayRequest.AssistantReferee2,
                    Location = matchDayRequest.Location,
                    CompetitionType = (CompetitionType?)matchDayRequest.CompetitionType,
                    Venue = matchDayRequest.Venue.HasValue ? (MatchVenue)matchDayRequest.Venue.Value : MatchVenue.Home,
                    UserId= userId,
                    SponsorDocuments = new List<SponsorDocument>()
                };

                if (matchDayRequest.SponsorDocuments != null && matchDayRequest.SponsorDocuments.Any())
                {
                    foreach (var doc in matchDayRequest.SponsorDocuments)
                    {
                        var sponsorDocument = new SponsorDocument
                        {
                            MatchdayId = matchday.Id,
                            Sponsor = doc.Sponsor,
                        };
                        if (doc.File != null && doc.File.Length > 0)
                        {
                            var fileName = doc.File.FileName;
                            var extension = Path.GetExtension(fileName);

                            await using var ms = new MemoryStream();
                            await doc.File.CopyToAsync(ms);

                            sponsorDocument.FileContent = ms.ToArray();
                            sponsorDocument.FileName = fileName;
                            sponsorDocument.Extension = extension;
                        }
                        matchday.SponsorDocuments.Add(sponsorDocument);
                    }
                }

                var result = await _eventService.SetMatchDayAsync(matchday);

                if (!result.Succeeded)
                {
                    return Ok(new { success = false, message = _localizer["Controller.Event.Message.Failed.SetMatchDayAsync"].Value, Error = result.Messages });
                }

                return Ok(new { success = true, message = _localizer["Controller.Event.Message.Succeeded.SetMatchDayAsync"].Value, Error = result.Messages });

            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                ModelState.AddModelError(string.Empty, e.Message);
            }

            return Ok(new { success = false, message = _localizer["Common.Message.ExceptionOccurred"].Value });
        }

        [HttpDelete("DeleteMatchDay/{matchId}")]
        public async Task<IActionResult> DeleteMatchDayAsync([FromRoute] Guid matchId)
        {
            var result = await _eventService.DeleteMatchDayAsync(matchId);

            if (!result.Succeeded)
            {
                return Ok(new
                {
                    success = false,
                    message = _localizer["Controller.Event.Message.Failed.DeleteMatchDayAsync"].Value,
                    errors = result.Messages
                });
            }

            return Ok(new
            {
                success = true,
                message = _localizer["Controller.Event.Message.Succeeded.DeleteMatchDayAsync"].Value,
                errors = result.Messages
            });
        }

        [HttpGet("GetUpcomingMatchs")]
        public async Task<IActionResult> GetUpcomingMatchsAsync()
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

            var result = await _eventService.GetUpcomingMatchsAsync(userid);

            var match = new List<UpcomingMatchDto>();

            if (result.Succeeded)
            {
                match = result.Data.ToList();
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = _localizer["Controller.Event.Message.Failed.GetUpcomingMatchsAsync"].Value, Data = match, Error = result.Messages });
            }

            return Ok(new { success = true, message = _localizer["Controller.Event.Message.Succeeded.GetUpcomingMatchsAsync"].Value , Data = match, Error = result.Messages });
        }

        [HttpPost("StartMatch")]
        public async Task<IActionResult> StartMatchAsync([FromForm] StartMatchRequest request)
        {
            try
            {
                var result = await _eventService.StartMatchAsync(request);

                if (!result.Succeeded)
                {
                    return Ok(new
                    {
                        success = false,
                        message = _localizer["Controller.Event.Message.Failed.StartMatchAsync"].Value,
                        error = result.Messages
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = _localizer["Controller.Event.Message.Succeeded.StartMatchAsync"].Value,
                    error = result.Messages
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

        [HttpGet("EndMatch/{matchId}")]
        public async Task<IActionResult> EndMatchAsync(Guid matchId)
        {

            try
            {
                var result = await _eventService.EndMatchAsync(matchId);

                if (!result.Succeeded)
                {
                    return Ok(new
                    {
                        success = false,
                        message = _localizer["Controller.Event.Message.Failed.EndMatchAsync"].Value,
                        error = result.Messages
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = _localizer["Controller.Event.Message.Succeeded.EndMatchAsync"].Value,
                    error = result.Messages
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

        [HttpPost("AddMatchSummary")]
        public async Task<IActionResult> AddMatchSummaryAsync([FromForm] MatchSummaryRequest matchSummaryRequest)
        {
            try
            {
                var matchSummary = new MatchSummary
                {
                    Id = matchSummaryRequest.Id ?? Guid.Empty,
                    EventTypeId = matchSummaryRequest.EventTypeId,
                    Minute = matchSummaryRequest.Minute,
                    Description = matchSummaryRequest.Description,
                    ClubId = matchSummaryRequest.ClubId,
                    MatchdayId = matchSummaryRequest.MatchdayId,
                    PlayerId =matchSummaryRequest.PlayerId,
                    AssistPlayerId = matchSummaryRequest.AssistPlayerId
                };

                var result = await _eventService.AddMatchSummaryAsync(matchSummary);

                if (!result.Succeeded)
                {
                    return Ok(new { success = false, message = _localizer["Controller.Event.Message.Failed.AddMatchSummaryAsync"].Value, Error = result.Messages });
                }

                return Ok(new { success = true, message = _localizer["Controller.Event.Message.Succeeded.AddMatchSummaryAsync"].Value, Error = result.Messages });
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                ModelState.AddModelError(string.Empty, e.Message);
            }

            return Ok(new { success = false, message = _localizer["Common.Message.ExceptionOccurred"].Value });
        }

        [HttpDelete("DeleteMatchSummary/{summaryId}")]
        public async Task<IActionResult> DeleteMatchSummaryAsync([FromRoute] Guid summaryId)
        {
            var result = await _eventService.DeleteMatchSummaryAsync(summaryId);

            if (!result.Succeeded)
            {
                return Ok(new
                {
                    success = false,
                    message = _localizer["Controller.Event.Message.Failed.DeleteMatchSummaryAsync"].Value,
                    errors = result.Messages
                });
            }

            return Ok(new
            {
                success = true,
                message = _localizer["Controller.Event.Message.Succeeded.DeleteMatchSummaryAsync"].Value,
                errors = result.Messages
            });
        }

        [HttpGet("GetMatchSummary/{matchId}")]
        public async Task<IActionResult> GetMatchSummaryAsync(Guid matchId)
        {
           

            var result = await _eventService.GetMatchSummaryAsync(matchId);

            var match = new List<MatchSummaryDto>();

            if (result.Succeeded)
            {
                match = result.Data.ToList();
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = _localizer["Controller.Event.Message.Failed.GetMatchSummaryAsync"].Value, Data = match, Error = result.Messages });
            }

            return Ok(new { success = true, message = _localizer["Controller.Event.Message.Succeeded.GetMatchSummaryAsync"].Value, Data = match, Error = result.Messages });
        }

        [HttpPost("SetMatchLineUp")]
        public async Task<IActionResult> SetMatchLineUpAsync([FromForm] MatchLineUpRequest request)
        {
            try
            {
                var result = await _eventService.SetMatchLineUpAsync(request);

                if (!result.Succeeded)
                {
                    return Ok(
                        new { 
                            success = false,
                            message = _localizer["Controller.Event.Message.Failed.SetMatchLineUpAsync"].Value,
                            Error = _localizer["Controller.Event.Message.Failed.SetMatchLineUpAsync"].Value }
                        );
                }
                return Ok(new { 
                    success = true,
                    message = _localizer["Controller.Event.Message.Succeeded.SetMatchLineUpAsync"].Value,
                    Error = "" }
                );

            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                ModelState.AddModelError(string.Empty, e.Message);
            }

            return Ok(new { success = false, message = _localizer["Common.Message.ExceptionOccurred"].Value, });
        }

        [HttpGet("GetLastMatchLineUp")]
        public async Task<IActionResult> GetLastMatchLineUpAsync()
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

            var result = await _eventService.GetLastMatchLineUpAsync(clubid);

            var roles = new Dictionary<string, CategoryPlayerDto>();

            if (result.Succeeded)
            {
                roles = result.Data;
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = _localizer["Controller.Event.Message.Failed.GetLastMatchLineUpAsync"].Value, Data = roles, Error = result.Messages });
            }

            return Ok(new { success = true, message = _localizer["Controller.Event.Message.Succeeded.GetLastMatchLineUpAsync"].Value, Data = roles, Error = result.Messages });
        }

        [HttpGet("GetCurrentMatchLineUp/{matchId}")]
        public async Task<IActionResult> GetCurrentMatchLineUpAsync(Guid matchId)
        {

            var result = await _eventService.GetCurrentMatchLineUpAsync(matchId);

            var roles = new Dictionary<string, CategoryPlayerDto>();

            if (result.Succeeded)
            {
                roles = result.Data;
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = _localizer["Controller.Event.Message.Failed.GetCurrentMatchLineUpAsync"].Value, Data = roles, Error = result.Messages });
            }

            return Ok(new { success = true, message = _localizer["Controller.Event.Message.Succeeded.GetCurrentMatchLineUpAsync"].Value, Data = roles, Error = result.Messages });
        }

        [HttpGet("GetMatchFairness/{matchId}")]
        public async Task<IActionResult> GetMatchFairnessAsync(Guid matchId)
        {

            var result = await _eventService.GetMatchFairnessAsync(matchId);

            var roles = new MatchStatsDto();

            if (result.Succeeded)
            {
                roles = result.Data;
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = _localizer["Controller.Event.Message.Failed.GetMatchFairnessAsync"].Value, Data = roles, Error = result.Messages });
            }

            return Ok(new { success = true, message = _localizer["Controller.Event.Message.Succeeded.GetMatchFairnessAsync"].Value, Data = roles, Error = result.Messages });
        }

        [HttpGet("GetMatchHistory")]
        public async Task<IActionResult> GetMatchHistoryAsync()
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

            var result = await _eventService.GetMatchHistoryAsync(userid);

            var match = new List<MatchHistoryDto>();

            if (result.Succeeded)
            {
                match = result.Data.ToList();
            }

            if (!result.Succeeded)
            {
                return Ok(new { success = false, message = _localizer["Controller.Event.Message.Failed.GetMatchHistoryAsync"].Value, Data = match, Error = result.Messages });
            }

            return Ok(new { success = true, message = _localizer["Controller.Event.Message.Succeeded.GetMatchHistoryAsync"].Value, Data = match, Error = result.Messages });
        }

    }
}
