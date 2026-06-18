namespace Fubaza.Application.Core.Entities
{
    public class MatchLineUp
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid MatchdayId { get; set; }
        public Guid PlayerId { get; set; }
        public Matchday? Matchday { get; set; }
        public Player? Player { get; set; }
    }
}
