namespace Fubaza.Application.DTO.DTO
{
    public class PlayerDashboardSocialStatsDto
    {
        public List<PlatformSocialStatsDto> Platforms { get; set; } = new();
    }

    public class PlatformSocialStatsDto
    {
        public string Platform { get; set; } = string.Empty;   // "Facebook" | "Instagram"
        public int TotalLikes { get; set; }
        public int TotalImpressions { get; set; }
        public int TotalShares { get; set; }
    }
}
