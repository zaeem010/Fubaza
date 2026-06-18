using System.ComponentModel.DataAnnotations;

namespace Fubaza.Application.DTO.Enums
{
    public enum TempleteType
    {
        [Display(Name = "Match Day|Spieltag")]
        Matchday = 1,

        [Display(Name = "Birthday|Geburtstag")]
        Birthday = 2,

        [Display(Name = "Lineup|Aufstellung")]
        Lineup = 3,

        [Display(Name = "Match Score|Spielstand")]
        MatchScore = 4,

        [Display(Name = "Match Win|Spiel gewonnen")]
        MatchWin = 5,

        [Display(Name = "Goal Scored|Tor erzielt")]
        GoalScored = 6
    }

}
