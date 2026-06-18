using Fubaza.Application.Core.Entities;
using Fubaza.Application.DTO.DTO;

using Fubaza.Application.DTO.Services;


namespace Fubaza.Application.Core.Interfaces.Repositories
{
    public interface IEventRepository
    {
        Task<Post> SchedulePostAsync(Post post);
        Task<bool> CancelPostAsync(Guid postId);
        Task<bool> UpdatePostCancleStatusAsync(Guid postId);
        Task<Post> GetPostByIdAsync(Guid Id);
        Task<List<Post>> GetDueScheduledPostsAsync(DateTime utcNow);
        Task<List<Post>> GetPostsToNotifyBeforeAsync(DateTime utcNow, CancellationToken cancellationToken = default);
        Task UpdatePostIdAsync(PostTarget target);
        Task<bool> DeleteDraftPostAsync(Guid postId);
        Task<List<CalendarEventDto>> GetCalendarEventsAsync(Guid userId);
        Task<List<UpcomingPostDto>> GetUpcomingPostsAsync(Guid userId);
        Task<List<DraftPost>> GetDraftPostsAsync(Guid userId);
        Task<bool> SetMatchDayAsync(Matchday matchDay);
        Task<bool> EndMatchAsync(Guid matchId);
        Task<bool> DeleteMatchDayAsync(Guid matchId);
        Task<List<UpcomingMatchDto>> GetUpcomingMatchsAsync(Guid userId);
        Task<bool> StartMatchAsync(StartMatchRequest request);
        Task<bool> AddMatchSummaryAsync(MatchSummary matchSummary);
        Task<bool> DeleteMatchSummaryAsync(Guid summaryId);
        Task<List<MatchSummaryDto>> GetMatchSummaryAsync(Guid matchId);
        Task<bool> SetMatchLineUpAsync(MatchLineUpRequest request);
        Task<Dictionary<string, CategoryPlayerDto>> GetCurrentMatchLineUpAsync(Guid matchId);
        Task<Dictionary<string, CategoryPlayerDto>> GetLastMatchLineUpAsync(Guid clubId );
        Task<MatchStatsDto> GetMatchFairnessAsync(Guid matchId);
        Task<List<MatchHistoryDto>> GetMatchHistoryAsync(Guid userId);
        Task<List<Player>> GetLinePlayerAsync(List<Guid> PlayerIds);
        Task<Sport> GetSportAsync(Guid sportId);
        Task<(string?, string?)> GetInstagramLoginAsync(Guid userId);

    }
}
