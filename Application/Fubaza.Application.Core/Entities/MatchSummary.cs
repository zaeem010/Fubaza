
namespace Fubaza.Application.Core.Entities
{
    public class MatchSummary
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid EventTypeId { get; set; }
        public int? Minute { get; set; }
        public string? Description { get; set; }
        public Guid? ClubId { get; set; }
        public Guid MatchdayId { get; set; }
        public Guid? PlayerId { get; set; }
        public Guid? AssistPlayerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Club? Club { get; set; }
        public Matchday? Matchday { get; set; }
        public EventType? EventType { get; set; }
        public virtual Player? Player { get; set; }
        public virtual Player? AssistPlayer { get; set; }
    }
}
