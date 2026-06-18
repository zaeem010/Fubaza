
namespace Fubaza.Application.DTO.Services
{
    public class NotificationPreferenceRequest
    {
        public Guid UserId { get; set; }
        public bool IsNotificationEnabled { get; set; }
    }
}
