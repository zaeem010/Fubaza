using Fubaza.Application.DTO.Services;

namespace Fubaza.Application.Core.Contracts.Services
{
    public interface IFirbaseService
    {
        Task<NotificationDispatchResult> SendAsync(NotificationRequest request);
        Task SubscribeAsync(TopicSubscriptionRequest request);
        Task UnsubscribeAsync(TopicSubscriptionRequest request);
        Task<string> SendSilentDataAsync(string token, IDictionary<string, string> data);
    }
}
