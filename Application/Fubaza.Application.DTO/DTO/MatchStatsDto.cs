namespace Fubaza.Application.DTO.DTO
{
    public class ClubStatsDto
    {
        public string ClubName { get; set; } = string.Empty;
        public string? ClubUrl { get; set; }
        public Dictionary<string, int> EventCounts { get; set; } = new();
    }

    public class MatchStatsDto
    {
        public ClubStatsDto Organizer { get; set; } = new();
        public ClubStatsDto Opponent { get; set; } = new();
    }

}
