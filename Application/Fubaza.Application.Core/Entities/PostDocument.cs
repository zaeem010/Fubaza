using Fubaza.Application.Core.Common;


namespace Fubaza.Application.Core.Entities
{
    public class PostDocument :  Document
    {
        public Guid PostId { get; set; }
        public virtual Post? Post { get; set; }
    }
}
