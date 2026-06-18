using AutoMapper;
using Azure.Core;
using Fubaza.Application.Core.Contracts.Services.Identity;
using Fubaza.Application.Core.Entities;
using Fubaza.Application.Core.Exceptions;
using Fubaza.Application.Core.Extensions;
using Fubaza.Application.Core.Interfaces.Repositories;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Enums;
using Fubaza.Application.DTO.Services;
using Fubaza.Application.Infrastructure.Persistence;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;



namespace Fubaza.Application.Infrastructure.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly ICurrentUser _currentUser;
        public EventRepository(ApplicationDbContext db, IMapper mapper, ICurrentUser currentUser)
        {
            _db = db;
            _mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<Post> SchedulePostAsync(Post post)
        {
            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    Post? dbPost;

                    if (post.Id == Guid.Empty)
                    {
                        // New post
                        await _db.Post.AddAsync(post);
                        dbPost = post;
                    }
                    else
                    {
                        // Update existing post
                        dbPost = await _db.Post
                            .Include(x => x.Document)
                            .Include(x => x.Targets)
                            .FirstOrDefaultAsync(c => c.Id == post.Id);

                        if (dbPost == null)
                            throw new CustomException($"Post with ID {post.Id} not found.");

                        // Update main fields
                        dbPost.Caption = post.Caption;
                        dbPost.ScheduleDateTime = post.ScheduleDateTime;
                        dbPost.IsDraft = post.IsDraft;
                        dbPost.IsCancelled = false;

                        // Handle document updates
                        if (post.Document != null)
                        {
                            if (dbPost.Document == null)
                                dbPost.Document = new PostDocument();

                            dbPost.Document.FileName = post.Document.FileName;
                            dbPost.Document.FileUrl = post.Document.FileUrl;
                            dbPost.Document.Extension = post.Document.Extension;
                        }

                        // ✅ Update targets (multiple pages/accounts)
                        await _db.PostTarget.Where(t => t.PostId == dbPost.Id).ExecuteDeleteAsync();
                        var newTargets = new List<PostTarget>();
                        foreach (var target in post.Targets)
                        {
                            newTargets.Add(new PostTarget
                            {
                                PageId = target.PageId,
                                InstagramBusinessId = target.InstagramBusinessId,
                                AccessToken = target.AccessToken,
                                IsFacebook = target.IsFacebook,
                                IsInstagram = target.IsInstagram,
                                PostId = dbPost.Id
                            });
                        }
                        await _db.PostTarget.AddRangeAsync(newTargets);
                    }

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return dbPost;
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();
                    throw new CustomException($"Unable to save post due to: {e.GetMessage()}");
                }
            }
        }
        public async Task<bool> CancelPostAsync(Guid postId)
        {
            try
            {
                await _db.PostTarget.Where(x => x.PostId == postId).ExecuteDeleteAsync();
                var post = await _db.Post
                    .Where(x => x.Id == postId)
                    .ExecuteUpdateAsync(x=>x
                    .SetProperty(z=>z.IsCancelled,true)
                    .SetProperty(z=>z.IsDraft,true)
                    );

                return true;
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to cancle Post due to: {e.GetMessage()}");
            }
        }
        public async Task<bool> UpdatePostCancleStatusAsync(Guid postId)
        {
            try
            {
                var post = await _db.Post
                    .Where(x => x.Id == postId)
                    .ExecuteUpdateAsync(x => x
                    .SetProperty(z => z.IsCancelled, false)
                    );
                return true;
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to update cancle Post status due to: {e.GetMessage()}");
            }
        }
        public async Task<List<Post>> GetDueScheduledPostsAsync(DateTime utcNow)
        {
            try
            {
                var windowStart = utcNow.AddSeconds(-30);
                var windowEnd = utcNow.AddSeconds(30);

                var posts =  await _db.Post
                            .Include(p => p.Targets.Where(x => !x.IsPublished))
                            .Include(p => p.Document)
                            .Where(p => p.ScheduleDateTime.HasValue
                                && p.ScheduleDateTime >= windowStart
                                && p.ScheduleDateTime <= windowEnd
                                && !p.IsDraft).ToListAsync();

                return posts;
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to update Post Id due to: {e.GetMessage()}");
            }
        }
        public async Task UpdatePostIdAsync(PostTarget target)
        {
            try
            {
                _db.PostTarget.Update(target);
                await _db.SaveChangesAsync();
               
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to update Post Id due to: {e.GetMessage()}");
            }
        }
        public async Task<bool> DeleteDraftPostAsync(Guid postId)
        {
            try
            {
                await _db.Post
                    .Where(t => t.Id == postId)
                    .ExecuteUpdateAsync(t => t
                        .SetProperty(x => x.IsDeleted, true)
                    );

                return true;
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to process the Post: {e.GetMessage()}");
            }
        }
        public async Task<List<Post>> GetPostsToNotifyBeforeAsync(DateTime utcNow, CancellationToken cancellationToken = default)
        {
            try
            {
                var targetTime = utcNow.AddHours(1);

                return await _db.Post
                    .Include(x=>x.Targets)
                    .Include(x => x.User)
                    .Where(p => p.ScheduleDateTime.HasValue
                                && p.ScheduleDateTime.Value >= targetTime.AddSeconds(-30)
                                && p.ScheduleDateTime.Value <= targetTime.AddSeconds(30)
                                && !p.IsDraft
                                )
                    .ToListAsync(cancellationToken);
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to fetch pre-publish Posts due to: {e.GetMessage()}");
            }
        }
        public async Task<List<CalendarEventDto>> GetCalendarEventsAsync(Guid userId)
        {
            try
            {
                // Load posts with their targets
                var postsFromDb = await _db.Post
                    .AsNoTracking()
                    .Include(p => p.Document)
                    .Include(p => p.Targets)
                    .Where(p => p.UserId == userId)
                    .ToListAsync();

                // Build event list from posts and their targets
                var postEvents = postsFromDb
                    .SelectMany(p =>
                        p.Targets
                            .Where(t => t.IsFacebook || t.IsInstagram) // ✅ only include true targets
                            .SelectMany(t =>
                            {
                                var events = new List<CalendarEventDto>();

                                if (t.IsFacebook)
                                {
                                    events.Add(new CalendarEventDto
                                    {
                                        Id = p.Id,
                                        DateTime = p.ScheduleDateTime.Value,
                                        Caption = p.Caption,
                                        FileUrl = p.Document?.FileUrl,
                                        Type = "post",
                                        Platform = "facebook"
                                    });
                                }

                                if (t.IsInstagram)
                                {
                                    events.Add(new CalendarEventDto
                                    {
                                        Id = p.Id,
                                        DateTime = p.ScheduleDateTime.Value,
                                        Caption = p.Caption,
                                        FileUrl = p.Document?.FileUrl,
                                        Type = "post",
                                        Platform = "instagram"
                                    });
                                }

                                return events;
                            })
                    )
                    .ToList();


                // Load matchdays for user
                #region countmatchscore
                    var scoringTypes = new List<string>();
                    var eventTypes = new List<EventTypeDTO>();
                    var sportId = _currentUser.GetSportId();

                    if (sportId.ToString() == "e0b5c3a2-39a1-4425-aebd-56f1f9e47b9f")
                    {
                        //For Basketball.
                        scoringTypes = new List<string>
                        {
                            "2-Point Field Goal Made",
                            "2-Point Field Goal Missed",
                            "3-Point Field Goal Made",
                            "3-Point Field Goal Missed",
                            "Free Throw Made",
                            "Free Throw Missed",
                            "Field Goal"
                        };
                        eventTypes = await _db.EventType
                        .Where(x => x.SportId == sportId && scoringTypes.Contains(x.Name ?? string.Empty))
                        .Select(x => new EventTypeDTO
                        {
                            Id = x.Id,
                            Name = x.Name,
                            EvenTypeName = x.EventTypeName
                        })
                        .ToListAsync();
                    }
                    else
                    {
                        //For other sports.
                        scoringTypes = new List<string>
                        {
                            "Field Goal",
                            "Goal",
                            "Point"
                        };
                        var eventType = await _db.EventType
                        .Where(x => x.SportId == sportId && scoringTypes.Contains(x.Name ?? string.Empty))
                        .Select(x => new EventTypeDTO
                        {
                            Id = x.Id,
                            Name = x.Name,
                            EvenTypeName = x.EventTypeName
                        })
                        .FirstOrDefaultAsync();
                        if (eventType != null)
                        {
                            eventTypes.Add(eventType);
                        }
                    }
                #endregion


                var matchdays = await _db.Matchday
                    .AsNoTracking()
                    .Where(m => m.UserId == userId)
                    .Include(m => m.OrganizerClub).ThenInclude(c => c.Document)
                    .Include(m => m.OpponentClub).ThenInclude(c => c.Document)
                    .ToListAsync();
                var matchdayEvents = matchdays.Select(m => new CalendarEventDto
                {

                    Id = m.Id,
                    DateTime = m.MatchDayDateTime,
                    Type = "matchday",
                    Platform = null,
                    OrganizerClubName = m.OrganizerClub.FullName,
                    OrganizerClubFileUrl = m.OrganizerClub.Document.FileUrl,
                    OrganizerClubFinalScore = CountMatchScore(m.MatchSummary, m.Id, m.OrganizerClubId, eventTypes),
                    OpponentClubName = m.OpponentClub.FullName,
                    OpponentClubFileUrl = m.OpponentClub.Document.FileUrl,
                    OpponentClubFinalScore = CountMatchScore(m.MatchSummary, m.Id, m.OpponentClubId, eventTypes),
                    Venue = (int)m.Venue
                }).ToList();

                // Merge & sort
                return postEvents
                    .Concat(matchdayEvents)
                    .OrderBy(e => e.DateTime)
                    .ToList();
            }
            catch (Exception e)
            {
                throw new CustomException(
                    $"Unable to fetch the Calendar Event due to: {e.GetBaseException().Message}"
                );
            }
        }
        public async Task<List<UpcomingPostDto>> GetUpcomingPostsAsync(Guid userId)
        {
            try
            {
                var upcomingPosts = await _db.Post.Include(x => x.Document)
                    .Where(p => p.ScheduleDateTime >= DateTime.UtcNow && p.UserId == userId && !p.IsDraft)
                    .OrderBy(p => p.ScheduleDateTime).ToListAsync();

                var postDto = _mapper.Map<List<UpcomingPostDto>>(upcomingPosts);

                return postDto;

            }
            catch (Exception e)
            {
                throw new CustomException(
                    $"Unable to fetch the Upcoming Post due to: {e.GetBaseException().Message}"
                );
            }
        }
        public async Task<List<DraftPost>> GetDraftPostsAsync(Guid userId)
        {
            try
            {
                var draftPost = await _db.Post.Include(x => x.Document)
                    .Where(p =>   p.UserId == userId && p.IsDraft && !p.IsDeleted)
                    .OrderBy(p => p.ScheduleDateTime).ToListAsync();

                var postDto = _mapper.Map<List<DraftPost>>(draftPost);

                return postDto;

            }
            catch (Exception e)
            {
                throw new CustomException(
                    $"Unable to fetch the Draft Post due to: {e.GetBaseException().Message}"
                );
            }
        }
        public async Task<bool> SetMatchDayAsync(Matchday matchDay)
        {
            try
            {
                if (matchDay.Id == Guid.Empty)
                {
                    matchDay.Id = Guid.NewGuid();
                    matchDay.DisappearDateTime = matchDay.MatchDayDateTime.AddHours(72);

                    await _db.Matchday.AddAsync(matchDay);
                }
                else
                {
                    var existingMatchDay = await _db.Matchday
                        .Include(x=>x.SponsorDocuments)
                        .FirstOrDefaultAsync(x => x.Id == matchDay.Id);
                    if (existingMatchDay != null) 
                    {
                        existingMatchDay.MatchdayNumber = matchDay.MatchdayNumber;
                        existingMatchDay.MatchDayDateTime = matchDay.MatchDayDateTime;
                        existingMatchDay.OrganizerClubId = matchDay.OrganizerClubId;
                        existingMatchDay.OpponentClubId = matchDay.OpponentClubId;
                        existingMatchDay.Referee = matchDay.Referee;
                        existingMatchDay.AssistantReferee1 = matchDay.AssistantReferee1;
                        existingMatchDay.AssistantReferee2 = matchDay.AssistantReferee2;
                        existingMatchDay.Location = matchDay.Location;
                        existingMatchDay.CompetitionType = matchDay.CompetitionType;
                        existingMatchDay.Venue = matchDay.Venue;
                        matchDay.DisappearDateTime = matchDay.MatchDayDateTime.AddHours(72);

                        if (matchDay.SponsorDocuments != null && matchDay.SponsorDocuments.Any())
                        {
                            await _db.SponsorDocument.Where(x => x.MatchdayId == existingMatchDay.Id).ExecuteDeleteAsync();
                            await _db.SponsorDocument.AddRangeAsync(matchDay.SponsorDocuments);
                        }
                    }
                }
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {

                throw new CustomException($"Unable to fetch the Match Day due to: {e.GetMessage()}");
            }

        }
        public async Task<List<UpcomingMatchDto>> GetUpcomingMatchsAsync(Guid userId)
        {
            try
            {
                #region countmatchscore
                var scoringTypes = new List<string>();
                var eventTypes = new List<EventTypeDTO>();
                var sportId = _currentUser.GetSportId();

                if (sportId.ToString() == "e0b5c3a2-39a1-4425-aebd-56f1f9e47b9f")
                {
                    //For Basketball.
                    scoringTypes = new List<string>
                        {
                            "2-Point Field Goal Made",
                            "2-Point Field Goal Missed",
                            "3-Point Field Goal Made",
                            "3-Point Field Goal Missed",
                            "Free Throw Made",
                            "Free Throw Missed",
                            "Field Goal"
                        };
                    eventTypes = await _db.EventType
                    .Where(x => x.SportId == sportId && scoringTypes.Contains(x.Name ?? string.Empty))
                    .Select(x => new EventTypeDTO
                    {
                        Id = x.Id,
                        Name = x.Name,
                        EvenTypeName = x.EventTypeName
                    })
                    .ToListAsync();
                }
                else
                {
                    //For other sports.
                    scoringTypes = new List<string>
                        {
                            "Field Goal",
                            "Goal",
                            "Point"
                        };
                    var eventType = await _db.EventType
                    .Where(x => x.SportId == sportId && scoringTypes.Contains(x.Name ?? string.Empty))
                    .Select(x => new EventTypeDTO
                    {
                        Id = x.Id,
                        Name = x.Name,
                        EvenTypeName = x.EventTypeName
                    })
                    .FirstOrDefaultAsync();
                    if (eventType != null)
                    {
                        eventTypes.Add(eventType);
                    }
                }
                #endregion

                var upcomingMatch = await _db.Matchday
                    .AsNoTracking()
                    .Include(x => x.OrganizerClub).ThenInclude(x => x.Document)
                    .Include(x => x.OpponentClub).ThenInclude(x => x.Document)
                    .Include(x => x.SponsorDocuments)
                    .Include(x => x.MatchSummary)
                    .Where(p =>
                        p.DisappearDateTime > DateTime.UtcNow &&   // only matches not expired
                        p.UserId == userId)
                    .OrderBy(p => p.MatchDayDateTime)
                    .ToListAsync();

                var matchs = _mapper.Map<List<UpcomingMatchDto>>(upcomingMatch);

                foreach (var match in matchs)
                {
                    var allMatchSummary = upcomingMatch.FirstOrDefault(x => x.Id == match.Id)?.MatchSummary;
                    if (allMatchSummary != null)
                    {
                        match.OrganizerClubScore = CountMatchScore(allMatchSummary, match.Id, match.OrganizerClubId, eventTypes);
                        match.OpponentClubScore = CountMatchScore(allMatchSummary, match.Id, match.OpponentClubId, eventTypes);
                    }
                }
                return matchs;
            }
            catch (Exception e)
            {
                throw new CustomException(
                    $"Unable to fetch the Upcoming Match due to: {e.GetBaseException().Message}"
                );
            }
        }
        public async Task<bool> StartMatchAsync(StartMatchRequest request)
        {
            try
            {
                var match = await _db.Matchday.FirstOrDefaultAsync(x => x.Id == request.MatchId);
                if (match == null)
                    return false;

                match.MatchStartDateTime = DateTime.UtcNow;

                await _db.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to start Match  due to: {e.GetMessage()}");
            }
        }
        public async Task<bool> AddMatchSummaryAsync(MatchSummary matchSummary)
        {
            try
            {
                await ValidateAssistAsync(matchSummary);

                if (!matchSummary.Minute.HasValue)
                {
                    throw new CustomException("Minute is required.");
                }

                if (matchSummary.Id == Guid.Empty)
                {
                    var lastMinute = await _db.MatchSummary
                        .Where(x => x.MatchdayId == matchSummary.MatchdayId)
                        .MaxAsync(x => x.Minute) ?? 0;

                    if (matchSummary.Minute < lastMinute)
                    {
                        throw new CustomException("You cannot add a match summary for an earlier minute.");
                    }

                    matchSummary.Id = Guid.NewGuid();
                    await _db.MatchSummary.AddAsync(matchSummary);
                }
                else
                {
                    var existingsummary = await _db.MatchSummary.FirstOrDefaultAsync(x => x.Id == matchSummary.Id);
                    if (existingsummary != null)
                    {
                        var lastMinute = await _db.MatchSummary
                            .Where(x => x.MatchdayId == matchSummary.MatchdayId && x.Id != matchSummary.Id)
                            .MaxAsync(x => (int?)x.Minute) ?? 0;

                        if (matchSummary.Minute < lastMinute)
                        {
                            throw new CustomException("You cannot set a match summary to an earlier minute than existing entries.");
                        }

                        existingsummary.EventTypeId = matchSummary.EventTypeId;
                        existingsummary.Minute = matchSummary.Minute;
                        existingsummary.Description = matchSummary.Description;
                        existingsummary.ClubId = matchSummary.ClubId;
                        existingsummary.MatchdayId = matchSummary.MatchdayId;
                        existingsummary.PlayerId = matchSummary.PlayerId;
                        existingsummary.AssistPlayerId = matchSummary.AssistPlayerId;
                    }
                }
                await _db.SaveChangesAsync();
                return true;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception e)
            {

                throw new CustomException($"Unable to add the Match Summary due to: {e.GetMessage()}");
            }
        }

        private async Task ValidateAssistAsync(MatchSummary matchSummary)
        {
            if (!matchSummary.AssistPlayerId.HasValue)
                return;

            // Assist only allowed for Goal events
            var eventType = await _db.EventType
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == matchSummary.EventTypeId);

            if (eventType == null || !string.Equals(eventType.EventTypeName, "Goal", StringComparison.OrdinalIgnoreCase))
            {
                throw new CustomException("An assist can only be assigned to a Goal event.");
            }

            // Assist player must be different from the scorer
            if (matchSummary.PlayerId.HasValue && matchSummary.AssistPlayerId.Value == matchSummary.PlayerId.Value)
            {
                throw new CustomException("Assist provider cannot be the same as the goal scorer.");
            }

            // Assist player must belong to the same club as the goal-scoring club
            var assistPlayer = await _db.Player
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == matchSummary.AssistPlayerId.Value);

            if (assistPlayer == null)
            {
                throw new CustomException("Assist player not found.");
            }

            if (matchSummary.ClubId.HasValue
                && assistPlayer.CurrentClubId.HasValue
                && assistPlayer.CurrentClubId.Value != matchSummary.ClubId.Value)
            {
                throw new CustomException("Assist player must belong to the same team as the goal scorer.");
            }

            // Assist player must be in the match lineup (only enforced if a lineup exists for this match)
            var lineupExists = await _db.MatchLineUp
                .AsNoTracking()
                .AnyAsync(l => l.MatchdayId == matchSummary.MatchdayId);

            if (lineupExists)
            {
                var assistInLineup = await _db.MatchLineUp
                    .AsNoTracking()
                    .AnyAsync(l => l.MatchdayId == matchSummary.MatchdayId
                                && l.PlayerId == matchSummary.AssistPlayerId.Value);

                if (!assistInLineup)
                {
                    throw new CustomException("Assist player must be part of the match squad.");
                }
            }
        }
        public async Task<List<MatchSummaryDto>> GetMatchSummaryAsync(Guid matchId)
        {
            try
            {
                var matchSummaries = await _db.MatchSummary
                    .AsNoTracking()
                    .Include(x => x.Club)
                    .Include(x => x.EventType)
                    .Include(x => x.Player)
                        .ThenInclude(p => p!.Documents)
                    .Include(x => x.AssistPlayer)
                        .ThenInclude(p => p!.Documents)
                    .Where(p => p.MatchdayId == matchId)
                    .OrderByDescending(p => p.Minute ?? int.MinValue)
                    .ThenByDescending(p => p.CreatedAt)
                    .ToListAsync();

                var matchSummaryDtos = _mapper.Map<List<MatchSummaryDto>>(matchSummaries);
                return matchSummaryDtos;
            }
            catch (Exception e)
            {
                throw new CustomException(
                    $"Unable to fetch the Match Summary due to: {e.GetBaseException().Message}"
                );
            }
        }
        public async Task<bool> SetMatchLineUpAsync(MatchLineUpRequest request)
        {
            try
            {
                if (request.PlayerIds == null || !request.PlayerIds.Any())
                    throw new CustomException("No players provided for lineup.");

                var existing = await _db.MatchLineUp
                    .Where(x => x.MatchdayId == request.MatchdayId)
                    .ToListAsync();

                if (existing.Any())
                {
                    _db.MatchLineUp.RemoveRange(existing);
                }

                var newLineup = request.PlayerIds.Select(pid => new MatchLineUp
                {
                    MatchdayId = request.MatchdayId,
                    PlayerId = pid
                }).ToList();

                await _db.MatchLineUp.AddRangeAsync(newLineup);

                var matchday = await _db.Matchday
                    .FirstOrDefaultAsync(md => md.Id == request.MatchdayId);

                if (matchday == null)
                    throw new CustomException("Matchday not found.");

                matchday.IsLineUp = true;

                _db.Matchday.Update(matchday);

                await _db.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to set the lineup due to: {e.GetMessage()}");
            }
        }
        public async Task<Dictionary<string, CategoryPlayerDto>> GetLastMatchLineUpAsync(Guid clubId)
        {
            try
            {
                Guid lastMatchdayId = await _db.Matchday
                    .Where(md =>
                        md.OrganizerClubId == clubId &&
                        _db.MatchLineUp.Any(ml => ml.MatchdayId == md.Id))
                    .OrderByDescending(md => md.MatchDayDateTime)
                    .Select(md => md.Id)
                    .FirstOrDefaultAsync();

                if (lastMatchdayId == Guid.Empty)
                    return new Dictionary<string, CategoryPlayerDto>();

                var lastMatchPlayers = await _db.MatchLineUp
                    .Include(m => m.Player)
                        .ThenInclude(p => p.PlayingPosition)
                    .Include(m => m.Player)
                        .ThenInclude(p => p.Documents)
                    .Where(m => m.MatchdayId == lastMatchdayId)
                    .Select(m => m.Player)
                    .ToListAsync();

                if (lastMatchPlayers == null || !lastMatchPlayers.Any())
                    return new Dictionary<string, CategoryPlayerDto>();

                var result = lastMatchPlayers
                    .GroupBy(p => p.PlayingPosition?.Category ?? PositionCategory.Other)
                    .OrderBy(g => (int)g.Key) // enum order
                    .ToDictionary(
                        g => g.Key.GetLocalizedEnum(), // 🌍 localized parent key
                        g => new CategoryPlayerDto
                        {
                            Count = g.Count(),
                            Players = g.Select(p => new PlayerDto
                            {
                                Id = p.Id,
                                FullName = p.FullName,
                                PlayingPosition = p.PlayingPosition?.Name,
                                PlayingPositionId = p.PlayingPosition?.Id,
                                IsCaption = p.IsCaption,
                                DateOfBirth = p.DateOfBirth,
                                StrongFoot = p.StrongFoot,
                                JerseyNumber = p.JerseyNumber,
                                Gender = p.Gender,
                                Documents = Enum.GetValues<PlayerDocumentType>()
                                    .ToDictionary(
                                        docType => docType,
                                        docType => p.Documents
                                            .FirstOrDefault(d => d.DocumentType == docType)?.FileUrl)
                            }).ToList()
                        });

                return result;
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to fetch the players due to: {e.GetMessage()}");
            }
        }
        public async Task<Dictionary<string, CategoryPlayerDto>> GetCurrentMatchLineUpAsync(Guid matchId)
        {
            try
            {
                var players = await _db.MatchLineUp
                    .Include(m => m.Player)
                        .ThenInclude(p => p.PlayingPosition)
                    .Include(m => m.Player)
                        .ThenInclude(p => p.Documents)
                    .Where(m => m.MatchdayId == matchId)
                    .Select(m => m.Player)
                    .ToListAsync();

                if (players == null || !players.Any())
                    return new Dictionary<string, CategoryPlayerDto>();

                var result = players
                    .GroupBy(p => p.PlayingPosition?.Category ?? PositionCategory.Other)
                    .OrderBy(g => (int)g.Key) // enum order
                    .ToDictionary(
                        g => g.Key.GetLocalizedEnum(), // 🌍 localized parent key
                        g => new CategoryPlayerDto
                        {
                            Count = g.Count(),
                            Players = g.Select(p => new PlayerDto
                            {
                                Id = p.Id,
                                FullName = p.FullName,
                                PlayingPosition = p.PlayingPosition?.Name,
                                PlayingPositionId = p.PlayingPosition?.Id,
                                IsCaption = p.IsCaption,
                                DateOfBirth = p.DateOfBirth,
                                StrongFoot = p.StrongFoot,
                                JerseyNumber = p.JerseyNumber,
                                Gender = p.Gender,
                                Documents = Enum.GetValues<PlayerDocumentType>()
                                    .ToDictionary(
                                        docType => docType,
                                        docType => p.Documents
                                            .FirstOrDefault(d => d.DocumentType == docType)?.FileUrl)
                            }).ToList()
                        });

                return result;
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to fetch the players due to: {e.GetMessage()}");
            }
        }
        public async Task<MatchStatsDto> GetMatchFairnessAsync(Guid matchId)
        {
            try
            {
                var match = await _db.Matchday
                    .AsNoTracking()
                    .Include(m => m.OrganizerClub)
                        .ThenInclude(c => c.Document)
                    .Include(m => m.OpponentClub)
                        .ThenInclude(c => c.Document)
                    .FirstOrDefaultAsync(m => m.Id == matchId);

                if (match == null)
                    return new MatchStatsDto();

                var eventCounts = await _db.EventType.Where(e => e.SportId == _currentUser.GetSportId())
                    .AsNoTracking()
                    .Where(e => !e.IsDeleted)
                    .Select(e => new
                    {
                        EventName = e.Name.Localize(e.NameDe),
                        OrganizerCount = _db.MatchSummary.Count(ms => ms.MatchdayId == matchId && ms.ClubId == match.OrganizerClubId && ms.EventTypeId == e.Id),
                        OpponentCount = _db.MatchSummary.Count(ms => ms.MatchdayId == matchId && ms.ClubId == match.OpponentClubId && ms.EventTypeId == e.Id)
                    })
                    .ToListAsync();

                return new MatchStatsDto
                {
                    Organizer = new ClubStatsDto
                    {
                        ClubName = match.OrganizerClub?.FullName ?? string.Empty,
                        ClubUrl = match.OrganizerClub?.Document?.FileUrl,
                        EventCounts = eventCounts.ToDictionary(e => e.EventName, e => e.OrganizerCount)
                    },
                    Opponent = new ClubStatsDto
                    {
                        ClubName = match.OpponentClub?.FullName ?? string.Empty,
                        ClubUrl = match.OpponentClub?.Document?.FileUrl,
                        EventCounts = eventCounts.ToDictionary(e => e.EventName, e => e.OpponentCount)
                    }
                };
            }
            catch (Exception ex)
            {
                throw new CustomException(
                    $"Unable to fetch match fairness stats due to: {ex.GetBaseException().Message}"
                );
            }
        }
        public async Task<List<MatchHistoryDto>> GetMatchHistoryAsync(Guid userId)
        {
            try
            {
                #region countmatchscore
                var scoringTypes = new List<string>();
                var eventTypes = new List<EventTypeDTO>();
                var sportId = _currentUser.GetSportId();

                if (sportId.ToString() == "e0b5c3a2-39a1-4425-aebd-56f1f9e47b9f")
                {
                    //For Basketball.
                    scoringTypes = new List<string>
                    {
                        "2-Point Field Goal Made",
                        "2-Point Field Goal Missed",
                        "3-Point Field Goal Made",
                        "3-Point Field Goal Missed",
                        "Free Throw Made",
                        "Free Throw Missed",
                        "Field Goal"
                    };
                    eventTypes = await _db.EventType
                    .Where(x => x.SportId == sportId && scoringTypes.Contains(x.Name ?? string.Empty))
                    .Select(x => new EventTypeDTO
                    {
                        Id = x.Id,
                        Name = x.Name,
                        EvenTypeName = x.EventTypeName
                    })
                    .ToListAsync();
                }
                else
                {
                    //For other sports.
                    scoringTypes = new List<string>
                    {
                        "Field Goal",
                        "Goal",
                        "Point"
                    };
                    var eventType = await _db.EventType
                    .Where(x => x.SportId == sportId && scoringTypes.Contains(x.Name ?? string.Empty))
                    .Select(x => new EventTypeDTO
                    {
                        Id = x.Id,
                        Name = x.Name,
                        EvenTypeName = x.EventTypeName
                    })
                    .FirstOrDefaultAsync();
                    if (eventType != null)
                    {
                        eventTypes.Add(eventType);
                    }
                }
                #endregion

                var matchs = await _db.Matchday
                .AsNoTracking()
                .Where(p => p.UserId == userId && p.DisappearDateTime < DateTime.UtcNow)
                .OrderBy(p => p.MatchDayDateTime)
                .Include(x=>x.OrganizerClub).ThenInclude(x=>x.Document)
                .Include(x=>x.OpponentClub).ThenInclude(x=>x.Document)
                .Include(x=>x.MatchSummary)
                .ToListAsync();

                var result = matchs.Select(x => new MatchHistoryDto
                {
                    Id = x.Id,
                    MatchdayNumber = x.MatchdayNumber,
                    MatchDayDateTime = x.MatchDayDateTime,
                    Referee = x.Referee,
                    AssistantReferee1 = x.AssistantReferee1,
                    AssistantReferee2 = x.AssistantReferee2,
                    Location = x.Location,

                    OrganizerClubId = x.OrganizerClubId,
                    OpponentClubId = x.OpponentClubId,

                    OrganizerClubName = x.OrganizerClub.FullName,
                    OpponentClubName = x.OpponentClub.FullName,

                    OrganizerClubUrl = x.OrganizerClub.Document.FileUrl,
                    OpponentClubUrl = x.OpponentClub.Document.FileUrl,

                    OrganizerGoal = CountMatchScore(x.MatchSummary,x.Id,x.OrganizerClubId,eventTypes),

                    OpponentGoal = CountMatchScore(x.MatchSummary,x.Id,x.OpponentClubId,eventTypes),

                    MatchStartDateTime = x.MatchStartDateTime,
                    IsLineUp = x.IsLineUp
                }).ToList();
                return result;
            }
            catch (Exception e)
            {
                throw new CustomException(
                    $"Unable to fetch the  Match History due to: {e.GetBaseException().Message}"
                );
            }
        }

        public async Task<List<Player>> GetLinePlayerAsync(List<Guid> PlayerIds)
        {
            try
            {
                var players = await _db.Player
                .Include(p => p.PlayingPosition)
                .Where(p => PlayerIds.Contains(p.Id))
                .ToListAsync();

                return players;

            }
            catch (Exception e)
            {
                throw new CustomException(
                    $"Unable to fetch the LineUp Player due to: {e.GetBaseException().Message}"
                );
            }
        }

        public async Task<Sport> GetSportAsync(Guid sportId)
        {
            try
            {
                var sport = await _db.Sport
               .AsNoTracking()
               .FirstOrDefaultAsync(s => s.Id == sportId);

                if (sport == null)
                {
                    throw new CustomException($"Sport with id '{sportId}' was not found.");
                }

                return sport;

            }
            catch (Exception e)
            {
                throw new CustomException(
                    $"Unable to fetch the sport due to: {e.GetBaseException().Message}"
                );
            }
        }

        public async Task<bool> DeleteMatchSummaryAsync(Guid summaryId)
        {
            try
            {
                var IsDelete = await _db.MatchSummary
                    .Where(t => t.Id == summaryId)
                    .ExecuteDeleteAsync();
                if (IsDelete > 0)
                {
                    return true;
                }
                throw new CustomException($"No summary found.");
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to process the Summary: {e.GetMessage()}");
            }
        }

        public async Task<bool> DeleteMatchDayAsync(Guid matchId)
        {
            try
            {
                var IsDelete = await _db.Matchday
                    .Where(t => t.Id == matchId)
                    .ExecuteDeleteAsync();
                if (IsDelete > 0)
                {
                    return true;
                }
                throw new CustomException($"No Match found.");
            }
            catch (Exception e)
            {
                throw new CustomException($"Unable to process the Match day: {e.GetMessage()}");
            }
        }

        public async Task<Post> GetPostByIdAsync(Guid postId)
        {
            try
            {
                var sport = await _db.Post
               .AsNoTracking()
               .Include(x=>x.Targets)
               .Include(x=>x.Document)
               .FirstOrDefaultAsync(s => s.Id == postId);

                if (sport == null)
                {
                    throw new CustomException($"Post with id '{postId}' was not found.");
                }

                return sport;

            }
            catch (Exception e)
            {
                throw new CustomException(
                    $"Unable to fetch the post due to: {e.GetBaseException().Message}"
                );
            }
        }

        public async Task<bool> EndMatchAsync(Guid matchId)
        {
            try
            {
                await _db.Matchday
                    .Where(m => m.Id == matchId)
                    .ExecuteUpdateAsync(m => m
                        .SetProperty(md => md.IsMatchEnd, true)
                        .SetProperty(md => md.DisappearDateTime, DateTime.UtcNow.AddMinutes(-1))
                    );
                return true;
            }
            catch (Exception e)
            {
                throw new CustomException(
                    $"Unable to end the match due to: {e.GetBaseException().Message}"
                );
            }
        }

        private static int CountMatchScore(ICollection<MatchSummary>? matchSummaries, Guid matchId, Guid? ClubId, List<EventTypeDTO> eventTypes)
        {
            int score = 0;
            if (matchSummaries == null) return score;
            var clubMatchSummaries = matchSummaries.Where(x => x.ClubId == ClubId && x.MatchdayId == matchId).ToList();

            foreach (var summary in clubMatchSummaries)
            {
                var eventType = eventTypes.FirstOrDefault(x => x.Id == summary.EventTypeId);
                if (eventType != null)
                {
                    switch (eventType.Name)
                    {
                        case "2-Point Field Goal Made":
                            score += 2;
                            break;
                        case "3-Point Field Goal Made":
                            score += 3;
                            break;
                        case "Free Throw Made":
                            score += 1;
                            break;
                        case "Field Goal":
                            score += 1;
                            break;
                        case "Goal":
                            score += 1;
                            break;
                        case "Point":
                            score += 1;
                            break;
                    }
                }
            }
            return score;
        }

        public async Task<(string?, string?)> GetInstagramLoginAsync(Guid userId)
        {
            try
            {
                var user = await _db.Users
                    .AsNoTracking()
                    .Select(u => new
                    {
                        u.Id,
                        u.InstagramLongLivedToken,
                        u.InstagramBusinessId
                    })
                    .FirstOrDefaultAsync(u => u.Id == userId);
                if (user != null)
                {
                    return (user.InstagramLongLivedToken, user.InstagramBusinessId);
                }
                return (null, null);
            }
            catch (Exception)
            {
               return (null, null);
            }
        }

       
    }
}
