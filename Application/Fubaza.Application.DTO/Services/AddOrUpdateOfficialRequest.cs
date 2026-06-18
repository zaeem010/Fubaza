using Microsoft.AspNetCore.Http;

namespace Fubaza.Application.DTO.Services
{
    public class AddOrUpdateOfficialRequest
    {
        public Guid? OfficialId { get; set; }
        public string? Name { get; set; }
        public int? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? JoiningDate { get; set; }
        public Guid DesignationId { get; set; }
        public Guid ClubId { get; set; }
        public IFormFile? File { get; set; }
    }
}
