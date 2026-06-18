using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fubaza.Application.DTO.Enums
{
    public enum CompetitionType
    {
        [Display(Name = "League|Liga")]
        League,
        [Display(Name = "Cup|Pokal")]
        Cup,
        [Display(Name = "Friendly|Freundschaftsspiel")] 
        Friendly,
        [Display(Name = "Tournament|Turnier")] 
        Tournament,
    }
}
