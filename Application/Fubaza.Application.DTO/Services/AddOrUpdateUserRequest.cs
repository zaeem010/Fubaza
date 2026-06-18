
namespace Fubaza.Application.DTO.Services
{
    public class AddOrUpdateUserRequest
    {
        public Guid UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public Guid? RoleId { get; set; }
        public string? Password { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsAdminPanel { get; set; }
    }
}
