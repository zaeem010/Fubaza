using Fubaza.Application.DTO.Enums;
using System.ComponentModel.DataAnnotations.Schema;


namespace Fubaza.Application.Core.Entities
{
    public class Player
    {
        public Player()
        {
            ClubHistory = new HashSet<PlayerClubHistory>();
            
        }
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Gender? Gender { get; set; }
        public int? WeightKg { get; set; }
        public int? HeightCm { get; set; }
        public StrongFoot? StrongFoot { get; set; }
        public ThrowingHand? ThrowingHand { get; set; }
        public Guid? CurrentClubId { get;  set; }
        public Guid UserId { get; set; }
        public Guid? PlayingPositionId { get; set; }
        public Guid? SportId { get; set; }
        public bool IsCaption { get; set; }
        public int? JerseyNumber { get; set; }
        public string? Nationality { get; set; }
        public virtual Club? CurrentClub { get;  set; }
        public virtual Sport? Sport { get; set; }
        public virtual PlayingPosition? PlayingPosition { get; set; }
        public virtual ICollection<PlayerClubHistory> ClubHistory { get;  set; }
        public virtual List<PlayerDocument> Documents { get; set; } = new List<PlayerDocument>();
        public virtual List<MatchSummary> MatchSummaries { get; set; } = new List<MatchSummary>();
        public virtual User User { get; set; } = null!;

        #region Not Mapped Properties
        [NotMapped]
        public Guid? RoleId { get; set; }
        #endregion
    }
}
