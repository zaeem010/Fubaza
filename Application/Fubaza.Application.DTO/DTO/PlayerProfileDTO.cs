using Fubaza.Application.DTO.Enums;

namespace Fubaza.Application.DTO.DTO
{
    public class PlayerProfileDTO
    {
        public string? FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Gender? Gender { get; set; }
        public string? FileUrl { get; set; }
        public Guid? SportId { get; set; }
        public Guid? PlayingPositionId { get; set; }
        public string? PlayingPositionName { get; set; }
        public string? Nationality { get; set; }
        public string? SportName { get; set; }
        public int? WeightKg { get; set; }
        public int? HeightCm { get; set; }
        public StrongFoot? StrongFoot { get; set; }
        public ThrowingHand? ThrowingHand { get; set; }
        public List<PlayerClubHistoryDTO>? ClubHistory { get; set; } = new List<PlayerClubHistoryDTO>();
    }

    public class PlayerClubHistoryDTO
    {
        public Guid ClubId { get; set; }
        public string? ClubName { get; set; }
        public int StartYear { get; set; }
        public int? EndYear { get; set; }
        public bool IsCurrentClub { get; set; }
    }
}


