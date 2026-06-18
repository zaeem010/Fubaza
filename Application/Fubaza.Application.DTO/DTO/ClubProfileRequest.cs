using Microsoft.AspNetCore.Http;

namespace Fubaza.Application.DTO.DTO
{
    public class ClubProfileRequest
    {
        public Guid? UserId { get; set; }
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Nationality { get; set; }  
        public string? League { get; set; }
        public string? Division { get; set; }
        public string? Description { get; set; }
        public IFormFile? File { get; set; }
        public Guid? SportId { get; set; }
        public Guid? RoleId { get; set; }
    }
}
