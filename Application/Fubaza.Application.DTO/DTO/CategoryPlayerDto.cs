using Fubaza.Application.DTO.Enums;

namespace Fubaza.Application.DTO.DTO
{
    public class CategoryPlayerDto
    {
        public int Count { get; set; }
        public List<PlayerDto> Players { get; set; } = new();
    }
    public class PlayerDto
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? PlayingPosition { get; set; }
        public Guid? PlayingPositionId { get; set; }
        public Gender? Gender { get; set; }
        public int? JerseyNumber { get; set; }
        public StrongFoot? StrongFoot { get; set; }
        public ThrowingHand? ThrowingHand { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool IsCaption { get; set; }
        public Dictionary<PlayerDocumentType, string?> Documents { get; set; } = new();
    }
}
