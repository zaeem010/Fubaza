namespace Fubaza.Application.DTO.DTO
{
    public class SocialDTO
    {
        public string? Email { get; set; }
        public string? Subject { get; set; }
        public bool IsConnectedFacebook { get; set; }
        public string? FacebookLongLivedToken { get; set; }   // 👈 store here
        public DateTime? FacebookTokenExpiresAt { get; set; }
    }
}
