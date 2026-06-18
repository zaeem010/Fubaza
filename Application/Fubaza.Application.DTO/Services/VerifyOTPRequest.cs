namespace Fubaza.Application.DTO.Services
{
    public class VerifyOTPRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
