using Microsoft.AspNetCore.Http;


namespace Fubaza.Application.DTO.Services
{
    public class TempleteGenerationRequest
    {
        public IFormFile? Templete { get; set; }
        public string? Prompt { get; set; }
        public IList<IFormFile>? Imgaes { get; set; }

        //public IFormFile? Imgaes { get; set; }
    }
}
