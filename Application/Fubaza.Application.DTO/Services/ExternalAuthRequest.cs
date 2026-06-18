namespace Fubaza.Application.DTO.Services
{
    public class ExternalAuthRequest
    {
        public string? Provider { get; set; }
        public string? FullName { get; set; }
        public string? Token { get; set; }
        public string FcmToken { get; set; } = string.Empty;
    }
}
