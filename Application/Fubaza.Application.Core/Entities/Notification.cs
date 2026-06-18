namespace Fubaza.Application.Core.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string TitleDe { get; set; } = string.Empty;
        public string BodyDe { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid UserId { get; set; }
        public User? User { get; set; }   
    }
}
