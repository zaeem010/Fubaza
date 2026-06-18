namespace Fubaza.Application.Core.Settings
{
	public class JwtSettings
	{
		public string Key { get; set; } = string.Empty;
        public int TokenExpirationInMinutes { get; set; } 
        public int RefreshTokenExpirationInDays { get; set; }
		public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
    }
}
