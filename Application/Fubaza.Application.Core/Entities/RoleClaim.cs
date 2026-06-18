using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Fubaza.Application.Core.Entities
{
    public class RoleClaim : IdentityRoleClaim<Guid>
    {
        public string? Description { get; set; }
        public string? Group { get; set; }

        [NotMapped]
        public bool Selected { get; set; }

        public virtual Role Role { get; set; } = null!;

        public RoleClaim() : base() { }

        public RoleClaim(string? roleClaimDescription = null, string? roleClaimGroup = null) : base()
        {
            Description = roleClaimDescription;
            Group = roleClaimGroup;
        }
    }
}
