using Fubaza.Application.DTO.Enums;
using Microsoft.AspNetCore.Http;



namespace Fubaza.Application.DTO.Services
{
    public class AddOrUpdateTempleteRequest
    {
        public Guid? TempleteId { get; set; }
        public string? Title { get; set; }
        public TempleteType TempleteType { get; set; }
        public Guid SportId { get; set; }
        public List<IFormFile>? Files { get; set; }
        public List<TempleteDocumentType>? DocumentTypes { get; set; }
    }
}
