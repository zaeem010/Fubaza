
namespace Fubaza.Application.DTO.DTO
{
    public class UserListItemDto
    {
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Role { get; set; }
        public bool IsActive { get; set; }
        public Guid RoleId { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
