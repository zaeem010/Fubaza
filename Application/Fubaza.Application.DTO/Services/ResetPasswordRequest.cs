namespace Fubaza.Application.Dto.Services
{
    public class ResetPasswordRequest
    {
        public Guid? UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
