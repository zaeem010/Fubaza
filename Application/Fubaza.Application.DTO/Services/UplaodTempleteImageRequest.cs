using Microsoft.AspNetCore.Http;

namespace Fubaza.Application.DTO.Services
{
    public class UplaodTempleteImageRequest
    {
        public IFormFile? file { get; set; }
        public bool IsUserUpload { get; set; }
    }
}
