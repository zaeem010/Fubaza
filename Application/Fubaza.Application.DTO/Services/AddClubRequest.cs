using Microsoft.AspNetCore.Http;


namespace Fubaza.Application.DTO.Services
{
    public class AddClubRequest
    {
        public string? FullName { get; set; }
        public Guid? SportId { get; set; }
        public IFormFile? File { get; set; }
    }
}
