namespace Fubaza.Application.Core.Entities
{
    public class AuditableEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? TableName { get; set; }
        public string? Action { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid? UserId { get; set; }
        public string? Changes { get; set; }
    }
}
