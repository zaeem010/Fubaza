using Microsoft.AspNetCore.Http;

namespace Fubaza.Application.DTO.Services
{
    public class MatchDayRequest
    {
        public Guid? Id { get; set; }
        public string? MatchdayNumber { get; set; }
        public DateTime MatchDayDateTime { get; set; }
        public DateTime DisappearDateTime { get; set; }
        public Guid? OrganizerClubId { get; set; }
        public Guid? OpponentClubId { get; set; }
        public string? Referee { get; set; }
        public string? AssistantReferee1 { get; set; }
        public string? AssistantReferee2 { get; set; }
        public string? Location { get; set; }
        public int? CompetitionType { get; set; }
        /// 0 = Home (default), 1 = Away, 2 = Neutral. Optional on create; null is treated as Home.
        public int? Venue { get; set; }
        public DateTime? MatchStartDateTime { get; set; }
        public Guid? UserId { get; set; }

        public virtual List<SponsorDocumentRequest>? SponsorDocuments { get; set; }
    }

    public class SponsorDocumentRequest
    {
        public IFormFile? File { get; set; }
        public string? Sponsor { get; set; }
        public Guid? MatchdayId { get; set; }
    }
}
