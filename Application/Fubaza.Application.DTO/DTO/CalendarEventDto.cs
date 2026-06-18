namespace Fubaza.Application.DTO.DTO
{
    public class CalendarEventDto
    {
        public Guid Id { get; set; }
        public DateTime DateTime { get; set; }
        public string? Caption { get; set; }
        public string? FileUrl { get; set; }
        public string Type { get; set; } = string.Empty; // "Post" or "Matchday"
        public string? Platform { get; set; } // e.g. Facebook, Instagram, TikTok

        public int? OrganizerClubFinalScore { get; set; }
        public string? OrganizerClubName { get; set; }
        public string? OrganizerClubFileUrl { get; set; }
        
        public int? OpponentClubFinalScore { get; set; }
        public string? OpponentClubName { get; set; }
        public string? OpponentClubFileUrl { get; set; }
        /// 0 = Home, 1 = Away, 2 = Neutral. Null for non-matchday events (e.g. Posts).
        public int? Venue { get; set; }
    }
}
