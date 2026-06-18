using Fubaza.Application.DTO.Enums;

namespace Fubaza.Application.Core.Interfaces.Services
{
    public interface ISocialInsightsProvider
    {
        SocialPlatform Platform { get; }

        Task<InsightFetchResult> FetchAsync(InsightFetchInput input, CancellationToken ct = default);
    }

    public sealed class InsightFetchInput
    {
        public string ExternalPostId { get; init; } = string.Empty;
        public string AccessToken { get; init; } = string.Empty;
    }

    public sealed class InsightFetchResult
    {
        public bool Success { get; init; }
        public int Likes { get; init; }
        public int Impressions { get; init; }
        public int Shares { get; init; }
        public string? ErrorMessage { get; init; }
        public string? RawResponse { get; init; }

        public static InsightFetchResult Ok(int likes, int impressions, int shares, string? raw = null) =>
            new() { Success = true, Likes = likes, Impressions = impressions, Shares = shares, RawResponse = raw };

        public static InsightFetchResult Fail(string error, string? raw = null) =>
            new() { Success = false, ErrorMessage = error, RawResponse = raw };
    }
}
