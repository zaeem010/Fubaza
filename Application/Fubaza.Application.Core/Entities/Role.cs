using Microsoft.AspNetCore.Identity;


namespace Fubaza.Application.Core.Entities
{
    public class Role : IdentityRole<Guid>
    {
        public virtual string? NameDe { get; set; }
        public  string? Description { get; set; }
        public virtual ICollection<RoleClaim> RoleClaims { get; set; }

        public Role() : base()
        {
            RoleClaims = new HashSet<RoleClaim>();
        }

        public Role(string roleName, string? roleDescription = null) : base(roleName)
        {
            RoleClaims = new HashSet<RoleClaim>();
            Description = roleDescription;
        }
    }
}
