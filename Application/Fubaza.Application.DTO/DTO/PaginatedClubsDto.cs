namespace Fubaza.Application.DTO.DTO
{
    public class PaginatedClubsDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Owner { get; set; } = "Autumn Phillips";
        public string SubscriptionPlan { get; set; } = "Pro (Monthly)";
        public DateTime? SubscriptionDate { get; set; } = new DateTime(2025, 12, 25);
        public string FileUrl { get; set; } = string.Empty;         
    }
}
