using System.ComponentModel.DataAnnotations;


namespace Fubaza.Application.DTO.Enums
{
    public enum ThrowingHand
    {
        [Display(Name = "Right|Rechte Hand")]
        Right,

        [Display(Name = "Left|Linke Hand")]
        Left,

        [Display(Name = "Both|Beide Hände")]
        Both
    }
}
