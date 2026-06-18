using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace Fubaza.Application.DTO.Enums
{
    public enum StrongFoot
    {
        [Display(Name = "Right Foot|Rechter Fuß")]
        Right,

        [Display(Name = "Left Foot|Linker Fuß")]
        Left,

        [Display(Name = "Two-Footed (Ambidextrous)|Beidfüßig (Beidhändig)")]
        TwoFooted
    }
}
