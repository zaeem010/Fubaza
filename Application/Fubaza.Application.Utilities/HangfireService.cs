using Fubaza.Application.Core.Contracts.Services;
using Fubaza.Application.Core.Entities;
using Fubaza.Application.Core.Interfaces.Repositories;
using Fubaza.Application.Core.Settings;
using Fubaza.Application.DTO.DTO;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.X509;
using System.Linq.Expressions;
using System.Net.Http.Headers;

namespace Fubaza.Application.Utilities
{
	public class HangfireService : IJobService
	{
        private readonly ILogger<HangfireService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IEventRepository _eventRepository;
        public HangfireService(ILogger<HangfireService> logger,
            HttpClient httpClient, IEventRepository eventRepository
            )
        {
            _logger = logger;
            _httpClient = httpClient;
            _eventRepository = eventRepository;
        }
        public string Enqueue(Expression<Func<Task>> methodCall)
		{
			return BackgroundJob.Enqueue(methodCall);
		}
        public async Task PublishToFacebookAsyncV3(string Id)
        {
            try
            {
                _logger.LogInformation($"▶️ PublishToFacebookAsyncV3 START for post {Id}");
                var post = await _eventRepository.GetPostByIdAsync(Guid.Parse(Id));
                if (!post.IsCancelled)
                {
                    var fbTargets = post.Targets.Where(x => x.IsFacebook).ToList();
                    _logger.LogInformation($"   ↳ Post {Id} has {fbTargets.Count} Facebook target(s)");

                    if (fbTargets.Count == 0)
                    {
                        _logger.LogWarning($"⚠️ PublishToFacebookAsyncV3 skipped — no Facebook targets on post {Id}");
                    }

                    foreach (var target in fbTargets)
                    {
                        if (!string.IsNullOrEmpty(target.PageId) || !string.IsNullOrEmpty(target.AccessToken))
                        {
                            var imageUrl = post.Document?.FileUrl;
                            var url = $"https://graph.facebook.com/v19.0/{target.PageId}/photos";

                            var parameters = new Dictionary<string, string>
                            {
                                { "url", imageUrl ?? string.Empty },
                                { "caption", post.Caption ?? string.Empty },
                                { "published", "true" },
                                { "access_token", target.AccessToken ?? string.Empty }
                            };

                            _logger.LogInformation($"📤 Facebook publish attempt for post {Id} | Page {target.PageId} | imageUrl={imageUrl} | tokenLen={(target.AccessToken?.Length ?? 0)}");

                            var response = await _httpClient.PostAsync(url, new FormUrlEncodedContent(parameters));
                            var responseText = await response.Content.ReadAsStringAsync();

                            if (response.IsSuccessStatusCode)
                            {
                                var json = JObject.Parse(responseText);
                                // /{page-id}/photos liefert ZWEI IDs zurück:
                                //   - "id"      = Photo Object ID  (kein 'shares'-Feld → für Insights unbrauchbar)
                                //   - "post_id" = Wall Post ID     (Format: "<page-id>_<post-id>")
                                // Wir wollen die Post-ID, weil die Insights-API auf Post-Ebene operiert.
                                var fbPostId = json["post_id"]?.ToString() ?? json["id"]?.ToString();

                                if (!string.IsNullOrEmpty(fbPostId))
                                {
                                    target.FacebookPostId = fbPostId;
                                    _logger.LogInformation($"✅ Facebook post published successfully for Page {target.PageId} - Post ID: {fbPostId}");
                                }
                                target.IsPublished = true;
                                await _eventRepository.UpdatePostIdAsync(target);
                            }
                            else
                            {
                                _logger.LogError($"❌ Facebook publish FAILED for post {Id} | Page {target.PageId} | Status={(int)response.StatusCode} {response.StatusCode} | Response={responseText}");
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"⚠️ FB target skipped for post {Id} — both PageId and AccessToken are empty");
                        }
                    }
                }
                else
                {
                    _logger.LogInformation($"🚫 Post {Id} was cancelled — updating cancel status, not publishing");
                    await _eventRepository.UpdatePostCancleStatusAsync(post.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"💥 PublishToFacebookAsyncV3 exception for post {Id}: {ex.Message}");
                throw;
            }
        }

        public async Task PublishToInstagramAsync(string Id)
        {
            try
            {
                _logger.LogInformation($"▶️ PublishToInstagramAsync (FB-login) START for post {Id}");
                var post = await _eventRepository.GetPostByIdAsync(Guid.Parse(Id));
                if (!post.IsCancelled)
                {
                    var igTargets = post.Targets.Where(x => x.IsFacebook).ToList();
                    _logger.LogInformation($"   ↳ Post {Id} has {igTargets.Count} target(s) eligible for IG (FB-login)");

                    if (igTargets.Count == 0)
                    {
                        _logger.LogWarning($"⚠️ PublishToInstagramAsync skipped — no eligible targets on post {Id}");
                    }

                    foreach (var target in igTargets)
                    {
                        if (!string.IsNullOrEmpty(target.InstagramBusinessId) || !string.IsNullOrEmpty(target.AccessToken))
                        {
                            var imageUrl = post.Document?.FileUrl;
                            var createUrl = $"https://graph.facebook.com/v19.0/{target.InstagramBusinessId}/media";

                            var payload = new Dictionary<string, string>
                            {
                                { "image_url", imageUrl ?? string.Empty },
                                { "caption", post.Caption ?? string.Empty },
                                { "access_token", target.AccessToken ?? string.Empty }
                            };

                            _logger.LogInformation($"📤 Instagram (FB-login) publish attempt for post {Id} | IG {target.InstagramBusinessId} | imageUrl={imageUrl} | tokenLen={(target.AccessToken?.Length ?? 0)}");

                            // Step 1️⃣: Create media container
                            var createResponse = await _httpClient.PostAsync(createUrl, new FormUrlEncodedContent(payload));
                            var createText = await createResponse.Content.ReadAsStringAsync();

                            if (createResponse.IsSuccessStatusCode)
                            {
                                var json = JObject.Parse(createText);
                                var creationId = json["id"]?.ToString();

                                if (!string.IsNullOrEmpty(creationId))
                                {
                                    // Step 2️⃣: Publish the media
                                    var publishUrl = $"https://graph.facebook.com/v19.0/{target.InstagramBusinessId}/media_publish";
                                    var publishPayload = new Dictionary<string, string>
                                    {
                                        { "creation_id", creationId },
                                        { "access_token", target.AccessToken ?? string.Empty }
                                    };

                                    var publishResponse = await _httpClient.PostAsync(publishUrl, new FormUrlEncodedContent(publishPayload));
                                    var publishText = await publishResponse.Content.ReadAsStringAsync();

                                    if (publishResponse.IsSuccessStatusCode)
                                    {
                                        var publishJson = JObject.Parse(publishText);
                                        var igMediaId = publishJson["id"]?.ToString();
                                        if (!string.IsNullOrEmpty(igMediaId))
                                        {
                                            target.InstagramPostId = igMediaId;
                                        }
                                        target.IsPublished = true;
                                        await _eventRepository.UpdatePostIdAsync(target);
                                        _logger.LogInformation($"✅ Instagram post published successfully for IG {target.InstagramBusinessId} - Media ID: {igMediaId ?? creationId}");
                                    }
                                    else
                                    {
                                        _logger.LogError($"❌ Instagram publish FAILED for post {Id} | IG {target.InstagramBusinessId} | Status={(int)publishResponse.StatusCode} {publishResponse.StatusCode} | Response={publishText}");
                                    }
                                }
                                else
                                {
                                    _logger.LogError($"❌ Instagram media-create returned no creationId for post {Id} | IG {target.InstagramBusinessId} | Response={createText}");
                                }
                            }
                            else
                            {
                                _logger.LogError($"❌ Instagram media-create FAILED for post {Id} | IG {target.InstagramBusinessId} | Status={(int)createResponse.StatusCode} {createResponse.StatusCode} | Response={createText}");
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"⚠️ IG target skipped for post {Id} — both InstagramBusinessId and AccessToken are empty");
                        }
                    }
                }
                else
                {
                    _logger.LogInformation($"🚫 Post {Id} was cancelled — updating cancel status, not publishing");
                    await _eventRepository.UpdatePostCancleStatusAsync(post.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"💥 PublishToInstagramAsync exception for post {Id}: {ex.Message}");
                throw;
            }
        }

        public async Task PublishToInstagramLoginAsync(string Id)
        {
            try
            {
                _logger.LogInformation($"▶️ PublishToInstagramLoginAsync (direct) START for post {Id}");
                var post = await _eventRepository.GetPostByIdAsync(Guid.Parse(Id));
                if (!post.IsCancelled)
                {
                    var instagramlogin = await _eventRepository.GetInstagramLoginAsync(post.UserId);
                    var hasToken = !string.IsNullOrEmpty(instagramlogin.Item1);
                    var hasIgUserId = !string.IsNullOrEmpty(instagramlogin.Item2);
                    _logger.LogInformation($"   ↳ IG-Login lookup for user {post.UserId}: hasToken={hasToken}, hasIgUserId={hasIgUserId}");

                    if (hasToken && hasIgUserId)
                    {
                        var imageUrl = post.Document?.FileUrl;
                        var createUrl = $"https://graph.instagram.com/v25.0/{instagramlogin.Item2}/media";

                        var payload = new Dictionary<string, string>
                        {
                            { "image_url", imageUrl ?? string.Empty },
                            { "caption", post.Caption ?? string.Empty }
                        };

                        _logger.LogInformation($"📤 Instagram (direct-login) publish attempt for post {Id} | IG {instagramlogin.Item2} | imageUrl={imageUrl} | tokenLen={(instagramlogin.Item1?.Length ?? 0)}");

                        // Step 1️⃣: Create media container
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", instagramlogin.Item1);
                        var createResponse = await _httpClient.PostAsync(createUrl, new FormUrlEncodedContent(payload));
                        var createText = await createResponse.Content.ReadAsStringAsync();

                        if (createResponse.IsSuccessStatusCode)
                        {
                            var json = JObject.Parse(createText);
                            var creationId = json["id"]?.ToString();

                            if (!string.IsNullOrEmpty(creationId))
                            {
                                // Step 2️⃣: Publish the media
                                var publishUrl = $"https://graph.instagram.com/v25.0/{instagramlogin.Item2}/media_publish";
                                var publishPayload = new Dictionary<string, string>
                                {
                                    { "creation_id", creationId },
                                };
                                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", instagramlogin.Item1);
                                var publishResponse = await _httpClient.PostAsync(publishUrl, new FormUrlEncodedContent(publishPayload));
                                var publishText = await publishResponse.Content.ReadAsStringAsync();

                                if (publishResponse.IsSuccessStatusCode)
                                {
                                    //target.IsPublished = true;
                                    //await _eventRepository.UpdatePostIdAsync(target);
                                    _logger.LogInformation($"✅ Instagram post published successfully for IG {instagramlogin.Item2} - Post ID: {creationId}");
                                }
                                else
                                {
                                    _logger.LogError($"❌ Instagram (direct-login) publish FAILED for post {Id} | IG {instagramlogin.Item2} | Status={(int)publishResponse.StatusCode} {publishResponse.StatusCode} | Response={publishText}");
                                }
                            }
                            else
                            {
                                _logger.LogError($"❌ Instagram media-create returned no creationId for post {Id} | IG {instagramlogin.Item2} | Response={createText}");
                            }
                        }
                        else
                        {
                            _logger.LogError($"❌ Instagram (direct-login) media-create FAILED for post {Id} | IG {instagramlogin.Item2} | Status={(int)createResponse.StatusCode} {createResponse.StatusCode} | Response={createText}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"⚠️ PublishToInstagramLoginAsync skipped for post {Id} — user {post.UserId} has no Instagram-direct credentials (hasToken={hasToken}, hasIgUserId={hasIgUserId}). Wahrscheinlich wurde der Post mit IsFacebookLogin=false eingereicht ohne dass IG-Direct verbunden ist.");
                    }
                }
                else
                {
                    _logger.LogInformation($"🚫 Post {Id} was cancelled — updating cancel status, not publishing");
                    await _eventRepository.UpdatePostCancleStatusAsync(post.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"💥 PublishToInstagramLoginAsync exception for post {Id}: {ex.Message}");
                throw;
            }
        }
    }
}
