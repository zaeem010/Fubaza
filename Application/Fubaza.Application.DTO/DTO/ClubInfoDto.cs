namespace Fubaza.Application.DTO.DTO
{
    public class ClubInfoDto
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? SportName { get; set; }
        public string? ClubUrl { get; set; }
        public int TotalPlayers { get; set; }
        public int TotalOfficials { get; set; }
        public int TotalMembers { get; set; } 
        public List<ClubPlayerDto> Players { get; set; } = new();
        public List<ClubOfficialDto> Officials { get; set; } = new();
    }

    public class ClubPlayerDto
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PlayingPositionName { get; set; }
        public string? PlayerUrl { get; set; }
    }

    public class ClubOfficialDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Designation { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? JoiningDate { get; set; }
        public string? ClubOfficialUrl { get; set; }

    }
}
