using Azure.Core;
using Fubaza.Application.Core.Contracts;
using Fubaza.Application.Core.Contracts.Services;
using Fubaza.Application.Core.Entities;
using Fubaza.Application.Core.Extensions;
using Fubaza.Application.Core.Interfaces.Repositories;
using Fubaza.Application.Core.Interfaces.Services;
using Fubaza.Application.Core.Settings;
using Fubaza.Application.Core.Wrapper;
using Fubaza.Application.Dto.Services;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Enums;
using Fubaza.Application.DTO.Services;
using Fubaza.Application.Infrastructure.Repositories;
using Hangfire;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using static Fubaza.Application.Core.Constants.Permissions;



namespace Fubaza.Application.Infrastructure.Services
{
    public class EventService : IEventService
    {
        private readonly ILogger<EventService> _logger;
        private readonly HttpClient _httpClient; 
        private readonly IEventRepository _repository;
        private readonly IFileService _fileService;
        private readonly AppSettings _appSettings;

        public EventService(ILogger<EventService> logger,
            IFileService fileService,
            IEventRepository repository,
            HttpClient httpClient,
            IOptions<AppSettings> appSettings
            )
        {
            _logger = logger;
            _repository = repository;
            _fileService = fileService;
            _httpClient = httpClient;
            _appSettings = appSettings.Value;
        }

