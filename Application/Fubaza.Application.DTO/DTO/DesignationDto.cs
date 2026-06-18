using Fubaza.Application.DTO.Enums;

namespace Fubaza.Application.DTO.DTO
{
    public class DesignationDto
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public Department? Department { get; set; }
    }
}
