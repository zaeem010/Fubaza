using Fubaza.Application.DTO.Enums;


namespace Fubaza.Application.Core.Entities
{
    public class Designation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Title { get; set; }
        public string? TitleDe { get; set; }
        public Department? Department { get; set; }
        public ICollection<ClubOfficial>? ClubOfficials { get; set; }
    }
}
