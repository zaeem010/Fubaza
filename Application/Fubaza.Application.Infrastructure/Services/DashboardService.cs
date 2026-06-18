using Fubaza.Application.Core.Contracts;
using Fubaza.Application.Core.Exceptions;
using Fubaza.Application.Core.Extensions;
using Fubaza.Application.Core.Interfaces.Repositories;
using Fubaza.Application.Core.Interfaces.Services;
using Fubaza.Application.Core.Wrapper;
using Fubaza.Application.DTO.DTO;
using Fubaza.Application.DTO.Enums;
using Microsoft.Extensions.Logging;

namespace Fubaza.Application.Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        // Plattformen, die im Dashboard immer auftauchen — auch wenn der User dort
        // (noch) keine Posts hat. Fehlende werden mit 0/0/0 aufgefüllt.
        private static readonly SocialPlatform[] DisplayedPlatforms =
        {
            SocialPlatform.Facebook,
            SocialPlatform.Instagram,
        };

        private readonly IPostInsightRepository _repo;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(IPostInsightRepository repo, ILogger<DashboardService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<IResult<PlayerDashboardSocialStatsDto>> GetSocialStatsAsync(Guid userId)
        {
            try
            {
                var aggregates = await _repo.GetLatestAggregatedByUserAsync(userId);
                var byPlatform = aggregates.ToDictionary(a => a.Platform);

                var dto = new PlayerDashboardSocialStatsDto
                {
                    Platforms = DisplayedPlatforms.Select(p =>
                    {
                        if (byPlatform.TryGetValue(p, out var agg))
                        {
                            return new PlatformSocialStatsDto
                            {
                                Platform = p.ToString(),
                                TotalLikes = agg.TotalLikes,
                                TotalImpressions = agg.TotalImpressions,
                                TotalShares = agg.TotalShares,
                            };
                        }
                        return new PlatformSocialStatsDto { Platform = p.ToString() };
                    }).ToList(),
                };

                return Result<PlayerDashboardSocialStatsDto>.Success(dto);
            }
            catch (CustomException e)
            {
                _logger.LogError(e, "GetSocialStatsAsync failed for user {UserId}", userId);
                return Result<PlayerDashboardSocialStatsDto>.Fail(e.GetMessage());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetSocialStatsAsync unexpected error for user {UserId}", userId);
                return Result<PlayerDashboardSocialStatsDto>.Fail(e.GetMessage());
            }
        }
    }
}
