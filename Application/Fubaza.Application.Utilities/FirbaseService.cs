using FirebaseAdmin.Messaging;

using Fubaza.Application.Core.Contracts.Services;
using Fubaza.Application.DTO.Services;

using Microsoft.Extensions.Logging;

namespace Fubaza.Application.Utilities
{
    public class FirbaseService : IFirbaseService
    {
        private readonly FirebaseMessaging _messaging;
        private readonly ILogger<FirbaseService> _logger;

        public FirbaseService(FirebaseMessaging messaging, ILogger<FirbaseService> logger)
        {
            _messaging = messaging;
            _logger = logger;
        }

        public async Task<NotificationDispatchResult> SendAsync(NotificationRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Token))
            {
                // ---------- Single token ----------
                var message = new Message
                {
                    Token = request.Token,
                    Data = PrepareData(request.Data),
                    Android = BuildAndroidConfig(request.DataOnly),
                    Apns = BuildApnsConfig(contentAvailable: request.DataOnly),
                    Webpush = BuildWebpushConfig(request.DataOnly),
                    Notification = request.DataOnly ? null : new Notification { Title = request.Title, Body = request.Body }
                };

                var id = await _messaging.SendAsync(message);
                _logger.LogInformation("FCM: Sent to token. MessageId={MessageId}", id);

                return new NotificationDispatchResult
                {
                    Mode = "token",
                    SuccessCount = 1,
                    FailureCount = 0,
                    MessageId = id
                };
            }

            if (request.Tokens != null && request.Tokens.Any())
            {
                // ---------- Multiple tokens ----------
                var tokens = request.Tokens.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct().ToList();
                if (tokens.Count == 0)
                    return new NotificationDispatchResult { Mode = "tokens", SuccessCount = 0, FailureCount = 0 };

                var message = new MulticastMessage
                {
                    Tokens = tokens,
                    Data = PrepareData(request.Data),
                    Android = BuildAndroidConfig(request.DataOnly),
                    Apns = BuildApnsConfig(contentAvailable: request.DataOnly),
                    Webpush = BuildWebpushConfig(request.DataOnly),
                    Notification = request.DataOnly ? null : new Notification { Title = request.Title, Body = request.Body }
                };

                var res = await _messaging.SendEachForMulticastAsync(message);

                var results = res.Responses.Select((r, i) => new PerTokenResult
                {
                    Index = i,
                    Token = tokens[i],
                    IsSuccess = r.IsSuccess,
                    MessageId = r.IsSuccess ? r.MessageId : null,
                    Error = r.IsSuccess ? null : r.Exception?.Message
                }).ToList();

                _logger.LogInformation("FCM: Multicast sent. Success={Success} Failure={Failure}",
                    res.SuccessCount, res.FailureCount);

                return new NotificationDispatchResult
                {
                    Mode = "tokens",
                    SuccessCount = res.SuccessCount,
                    FailureCount = res.FailureCount,
                    Results = results
                };
            }

            if (!string.IsNullOrWhiteSpace(request.Topic))
            {
                // ---------- Topic ----------
                var message = new Message
                {
                    Topic = request.Topic,
                    Data = PrepareData(request.Data),
                    Android = BuildAndroidConfig(request.DataOnly),
                    Apns = BuildApnsConfig(contentAvailable: request.DataOnly),
                    Webpush = BuildWebpushConfig(request.DataOnly),
                    Notification = request.DataOnly ? null : new Notification { Title = request.Title, Body = request.Body }
                };

                var id = await _messaging.SendAsync(message);
                _logger.LogInformation("FCM: Sent to topic '{Topic}'. MessageId={MessageId}", request.Topic, id);

                return new NotificationDispatchResult
                {
                    Mode = "topic",
                    SuccessCount = 1,
                    FailureCount = 0,
                    MessageId = id
                };
            }

            throw new ArgumentException("NotificationRequest must include Token, Tokens, or Topic.");
        }

        public async Task SubscribeAsync(TopicSubscriptionRequest request)
        {
            var tokens = (request.Tokens ?? new()).Where(t => !string.IsNullOrWhiteSpace(t)).Distinct().ToList();
            if (tokens.Count == 0 || string.IsNullOrWhiteSpace(request.Topic)) return;

            var res = await _messaging.SubscribeToTopicAsync(tokens, request.Topic);
            _logger.LogInformation("FCM: Subscribed {Success} tokens to '{Topic}'. Failures={Failures}",
                res.SuccessCount, request.Topic, res.FailureCount);
        }

        public async Task UnsubscribeAsync(TopicSubscriptionRequest request)
        {
            var tokens = (request.Tokens ?? new()).Where(t => !string.IsNullOrWhiteSpace(t)).Distinct().ToList();
            if (tokens.Count == 0 || string.IsNullOrWhiteSpace(request.Topic)) return;

            var res = await _messaging.UnsubscribeFromTopicAsync(tokens, request.Topic);
            _logger.LogInformation("FCM: Unsubscribed {Success} tokens from '{Topic}'. Failures={Failures}",
                res.SuccessCount, request.Topic, res.FailureCount);
        }

        public Task<string> SendSilentDataAsync(string token, IDictionary<string, string> data) =>
            _messaging.SendAsync(new Message
            {
                Token = token,
                Data = PrepareData(data),
                Android = BuildAndroidConfig(dataOnly: true),
                Apns = BuildApnsConfig(contentAvailable: true),
                Webpush = BuildWebpushConfig(dataOnly: true)
            });

        private static IReadOnlyDictionary<string, string> PrepareData(IDictionary<string, string>? data) =>
            (data ?? new Dictionary<string, string>())
            .Where(kv => kv.Key != null && kv.Value != null)
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        private static AndroidConfig BuildAndroidConfig(bool dataOnly = false) =>
            new AndroidConfig
            {
                Priority = Priority.High,
                Notification = dataOnly ? null : new AndroidNotification
                {
                    Sound = "default",
                    ChannelId = "default"
                }
            };

        private static ApnsConfig BuildApnsConfig(bool contentAvailable = false) =>
            new ApnsConfig
            {
                Headers = new Dictionary<string, string>
                {
                    ["apns-priority"] = contentAvailable ? "5" : "10"
                },
                Aps = new Aps
                {
                    Sound = contentAvailable ? null : "default",
                    ContentAvailable = contentAvailable
                }
            };

        private static WebpushConfig BuildWebpushConfig(bool dataOnly = false) =>
            new WebpushConfig
            {
                Headers = new Dictionary<string, string>
                {
                    ["Urgency"] = "high"
                },
                Notification = dataOnly ? null : new WebpushNotification
                {
                    RequireInteraction = false
                }
            };
    }

}


