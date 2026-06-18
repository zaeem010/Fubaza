using Fubaza.Application.DTO.Enums;

namespace Fubaza.Application.Core.Entities
{
    public class Matchday
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? MatchdayNumber { get; set; }
        public DateTime MatchDayDateTime { get; set; }
        public DateTime DisappearDateTime { get; set; }
        public Guid? OrganizerClubId { get; set; }
        public Guid? OpponentClubId { get; set; }
        public string? Referee { get; set; }
        public string? AssistantReferee1 { get; set; } 
        public string? AssistantReferee2 { get; set; } 
        public string? Location { get; set; }
        public DateTime? MatchStartDateTime { get; set; }
        public Guid? UserId { get; set; }
        public bool IsLineUp { get; set; }
        public bool IsMatchEnd { get; set; }
        public User? User { get; set; }
        public Club? OrganizerClub { get; set; }
        public Club? OpponentClub { get; set; }
        public CompetitionType? CompetitionType { get; set; }
        public MatchVenue Venue { get; set; } = MatchVenue.Home;
        public virtual IList<SponsorDocument>? SponsorDocuments { get; set; }
        public virtual ICollection<MatchSummary>? MatchSummary { get; set; }
        public virtual ICollection<MatchLineUp>? MatchLineUp { get; set; }
    }
}
