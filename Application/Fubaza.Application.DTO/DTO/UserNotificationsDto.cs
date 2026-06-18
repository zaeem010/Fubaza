namespace Fubaza.Application.DTO.DTO
{
    public class UserNotificationsDto
    {
        public int UnreadCount { get; set; }
        public List<NotificationDto> Notifications { get; set; } = new();
    }

    public class NotificationDto
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
