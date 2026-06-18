namespace Fubaza.Application.DTO.DTO
{
    public class MatchHistoryDto
    {
        public Guid Id { get; set; }
        public string? MatchdayNumber { get; set; }
        public DateTime MatchDayDateTime { get; set; }
        public string? Referee { get; set; }
        public string? AssistantReferee1 { get; set; }
        public string? AssistantReferee2 { get; set; }
        public string? Location { get; set; }
        public Guid? OrganizerClubId { get; set; }
        public Guid? OpponentClubId { get; set; }
        public string? OrganizerClubName { get; set; }
        public string? OpponentClubName { get; set; }
        public string? OrganizerClubUrl { get; set; }
        public string? OpponentClubUrl { get; set; }
        public int OrganizerGoal { get; set; } = 0;
        public int OpponentGoal { get; set; } = 0;
        public DateTime? MatchStartDateTime { get; set; }
        public bool? IsLineUp { get; set; }
        /// 0 = Home, 1 = Away, 2 = Neutral.
        public int Venue { get; set; }
    }
}
