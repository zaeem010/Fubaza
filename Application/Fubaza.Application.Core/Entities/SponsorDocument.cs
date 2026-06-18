using Fubaza.Application.Core.Common;
using System.ComponentModel.DataAnnotations;

namespace Fubaza.Application.Core.Entities
{
    public class SponsorDocument: Document
    {
        [MaxLength(512)]
        public string? Sponsor { get; set; }
        public Guid MatchdayId { get; set; }
        public virtual Matchday? Matchday { get; set; }
    }
}
