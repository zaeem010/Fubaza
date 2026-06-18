using Fubaza.Application.Core.Common;

namespace Fubaza.Application.Core.Entities
{
    public class TempleteImage :  Document
    {
        public Guid? UserId { get; set; }
        public bool IsUserUpload { get; set; }
        public virtual User User { get; set; } = null!;

    }
}
