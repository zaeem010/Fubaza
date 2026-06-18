namespace Fubaza.Application.DTO.DTO
{
    public class PaginatedPlayersDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string SubscriptionPlan { get; set; } = "Pro (Monthly)";
        public DateTime? SubscriptionDate { get; set; } = new DateTime(2025, 12, 25);
        public string PlayingPosition { get; set; } = string.Empty;  
        public string CurrentClub { get; set; } = string.Empty;     
        public string FileUrl { get; set; } = string.Empty;    
    }

}
