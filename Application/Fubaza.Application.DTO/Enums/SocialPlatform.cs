using System.ComponentModel.DataAnnotations;

namespace Fubaza.Application.DTO.Enums
{
    public enum SocialPlatform
    {
        [Display(Name = "Facebook|Facebook")]   Facebook  = 0,
        [Display(Name = "Instagram|Instagram")] Instagram = 1,
        // TikTok = 2, X = 3 — wenn neue Plattformen kommen, hier ergänzen
        // und einen passenden ISocialInsightsProvider registrieren.
    }
}
