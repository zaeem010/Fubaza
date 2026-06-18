using System.ComponentModel.DataAnnotations;

namespace Fubaza.Application.DTO.Enums
{
    public enum MatchVenue
    {
        [Display(Name = "Home|Heim")]       Home    = 0,
        [Display(Name = "Away|Auswärts")]   Away    = 1,
        [Display(Name = "Neutral|Neutral")] Neutral = 2,
    }
}
