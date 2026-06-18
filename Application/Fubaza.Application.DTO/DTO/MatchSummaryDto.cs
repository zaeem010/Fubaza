using Fubaza.Application.DTO.Enums;

namespace Fubaza.Application.DTO.DTO
{
    public class MatchSummaryDto
    {
        public Guid Id { get; set; } 
        public Guid? PlayerId { get; set; }
        public string? PlayerFullName { get; set; }
        public string? PlayerFileUrl { get; set; }
        public Guid? AssistPlayerId { get; set; }
        public string? AssistPlayerFullName { get; set; }
        public string? AssistPlayerFileUrl { get; set; }
        public string? Minute { get; set; }
        public Guid EventTypeId { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? EventType { get; set; }
        public string? ClubName { get; set; }
        public Guid? ClubId { get; set; }

    }
}
