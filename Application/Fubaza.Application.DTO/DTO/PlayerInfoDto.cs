using Fubaza.Application.DTO.Enums;

namespace Fubaza.Application.DTO.DTO
{
    public class PlayerInfoDto
    {
        public string? FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Gender? Gender { get; set; }
        public int? WeightKg { get; set; }
        public int? HeightCm { get; set; }
        public int? JerseyNumber { get; set; }
        public DateTime SignedAt { get; set; }
        public string? PlayingPositionName { get; set; }
        public string? CurrentClub { get; set; }
        public int JoinedAt { get; set; }
        public PlayerImageDto? Images { get; set; }
        public List<CareerClubDto> Career { get; set; } = new();
    }

    public class PlayerImageDto
    {
        public string? ProfileUrl { get; set; }
        public string? InMotionUrl { get; set; }
        public string? CelebrationUrl { get; set; }
        public string? FullBodyUrl { get; set; }
    }

    public class CareerClubDto
    {
        public Guid? ClubId { get; set; }
        public string? ClubName { get; set; }
        public int StartYear { get; set; }
        public int? EndYear { get; set; }
        public bool IsCurrentClub { get; set; }
        public string? ClubUrl { get; set; }
    }
    public class LineupPlayerInfoDto
    {
        public Guid PlayerId { get; set; }
        public string? FullName { get; set; }
        public string? FileUrl { get; set; }
    }
}


