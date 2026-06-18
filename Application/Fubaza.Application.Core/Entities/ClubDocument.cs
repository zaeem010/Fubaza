using Fubaza.Application.Core.Common;

namespace Fubaza.Application.Core.Entities
{
    public class ClubDocument : Document
    {
        public Guid ClubId { get; set; }
        public virtual Club? Club { get; set; }
    }
}
