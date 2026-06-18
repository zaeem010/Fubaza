using Microsoft.AspNetCore.Identity;

using System.ComponentModel.DataAnnotations.Schema;

namespace Fubaza.Application.Core.Entities
{
    public class User : IdentityUser<Guid>
    {
        public string? FullName { get; set; }
        public bool IsPasswordRequired { get; set; }
        public string? EmailVerificationCode { get; set; }
        public DateTime? CodeGeneratedAt { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
        public bool CanLogin { get; set; }
        public bool IsActive { get; set; }
        public bool IsAdminPanel { get; set; }
        public DateTime Created { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? FcmToken { get; set; }
        public DateTime? LastModified { get; set; }
        public bool IsConnectedFacebook { get; set; }
        public string? FacebookLongLivedToken { get; set; }
        public DateTime? FacebookTokenExpiresAt { get; set; }
        public bool IsConnectedInstagram { get; set; }
        public string? InstagramLongLivedToken { get; set; }
        public string? InstagramBusinessId { get; set; }
        public DateTime? InstagramTokenExpiresAt { get; set; }
        public string? LanguageCode { get; set; }
        public bool IsNotificationEnabled { get; set; }
        public virtual Player? Player { get; set; }
        public virtual Club? Club { get; set; }
        public virtual ICollection<Role>? Roles { get; set; }

        #region Not Mapped Properties
        [NotMapped]
        public string? Password { get; set; }
        
        [NotMapped]
        public bool IsAddedFromPortal { get; set; } = false;

        [NotMapped]
        public Guid? RoleId { get; set; }


        #endregion
    }

   
}
