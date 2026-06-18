namespace Fubaza.Application.DTO.Services
{
    public class ChangePasswordRequest
    {
        public string? OldPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}
