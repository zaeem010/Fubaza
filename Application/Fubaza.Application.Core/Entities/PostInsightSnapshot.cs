using Fubaza.Application.DTO.Enums;

namespace Fubaza.Application.Core.Entities
{
    public class PostInsightSnapshot
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid PostTargetId { get; set; }
        public PostTarget? PostTarget { get; set; }

        public SocialPlatform Platform { get; set; }

        // Gespiegelte externe Post-ID (FacebookPostId / InstagramPostId).
        // Vereinfacht Lookups, falls PostTarget später entkoppelt werden soll.
        public string ExternalPostId { get; set; } = string.Empty;

        public int Likes { get; set; }
        public int Impressions { get; set; }
        public int Shares { get; set; }

        public DateTime FetchedAt { get; set; }

        // Optional: rohe Graph-API-Response für Debug. nvarchar(max).
        public string? RawResponse { get; set; }
    }
}
