namespace Fubaza.Application.Dto.Services
{
    public class SignInRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? FcmToken { get; set; } = string.Empty;

        public bool IsAdminPanel { get; set; } = false;
    }
}
