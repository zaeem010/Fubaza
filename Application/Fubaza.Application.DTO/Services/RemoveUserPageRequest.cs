using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fubaza.Application.DTO.Services
{
    public class RemoveUserPageRequest
    {
        public bool RemoveFacebook { get; set; }
        public bool RemoveInstagram { get; set; }
        public Guid? UserId { get; set; }
    }
}
