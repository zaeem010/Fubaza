using Fubaza.Application.Core.Common;
using Fubaza.Application.DTO.Enums;

namespace Fubaza.Application.Core.Entities
{
    public class TempleteDocument : Document
    {
        public Guid TempleteId { get; set; }
        public virtual Templete? Templete { get; set; }
        public TempleteDocumentType DocumentType { get; set; }
    }
}
