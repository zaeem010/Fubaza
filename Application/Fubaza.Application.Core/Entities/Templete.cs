using Fubaza.Application.DTO.Enums;

namespace Fubaza.Application.Core.Entities
{
    public class Templete
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Title { get; set; }
        public TempleteType? TempleteType { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? SportId { get; set; }
        public Guid? UserId { get; set; }
        public bool IsApproved { get; set; }
        public bool IsDraft { get; set; }
        public virtual Sport? Sport { get; set; }
        public virtual ICollection<TempleteDocument> Documents { get; set; } = new List<TempleteDocument>();
        public virtual User User { get; set; } = null!;

    }
}