        public async Task<IResult<bool>> SchedulePostAsync(Post post)
        {
            const string message = "Unable to add the Post";

            try
            {
                var origin = _appSettings.BaseUrl;
                // Upload file if needed
                if (post.Document != null && !string.IsNullOrEmpty(post.Document.FileName))
                {
                    post.Document.FileUrl = await _fileService.UploadAsync(new UploadRequest
                    {
                        FileContent = post.Document.FileContent,
                        Extension = post.Document.Extension,
                        FileName = post.Document.FileName,
                        UploadType = UploadType.PostRequest
                    });
                }

                var savedPost = await _repository.SchedulePostAsync(post);
                if (savedPost == null)
                    return await Result<bool>.FailAsync(message);

                _logger.LogInformation($"Post scheduled for {post.ScheduleDateTime}");


                var scheduleTime = savedPost.ScheduleDateTime.Value;

                if (scheduleTime <= DateTime.UtcNow)
                {
                    scheduleTime = DateTime.UtcNow.AddSeconds(10); // fallback
                }

                if (!savedPost.IsDraft)
                {
                    _logger.LogInformation($"🧭 Routing post {savedPost.Id} | IsFacebookLogin={savedPost.IsFacebookLogin} | Targets={savedPost.Targets.Count} (FB={savedPost.Targets.Count(t => t.IsFacebook)}, IG={savedPost.Targets.Count(t => t.IsInstagram)}) | scheduleTime={scheduleTime:o}");

                    if (savedPost.IsFacebookLogin)
                    {
                        foreach (var target in savedPost.Targets)
                        {
                            if (target.IsFacebook)
                            {
                                _logger.LogInformation($"🗓️ Scheduling PublishToFacebookAsyncV3 for post {savedPost.Id} | Page {target.PageId}");
                                BackgroundJob.Schedule<IJobService>(processor =>
                                  processor.PublishToFacebookAsyncV3(savedPost.Id.ToString()),
                                  scheduleTime
                                );
                            }
                            if (target.IsInstagram)
                            {
                                _logger.LogInformation($"🗓️ Scheduling PublishToInstagramAsync (FB-login) for post {savedPost.Id} | IG {target.InstagramBusinessId}");
                                BackgroundJob.Schedule<IJobService>(processor =>
                                  processor.PublishToInstagramAsync(savedPost.Id.ToString()),
                                  scheduleTime
                                );
                            }
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"🗓️ Scheduling PublishToInstagramLoginAsync (direct) for post {savedPost.Id}");
                        BackgroundJob.Schedule<IJobService>(processor =>
                            processor.PublishToInstagramLoginAsync(savedPost.Id.ToString()),
                            scheduleTime
                        );
                        //for test
                        //await PublishToInstagramLoginAsync(savedPost.Id.ToString());
                    }
                }
                else
                {
                    _logger.LogInformation($"💾 Post {savedPost.Id} saved as draft — no publish job scheduled.");
                }

                return await Result<bool>.SuccessAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<bool>.FailAsync(e.Message);
            }
        }
        public async Task<IResult<bool>> CancelPostAsync(Guid postId)
        {
            const string message = "Unable to cancel the post";
            try
            {
                var response = await _repository.CancelPostAsync(postId);
                if (response)
                {
                    return await Result<bool>.SuccessAsync(response,"Post cancelled successfully");
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
        public async Task<IResult<List<CalendarEventDto>>> GetCalendarEventsAsync(Guid userId)
        {
            const string message = "Unable to add the MatchDay";
            try
            {
                var response = await _repository.GetCalendarEventsAsync(userId);

                if (response != null)
                {
                    return await Result<List<CalendarEventDto>>.SuccessAsync(response);
                }

                _logger.LogError(message);
                return await Result<List<CalendarEventDto>>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<List<CalendarEventDto>>.FailAsync(e.Message);
            }
        }
        public async Task<IResult<List<UpcomingPostDto>>> GetUpcomingPostsAsync(Guid userId)
        {
            const string message = "Unable to get the Upcoming Post";
            try
            {
                var response = await _repository.GetUpcomingPostsAsync(userId);

                if (response != null)
                {
                    return await Result<List<UpcomingPostDto>>.SuccessAsync(response);
                }

                _logger.LogError(message);
                return await Result<List<UpcomingPostDto>>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<List<UpcomingPostDto>>.FailAsync(e.Message);
            }
        }
        public async Task<IResult<List<DraftPost>>> GetDraftPostsAsync(Guid userId)
        {
            const string message = "Unable to add the Draft Posts";
            try
            {
                var response = await _repository.GetDraftPostsAsync(userId);

                if (response != null)
                {
                    return await Result<List<DraftPost>>.SuccessAsync(response);
                }

                _logger.LogError(message);
                return await Result<List<DraftPost>>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<List<DraftPost>>.FailAsync(e.Message);
            }
        }
        public async Task<IResult<bool>> DeleteDraftPostAsync(Guid postId)
        {
            const string message = "Unable to delete  Post";
            try
            {

                var response = await _repository.DeleteDraftPostAsync(postId);
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
        public async Task<IResult<bool>> SetMatchDayAsync(Matchday matchDay)
        {
            const string message = "Unable to add the MatchDay";
            try
            {
                // Upload file if needed
                if (matchDay.SponsorDocuments != null && matchDay.SponsorDocuments.Any())
                {
                    for (int i = 0; i < matchDay.SponsorDocuments.Count(); i++)
                    {
                        if (matchDay.SponsorDocuments[i].FileUrl == null && matchDay.SponsorDocuments[i].FileContent != null)
                        {
                            matchDay.SponsorDocuments[i].FileUrl = await _fileService.UploadAsync(new UploadRequest
                            {
                                FileContent = matchDay.SponsorDocuments[i].FileContent,
                                Extension = matchDay.SponsorDocuments[i].Extension,
                                FileName = matchDay.SponsorDocuments[i].FileName,
                                UploadType = UploadType.MatchDaySponsor
                            });
                        }
                    }
                }

                var response = await _repository.SetMatchDayAsync(matchDay);

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
        public async Task<IResult<List<UpcomingMatchDto>>> GetUpcomingMatchsAsync(Guid userId)
        {
            const string message = "Unable to add the Upcoming Matchs";
            try
            {
                var response = await _repository.GetUpcomingMatchsAsync(userId);

                if (response != null)
                {
                    return await Result<List<UpcomingMatchDto>>.SuccessAsync(response);
                }

                _logger.LogError(message);
                return await Result<List<UpcomingMatchDto>>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<List<UpcomingMatchDto>>.FailAsync(e.Message);
            }
        }
        public async Task<IResult<bool>> StartMatchAsync(StartMatchRequest request)
        {
            const string message = "Unable to start Match";
            try
            {
                var response = await _repository.StartMatchAsync(request);

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
        public async Task<IResult<bool>> EndMatchAsync(Guid matchId)
        {
            const string message = "Unable to end match";
            try
            {
                var response = await _repository.EndMatchAsync(matchId);

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
        public async Task<IResult<bool>> AddMatchSummaryAsync(MatchSummary matchSummary)
        {
            const string message = "Unable to Add Match Summary";
            try
            {
                var response = await _repository.AddMatchSummaryAsync(matchSummary);

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
        public async Task<IResult<List<MatchSummaryDto>>> GetMatchSummaryAsync(Guid matchId)
        {
            const string message = "Unable to fetch the Match Summary";
            try
            {
                var response = await _repository.GetMatchSummaryAsync(matchId);

                if (response != null)
                {
                    return await Result<List<MatchSummaryDto>>.SuccessAsync(response);
                }

                _logger.LogError(message);
                return await Result<List<MatchSummaryDto>>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<List<MatchSummaryDto>>.FailAsync(e.Message);
            }
        }
        public async Task<IResult<Dictionary<string, CategoryPlayerDto>>> GetLastMatchLineUpAsync(Guid clubId)
        {
            try
            {
                var result = await _repository.GetLastMatchLineUpAsync(clubId);

                if (result != null)
                {
                    return await Result<Dictionary<string, CategoryPlayerDto>>.SuccessAsync(result);
                }

                const string message = "Unable to get the Player";

                _logger.LogError(message);
                return await Result<Dictionary<string, CategoryPlayerDto>>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<Dictionary<string, CategoryPlayerDto>>.FailAsync(e.Message);
            }
        }
        public async Task<IResult<Dictionary<string, CategoryPlayerDto>>> GetCurrentMatchLineUpAsync(Guid matchId)
        {
            try
            {
                var result = await _repository.GetCurrentMatchLineUpAsync(matchId);

                if (result != null)
                {
                    return await Result<Dictionary<string, CategoryPlayerDto>>.SuccessAsync(result);
                }

                const string message = "Unable to get the Player";

                _logger.LogError(message);
                return await Result<Dictionary<string, CategoryPlayerDto>>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<Dictionary<string, CategoryPlayerDto>>.FailAsync(e.Message);
            }
        }
        public async Task<IResult<MatchStatsDto>> GetMatchFairnessAsync(Guid matchId)
        {
            const string message = "Unable to fetch the Match Fairness";
            try
            {
                var response = await _repository.GetMatchFairnessAsync(matchId);

                if (response != null)
                {
                    return await Result<MatchStatsDto>.SuccessAsync(response);
                }

                _logger.LogError(message);
                return await Result<MatchStatsDto>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<MatchStatsDto>.FailAsync(e.Message);
            }
        }
        public async Task<IResult<List<MatchHistoryDto>>> GetMatchHistoryAsync(Guid userId)
        {
            const string message = "Unable to fetch the Match History";
            try
            {
                var response = await _repository.GetMatchHistoryAsync(userId);

                if (response != null)
                {
                    return await Result<List<MatchHistoryDto>>.SuccessAsync(response);
                }

                _logger.LogError(message);
                return await Result<List<MatchHistoryDto>>.FailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return await Result<List<MatchHistoryDto>>.FailAsync(e.Message);
            }
        }

        public async Task<IResult<bool>> SetMatchLineUpAsync(MatchLineUpRequest request)
        {
            const string errorKey =
                "Controller.Event.Error.SetMatchLineUp";

            try
            {
                // 🔹 Validate lineup
                var validation = await ValidateLineUpAsync(request);
                if (!validation.Succeeded)
                    return await Result<bool>.FailAsync(validation.Messages, validation.Arguments ?? Array.Empty<object>());

                // 🔹 Save lineup
                var result = await _repository.SetMatchLineUpAsync(request);

                if (result)
                {
                    return await Result<bool>.SuccessAsync(
                        true,
                        "Controller.Event.Notification.SetMatchLineUp");
                }

                _logger.LogError(errorKey);
                return await Result<bool>.FailAsync(errorKey);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.GetMessage());
                return await Result<bool>.FailAsync(errorKey);
            }
        }
        
        #region method

        private async Task<IResult> ValidateLineUpAsync(MatchLineUpRequest request)
        {
            if (request.PlayerIds == null || !request.PlayerIds.Any())
                return await Result.FailAsync(
                    "EventService.ValidateLineUpAsync.NoPlayers");

            var players = await _repository.GetLinePlayerAsync(request.PlayerIds);

            if (players.Count != request.PlayerIds.Count)
                return await Result.FailAsync(
                    "EventService.ValidateLineUpAsync.PlayerNotFound");

            var sportId = players.First().PlayingPosition?.SportId;
            if (sportId == null)
                return await Result.FailAsync(
                    "EventService.ValidateLineUpAsync.InvalidSport");

            var categories = players
                .Where(p => p.PlayingPosition!.Category.HasValue)
                .GroupBy(p => p.PlayingPosition!.Category!.Value)
                .ToDictionary(g => g.Key, g => g.Count());

            return await ValidateBySportAsync(
                sportId.Value,
                players.Count,
                categories);
        }
        private async Task<IResult> ValidateBySportAsync(Guid sportId, int totalPlayers, Dictionary<PositionCategory, int> categories)
        {
            var sport = await _repository.GetSportAsync(sportId);

            if (sport == null || string.IsNullOrWhiteSpace(sport.NormalizedName))
            {
                return await Result.FailAsync(
                    "EventService.ValidateBySportAsync.InvalidSport");
            }

            if (!SportLineupRules.TryGetValue(sport.NormalizedName, out var rule))
            {
                return await Result.FailAsync(
                    "EventService.ValidateBySportAsync.NotDefined",
                    new object[] { LocalizationExtensions.Localize(sport.Name, sport.NameDe) });
            }

            //if (totalPlayers < rule.Total)
            //{
            //    return await Result.FailAsync(
            //        "EventService.ValidateBySportAsync.MinPlayers",
            //        new object[] { LocalizationExtensions.Localize(sport.Name, sport.NameDe), rule.Total });
            //}

            //foreach (var requirement in rule.Rules)
            //{
            //    if (!categories.TryGetValue(requirement.Key, out var count)
            //        || count < requirement.Value)
            //    {
            //        return await Result.FailAsync(
            //            "EventService.ValidateBySportAsync.MinPosition",
            //            new object[]
            //            {
            //        LocalizationExtensions.Localize(sport.Name , sport.NameDe),
            //        requirement.Value,
            //        requirement.Key.GetLocalizedEnum()
            //            });
            //    }
            //}

            return await Result.SuccessAsync();
        }

        public async Task<IResult<bool>> DeleteMatchSummaryAsync(Guid summaryId)
        {
            const string message = "Unable to delete Summary";
            try
            {
                var response = await _repository.DeleteMatchSummaryAsync(summaryId);
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

        public async Task<IResult<bool>> DeleteMatchDayAsync(Guid matchId)
        {
            const string message = "Unable to delete Match day";
            try
            {
                var response = await _repository.DeleteMatchDayAsync(matchId);
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


        private static readonly Dictionary<string, (int Total, Dictionary<PositionCategory, int> Rules)> SportLineupRules = new()
        {
            ["football"] = (11, new()
            {
            { PositionCategory.Football_Goalkeeper, 1 },
            { PositionCategory.Football_Defender, 4 },
            { PositionCategory.Football_Midfield, 4 },
            { PositionCategory.Football_Striker, 2 }
            }),

            ["handball"] = (7, new()
            {
            { PositionCategory.Handball_Goalkeeper, 1 },
            { PositionCategory.Handball_Wing, 2 },
            { PositionCategory.Handball_Back, 2 },
            { PositionCategory.Handball_Center, 1 },
            { PositionCategory.Handball_Pivot, 1 }
            }),

            ["basketball"] = (5, new()
            {
            { PositionCategory.Basketball_BackCourt, 2 },
            { PositionCategory.Basketball_Forward, 2 },
            { PositionCategory.Basketball_FrontCourt, 1 }
            }),


            ["americanFootball"] = (11, new()
            {
            { PositionCategory.AmericanFootball_Quarterback, 1 },
            { PositionCategory.AmericanFootball_Offense, 5 },
            { PositionCategory.AmericanFootball_Special_Teams, 5 }
            }),

            ["volleyball"] = (6, new()
            {
            { PositionCategory.Volleyball_Setter, 1 },
            { PositionCategory.Volleyball_Hitter, 4 },
            { PositionCategory.Volleyball_DefensiveSpecialist, 1 }
            }),


            ["iceHockey"] = (6, new()
            {
            { PositionCategory.IceHockey_Goaltender, 1 },
            { PositionCategory.IceHockey_Defender, 2 },
            { PositionCategory.IceHockey_Forward, 3 }
            })
        };

        #endregion
        public async Task PublishToInstagramLoginAsync(string Id)
        {
            try
            {
                var post = await _repository.GetPostByIdAsync(Guid.Parse(Id));
                var instagramlogin = await _repository.GetInstagramLoginAsync(post.UserId);
                if (!string.IsNullOrEmpty(instagramlogin.Item1) && !string.IsNullOrEmpty(instagramlogin.Item2))
                {
                    var imageUrl = post.Document?.FileUrl;
                    var createUrl = $"https://graph.instagram.com/v25.0/{instagramlogin.Item2}/media";

                    var payload = new Dictionary<string, string>
                        {
                            { "image_url", imageUrl ?? string.Empty },
                            { "caption", post.Caption ?? string.Empty }
                        };

                    // Step 1️⃣: Create media container
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", instagramlogin.Item1);
                    var createResponse = await _httpClient.PostAsync(createUrl, new FormUrlEncodedContent(payload));
                    var createText = await createResponse.Content.ReadAsStringAsync();

                    if (createResponse.IsSuccessStatusCode)
                    {
                        var json = JObject.Parse(createText);
                        var creationId = json["id"]?.ToString();

                        if (!string.IsNullOrEmpty(creationId))
                        {
                            // Step 2️⃣: Publish the media
                            var publishUrl = $"https://graph.instagram.com/v25.0/{instagramlogin.Item2}/media_publish";
                            var publishPayload = new Dictionary<string, string>
                            {
                                { "creation_id", creationId },
                            };
                            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", instagramlogin.Item1);
                            var publishResponse = await _httpClient.PostAsync(publishUrl, new FormUrlEncodedContent(publishPayload));
                            var publishText = await publishResponse.Content.ReadAsStringAsync();

                            if (publishResponse.IsSuccessStatusCode)
                            {
                                //target.IsPublished = true;
                                //await _eventRepository.UpdatePostIdAsync(target);
                                _logger.LogInformation($"✅ Instagram post published successfully for IG {instagramlogin.Item2} - Post ID: {creationId}");
                            }

                            _logger.LogError($"❌ Instagram publish failed: {publishText}");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error posting to Instagram for target {ex.Message}");
                throw;
            }
        }

    }
}
