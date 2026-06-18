using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace Fubaza.Application.DTO.Enums
{
    public enum Department
    {
        [Display(Name = "Management|Management")]
        Management,

        [Display(Name = "Coaching Staff|Trainerstab")]
        Coaching,

        [Display(Name = "Medical|Medizinisch")]
        Medical,
        
        [Display(Name = "Analysis|Analyse")]
        Analysis,

        [Display(Name = "Marketing|Marketing")]
        Marketing,
        
        [Display(Name = "Administration|Verwaltung")]
        Administration
    }
}
