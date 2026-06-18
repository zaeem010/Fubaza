using Fubaza.Application.DTO.Enums;


namespace Fubaza.Application.Core.Entities
{
    public class PlayingPosition
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Name { get; set; }
        public string? NameDe { get; set; }
        public Guid SportId { get; set; }
        public PositionCategory? Category { get; set; }
        public bool IsDeleted { get; set; }
        public int OrderId { get; set; }
        public virtual Sport? Sport { get; set; }  
    }
}
