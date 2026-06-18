using System.ComponentModel.DataAnnotations.Schema;


namespace Fubaza.Application.Core.Entities
{
    public class Club
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? Nationality { get; set; }
        public string? League { get; set; }
        public string? Division { get; set; }
        public string? Description { get; set; }
        public Guid? UserId { get; set; }
        public Guid? CreatorClubId { get; set; }
        public Guid? CreatedByUserId { get; set; }
        public Guid? SportId { get; set; }
        public virtual Sport? Sport { get; set; }
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Player> Players { get; set; }
        public virtual ICollection<ClubOfficial> Officials { get; set; }
        public virtual ClubDocument? Document { get; set; }
        public virtual ICollection<PlayerClubHistory> PlayerClubHistories { get; set; }
        public Club()
        {
            Players = new HashSet<Player>();
            Officials = new HashSet<ClubOfficial>();
            PlayerClubHistories = new HashSet<PlayerClubHistory>();
        }

        #region Not Mapped Properties
        [NotMapped]
        public Guid? RoleId { get; set; }
        [NotMapped]
        public string? PhoneNumber { get; set; }

        #endregion
    }
}
