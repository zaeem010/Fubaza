using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fubaza.Application.DTO.DTO
{
    public class ClubStatDto
    {
        public int? Matches { get; set; }
        public int? Goals { get; set; }
        public int? Wins { get; set; }
        public int? Loses { get; set; }
        public int? Draws { get; set; }
    }
}
