using Microsoft.AspNetCore.Http;

namespace Fubaza.Application.DTO.Services
{
    public class EditPlayerProfileRequest
    {
        public Guid? UserId { get; set; }
        public string? FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? Gender { get; set; }
        public IFormFile? File { get; set; }
        public string? Nationality { get; set; }
        public Guid? CurrentClubId { get; set; }
        public int? WeightKg { get; set; }
        public int? HeightCm { get; set; }
        public int? StrongFoot { get; set; }
        public int? ThrowingHand { get; set; }
        public Guid? PlayingPositionId { get; set; }
        public List<EditPlayerClubHistoryDto>? ClubHistory { get; set; }
    }

    public class EditPlayerClubHistoryDto
    {
        public Guid ClubId { get; set; }
        public int StartYear { get; set; }
        public int? EndYear { get; set; }
        public bool IsCurrentClub { get; set; }
    }
}
