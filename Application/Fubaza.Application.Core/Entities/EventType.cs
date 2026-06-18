using System.ComponentModel.DataAnnotations.Schema;

namespace Fubaza.Application.Core.Entities
{
    public class EventType
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SportId { get; set; }
        public string? Name { get; set; }
        public string? NameDe { get; set; }
        public bool IsDeleted { get; set; }
        public string? EventTypeName { get; set; }
        public virtual Sport? Sport { get; set; }
       
    }
}
