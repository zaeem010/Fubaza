using Fubaza.Application.Core.Entities;
using Fubaza.Application.DTO.Enums;

namespace Fubaza.Application.Core.Interfaces.Repositories
{
    public interface IPostInsightRepository
    {
        // Liefert alle PostTargets, die durch Fubaza publiziert wurden und für die
        // wir eine externe Post-ID kennen. Pro Plattform genau ein Record (entweder
        // Facebook oder Instagram, abhängig davon welches Feld gesetzt ist).
        Task<List<PostInsightCollectionTarget>> GetActiveTargetsForCollectionAsync(CancellationToken ct = default);

        Task AddSnapshotAsync(PostInsightSnapshot snapshot, CancellationToken ct = default);

        // Aggregiert pro Plattform den jüngsten Snapshot je PostTarget und summiert.
        Task<List<PlatformAggregateResult>> GetLatestAggregatedByUserAsync(Guid userId, CancellationToken ct = default);
    }

    // Read-Model für den Collector Job — alles, was der ISocialInsightsProvider braucht.
    public sealed class PostInsightCollectionTarget
    {
        public Guid PostTargetId { get; set; }
        public SocialPlatform Platform { get; set; }
        public string ExternalPostId { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
    }

    // Read-Model für die Dashboard-Aggregation.
    public sealed class PlatformAggregateResult
    {
        public SocialPlatform Platform { get; set; }
        public int TotalLikes { get; set; }
        public int TotalImpressions { get; set; }
        public int TotalShares { get; set; }
    }
}
