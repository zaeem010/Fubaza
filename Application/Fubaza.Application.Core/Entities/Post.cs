namespace Fubaza.Application.Core.Entities
{
    public class Post
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Caption { get; set; }
        public DateTime? ScheduleDateTime { get; set; }
        public bool IsDraft { get; set; }
        public Guid UserId { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsFacebookLogin { get; set; }
        public bool IsCancelled { get; set; }
        public User? User { get; set; }
        public virtual PostDocument? Document { get; set; }
        public virtual ICollection<PostTarget> Targets { get; set; } = new List<PostTarget>();
    }
}
