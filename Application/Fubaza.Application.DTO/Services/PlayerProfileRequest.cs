using Microsoft.AspNetCore.Http;

namespace Fubaza.Application.DTO.Services
{
    public class PlayerProfileRequest
    {
        public Guid? UserId { get; set; }
        public string? FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? Gender { get; set; }
        public string? Nationality { get; set; }
        public IFormFile? File { get; set; }
        public Guid? SportId { get; set; }
        public Guid? RoleId { get; set; }
        public Guid? PlayingPositionId { get; set; }
        public int? WeightKg { get; set; }
        public int? HeightCm { get; set; }
        public int? StrongFoot { get; set; }

        public int? ThrowingHand { get; set; }
        public List<PlayerClubHistoryDto>? ClubHistory { get; set; }
    }

    public class PlayerClubHistoryDto
    {
        public Guid ClubId { get; set; }
        public int StartYear { get; set; }
        public int? EndYear { get; set; } 
        public bool IsCurrentClub { get; set; }
    }
}
