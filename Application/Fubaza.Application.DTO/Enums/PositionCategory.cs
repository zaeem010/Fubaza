using System.ComponentModel.DataAnnotations;

namespace Fubaza.Application.DTO.Enums
{
    public enum PositionCategory
    {
        // Football
        [Display(Name = "Goalkeeper|Torwart")]
        Football_Goalkeeper = 1,

        [Display(Name = "Defender|Verteidiger")]
        Football_Defender = 2,

        [Display(Name = "Midfielder|Mittelfeldspieler")]
        Football_Midfield = 3,

        [Display(Name = "Striker|Stürmer")]
        Football_Striker = 4,

        // Basketball
        [Display(Name = "Backcourt|Aufbauspieler")]
        Basketball_BackCourt = 5,

        [Display(Name = "Frontcourt|Frontcourt")]
        Basketball_FrontCourt = 6,

        [Display(Name = "Forward|Flügelspieler")]
        Basketball_Forward = 7,

        // Volleyball
        [Display(Name = "Defensive Specialist|Defensivspezialist")]
        Volleyball_DefensiveSpecialist = 8,

        [Display(Name = "Hitter|Angreifer")]
        Volleyball_Hitter = 9,

        [Display(Name = "Setter|Zuspieler")]
        Volleyball_Setter = 10,

        // Ice Hockey
        [Display(Name = "Goaltender|Torwart")]
        IceHockey_Goaltender = 11,

        [Display(Name = "Defenseman|Verteidiger")]
        IceHockey_Defender = 12,

        [Display(Name = "Forward|Stürmer")]
        IceHockey_Forward = 13,

        // Handball
        [Display(Name = "Goalkeeper|Torwart")]
        Handball_Goalkeeper = 14,

        [Display(Name = "Wing|Flügelspieler")]
        Handball_Wing = 15,

        [Display(Name = "Backcourt Player|Rückraumspieler")]
        Handball_Back = 16,

        [Display(Name = "Center Back|Spielmacher")]
        Handball_Center = 17,

        [Display(Name = "Pivot|Kreisläufer")]
        Handball_Pivot = 18,

        // American Football
        [Display(Name = "Quarterback|Quarterback")]
        AmericanFootball_Quarterback = 19,

        [Display(Name = "Offense|Angriff")]
        AmericanFootball_Offense = 20,

        [Display(Name = "Defense|Verteidigung")]
        AmericanFootball_Defense = 21,

        [Display(Name = "Special Teams|Spezialteams")]
        AmericanFootball_Special_Teams = 22,

        // Other
        [Display(Name = "Other|Sonstige")]
        Other = 99
    }
}
