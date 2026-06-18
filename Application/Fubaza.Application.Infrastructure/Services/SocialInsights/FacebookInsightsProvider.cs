using Fubaza.Application.Core.Interfaces.Services;
using Fubaza.Application.DTO.Enums;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Fubaza.Application.Infrastructure.Services.SocialInsights
{
    public class FacebookInsightsProvider : ISocialInsightsProvider
    {
        private const string GraphApiVersion = "v19.0";

        private readonly HttpClient _httpClient;
        private readonly ILogger<FacebookInsightsProvider> _logger;

        public FacebookInsightsProvider(HttpClient httpClient, ILogger<FacebookInsightsProvider> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public SocialPlatform Platform => SocialPlatform.Facebook;

        public async Task<InsightFetchResult> FetchAsync(InsightFetchInput input, CancellationToken ct = default)
        {
            // 1) Likes + Shares über basic fields
            var basicUrl =
                $"https://graph.facebook.com/{GraphApiVersion}/{input.ExternalPostId}" +
                $"?fields=likes.summary(true),shares" +
                $"&access_token={Uri.EscapeDataString(input.AccessToken)}";

            int likes = 0;
            int shares = 0;
            string? basicRaw = null;

            try
            {
                var basicResp = await _httpClient.GetAsync(basicUrl, ct);
                basicRaw = await basicResp.Content.ReadAsStringAsync(ct);

                if (!basicResp.IsSuccessStatusCode)
                {
                    return InsightFetchResult.Fail(
                        $"FB basic fetch failed: {(int)basicResp.StatusCode} — {basicRaw}",
                        basicRaw);
                }

                var basicJson = JObject.Parse(basicRaw);
                likes = basicJson["likes"]?["summary"]?["total_count"]?.Value<int?>() ?? 0;
                shares = basicJson["shares"]?["count"]?.Value<int?>() ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "FB basic insights call threw for post {PostId}", input.ExternalPostId);
                return InsightFetchResult.Fail($"FB basic fetch exception: {ex.Message}", basicRaw);
            }

            // 2) Impressions über /insights — eigene Permission (read_insights). Soft-Fail.
            int impressions = 0;
            string? insightsRaw = null;
            try
            {
                var insightsUrl =
                    $"https://graph.facebook.com/{GraphApiVersion}/{input.ExternalPostId}/insights/post_impressions" +
                    $"?access_token={Uri.EscapeDataString(input.AccessToken)}";

                var insightsResp = await _httpClient.GetAsync(insightsUrl, ct);
                insightsRaw = await insightsResp.Content.ReadAsStringAsync(ct);

                if (insightsResp.IsSuccessStatusCode)
                {
                    var insightsJson = JObject.Parse(insightsRaw);
                    impressions = insightsJson["data"]?
                        .FirstOrDefault()?["values"]?
                        .FirstOrDefault()?["value"]?
                        .Value<int?>() ?? 0;
                }
                else
                {
                    _logger.LogWarning(
                        "FB impressions fetch returned {Status} for post {PostId} — fehlt 'read_insights'? Response: {Body}",
                        (int)insightsResp.StatusCode, input.ExternalPostId, insightsRaw);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "FB impressions call threw for post {PostId}", input.ExternalPostId);
            }

            return InsightFetchResult.Ok(likes, impressions, shares, raw: $"basic={basicRaw};insights={insightsRaw}");
        }
    }
}
