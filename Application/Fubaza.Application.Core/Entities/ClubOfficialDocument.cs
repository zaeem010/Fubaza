using Fubaza.Application.Core.Common;


namespace Fubaza.Application.Core.Entities
{
    public class ClubOfficialDocument : Document
    {
        public Guid ClubOfficialId { get; set; }
        public virtual ClubOfficial? ClubOfficial { get; set; }
    }
}
