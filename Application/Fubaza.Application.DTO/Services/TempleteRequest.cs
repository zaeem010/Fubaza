using Fubaza.Application.DTO.Enums;

namespace Fubaza.Application.DTO.Services
{
    public class TempleteRequest
    {
        public Guid SportId { get; set; }
        public TempleteType? TempleteType { get; set; }
    }
}
