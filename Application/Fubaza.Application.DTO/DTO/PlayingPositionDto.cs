using Fubaza.Application.DTO.Enums;

namespace Fubaza.Application.DTO.DTO
{
    public class PlayingPositionDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid SportId { get; set; }
        public PositionCategory? Category { get; set; }
        public bool IsDeleted { get; set; }
    }
}
