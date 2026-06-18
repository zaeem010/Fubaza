namespace Fubaza.Application.Dto.Services
{
    public class SignUpRequest
    {
        public string? Email { get; set; }
        public string? Password { get; set; }

        public string FcmToken { get; set; } = string.Empty;

    }
}
