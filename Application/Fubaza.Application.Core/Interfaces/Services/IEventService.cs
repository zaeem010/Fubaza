using Fubaza.Application.Core.Contracts;
using Fubaza.Application.Core.Entities;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Services;


namespace Fubaza.Application.Core.Interfaces.Services
{
    public interface IEventService
    {
        Task<IResult<bool>> SchedulePostAsync(Post post);
        Task<IResult<bool>> CancelPostAsync(Guid postId);
        Task<IResult<List<CalendarEventDto>>> GetCalendarEventsAsync(Guid userId);
        Task<IResult<List<UpcomingPostDto>>> GetUpcomingPostsAsync(Guid userId);
        Task<IResult<List<DraftPost>>> GetDraftPostsAsync(Guid userId);
        Task<IResult<bool>> DeleteDraftPostAsync(Guid postId);
        Task<IResult<bool>> SetMatchDayAsync(Matchday matchDay);
        Task<IResult<bool>> DeleteMatchDayAsync(Guid matchId);
        Task<IResult<List<UpcomingMatchDto>>> GetUpcomingMatchsAsync(Guid userId);
        Task<IResult<bool>> StartMatchAsync(StartMatchRequest request);
        Task<IResult<bool>> EndMatchAsync(Guid matchId);
        Task<IResult<bool>> AddMatchSummaryAsync(MatchSummary matchSummary);
        Task<IResult<bool>> DeleteMatchSummaryAsync(Guid summaryId);
        Task<IResult<List<MatchSummaryDto>>> GetMatchSummaryAsync(Guid matchId);
        Task<IResult<bool>> SetMatchLineUpAsync(MatchLineUpRequest request);
        Task<IResult<Dictionary<string, CategoryPlayerDto>>> GetLastMatchLineUpAsync(Guid clubId);
        Task<IResult<Dictionary<string, CategoryPlayerDto>>> GetCurrentMatchLineUpAsync(Guid matchId);
        Task<IResult<MatchStatsDto>> GetMatchFairnessAsync(Guid matchId);
        Task<IResult<List<MatchHistoryDto>>> GetMatchHistoryAsync(Guid userId);
    }
}
