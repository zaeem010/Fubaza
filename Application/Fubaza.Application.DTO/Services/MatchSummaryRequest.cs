
namespace Fubaza.Application.DTO.Services
{
    public class MatchSummaryRequest
    {
        public Guid? Id { get; set; } 
        public Guid EventTypeId { get; set; }
        public int? Minute { get; set; }
        public string? Description { get; set; }
        public Guid? ClubId { get; set; }
        public Guid MatchdayId { get; set; }
        public Guid? PlayerId { get; set; }
        public Guid? AssistPlayerId { get; set; }
    }
}
