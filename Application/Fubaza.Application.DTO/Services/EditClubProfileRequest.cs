using Microsoft.AspNetCore.Http;

namespace Fubaza.Application.DTO.Services
{
    public class EditClubProfileRequest
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public virtual string? PhoneNumber { get; set; }
        public string? Nationality { get; set; }
        public string? League { get; set; }
        public string? Division { get; set; }
        public string? Description { get; set; }
        public IFormFile? File { get; set; }
    }
}
