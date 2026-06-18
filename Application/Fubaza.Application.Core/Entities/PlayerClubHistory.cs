namespace Fubaza.Application.Core.Entities
{
    public class PlayerClubHistory
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? PlayerId { get; set; }
        public virtual Player? Player { get; set; }
        public Guid? ClubId { get; set; }
        public bool IsCurrentClub { get; set; }
        public virtual Club? Club { get; set; }
        public int StartYear { get; set; }
        public int? EndYear { get; set; }
    }
}
