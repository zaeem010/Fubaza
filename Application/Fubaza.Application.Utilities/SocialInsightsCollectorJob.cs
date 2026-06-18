using Fubaza.Application.Core.Entities;
using Fubaza.Application.Core.Interfaces.Repositories;
using Fubaza.Application.Core.Interfaces.Services;
using Fubaza.Application.DTO.Enums;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace Fubaza.Application.Utilities
{
    // Hangfire Recurring Job — läuft täglich (cron via Registrierung).
    // Holt für jeden über Fubaza publizierten Post die aktuellen Insights von der Meta Graph API
    // und schreibt einen append-only Snapshot in PostInsightSnapshot.
    public class SocialInsightsCollectorJob
    {
        public const string JobId = "social-insights-collector";

        private readonly IPostInsightRepository _repo;
        private readonly IEnumerable<ISocialInsightsProvider> _providers;
        private readonly ILogger<SocialInsightsCollectorJob> _logger;

        public SocialInsightsCollectorJob(
            IPostInsightRepository repo,
            IEnumerable<ISocialInsightsProvider> providers,
            ILogger<SocialInsightsCollectorJob> logger)
        {
            _repo = repo;
            _providers = providers;
            _logger = logger;
        }

        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        [AutomaticRetry(Attempts = 0)]
        public async Task RunAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("▶️ SocialInsightsCollectorJob START");

            var providersByPlatform = _providers.ToDictionary(p => p.Platform);
            var targets = await _repo.GetActiveTargetsForCollectionAsync(ct);

            _logger.LogInformation("   ↳ {Count} targets to collect", targets.Count);

            int ok = 0;
            int skipped = 0;
            int failed = 0;

            foreach (var t in targets)
            {
                if (ct.IsCancellationRequested) break;

                if (!providersByPlatform.TryGetValue(t.Platform, out var provider))
                {
                    skipped++;
                    continue;
                }

                try
                {
                    var result = await provider.FetchAsync(
                        new InsightFetchInput
                        {
                            ExternalPostId = t.ExternalPostId,
                            AccessToken = t.AccessToken,
                        },
                        ct);

                    if (!result.Success)
                    {
                        failed++;
                        _logger.LogWarning(
                            "Insight fetch failed — target={TargetId} platform={Platform} err={Err}",
                            t.PostTargetId, t.Platform, result.ErrorMessage);
                        continue;
                    }

                    await _repo.AddSnapshotAsync(new PostInsightSnapshot
                    {
                        PostTargetId = t.PostTargetId,
                        Platform = t.Platform,
                        ExternalPostId = t.ExternalPostId,
                        Likes = result.Likes,
                        Impressions = result.Impressions,
                        Shares = result.Shares,
                        FetchedAt = DateTime.UtcNow,
                        RawResponse = result.RawResponse,
                    }, ct);

                    ok++;
                }
                catch (Exception ex)
                {
                    failed++;
                    _logger.LogError(ex,
                        "Insight fetch exception — target={TargetId} platform={Platform}",
                        t.PostTargetId, t.Platform);
                }
            }

            _logger.LogInformation(
                "✅ SocialInsightsCollectorJob DONE — ok={Ok} failed={Failed} skipped={Skipped}",
                ok, failed, skipped);
        }
    }
}
