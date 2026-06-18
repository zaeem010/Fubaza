using Fubaza.Application.Core.Common;
using Fubaza.Application.DTO.Enums;

namespace Fubaza.Application.Core.Entities
{
    public class PlayerDocument : Document
    {
        public Guid PlayerId { get; set; }
        public virtual Player? Player { get; set; }
        public PlayerDocumentType DocumentType { get; set; }
    }
    
}
