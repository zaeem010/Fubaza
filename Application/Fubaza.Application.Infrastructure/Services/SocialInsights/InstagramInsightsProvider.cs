using Fubaza.Application.Core.Interfaces.Services;
using Fubaza.Application.DTO.Enums;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Fubaza.Application.Infrastructure.Services.SocialInsights
{
    // Insights für IG Business Accounts, die über die FB Page verbunden sind.
    // Direct-Login-IG (Posts ohne PostTarget, siehe HangfireService.PublishToInstagramLoginAsync)
    // wird in dieser Iteration nicht unterstützt — separate Folge-Story.
    public class InstagramInsightsProvider : ISocialInsightsProvider
    {
        private const string GraphApiVersion = "v19.0";

        private readonly HttpClient _httpClient;
        private readonly ILogger<InstagramInsightsProvider> _logger;

        public InstagramInsightsProvider(HttpClient httpClient, ILogger<InstagramInsightsProvider> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public SocialPlatform Platform => SocialPlatform.Instagram;

        public async Task<InsightFetchResult> FetchAsync(InsightFetchInput input, CancellationToken ct = default)
        {
            // 1) like_count direkt am Media-Object
            int likes = 0;
            string? basicRaw = null;

            try
            {
                var basicUrl =
                    $"https://graph.facebook.com/{GraphApiVersion}/{input.ExternalPostId}" +
                    $"?fields=like_count" +
                    $"&access_token={Uri.EscapeDataString(input.AccessToken)}";

                var basicResp = await _httpClient.GetAsync(basicUrl, ct);
                basicRaw = await basicResp.Content.ReadAsStringAsync(ct);

                if (!basicResp.IsSuccessStatusCode)
                {
                    return InsightFetchResult.Fail(
                        $"IG basic fetch failed: {(int)basicResp.StatusCode} — {basicRaw}",
                        basicRaw);
                }

                var basicJson = JObject.Parse(basicRaw);
                likes = basicJson["like_count"]?.Value<int?>() ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "IG basic insights call threw for media {MediaId}", input.ExternalPostId);
                return InsightFetchResult.Fail($"IG basic fetch exception: {ex.Message}", basicRaw);
            }

            // 2) impressions + shares über /insights — eigene Permission (instagram_manage_insights). Soft-Fail.
            int impressions = 0;
            int shares = 0;
            string? insightsRaw = null;

            try
            {
                var insightsUrl =
                    $"https://graph.facebook.com/{GraphApiVersion}/{input.ExternalPostId}/insights" +
                    $"?metric=impressions,shares" +
                    $"&access_token={Uri.EscapeDataString(input.AccessToken)}";

                var insightsResp = await _httpClient.GetAsync(insightsUrl, ct);
                insightsRaw = await insightsResp.Content.ReadAsStringAsync(ct);

                if (insightsResp.IsSuccessStatusCode)
                {
                    var insightsJson = JObject.Parse(insightsRaw);
                    var data = insightsJson["data"] as JArray;
                    if (data != null)
                    {
                        foreach (var metric in data)
                        {
                            var name = metric["name"]?.ToString();
                            var value = metric["values"]?.FirstOrDefault()?["value"]?.Value<int?>() ?? 0;
                            if (name == "impressions") impressions = value;
                            else if (name == "shares") shares = value;
                        }
                    }
                }
                else
                {
                    _logger.LogWarning(
                        "IG insights fetch returned {Status} for media {MediaId} — fehlt 'instagram_manage_insights'? Response: {Body}",
                        (int)insightsResp.StatusCode, input.ExternalPostId, insightsRaw);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "IG insights call threw for media {MediaId}", input.ExternalPostId);
            }

            return InsightFetchResult.Ok(likes, impressions, shares, raw: $"basic={basicRaw};insights={insightsRaw}");
        }
    }
}
