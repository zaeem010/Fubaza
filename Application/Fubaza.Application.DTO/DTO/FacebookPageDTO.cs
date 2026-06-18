namespace Fubaza.Application.DTO.DTO
{
    public class FacebookPagesResponseDTO
    {
        public bool IsFacebookConnected { get; set; }
        public DateTime? FacebookTokenExpiresAt { get; set; }
        public List<FacebookPageDTO> Pages { get; set; } = new();
    }

    public class FacebookPageDTO
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? AccessToken { get; set; }
        public string? PictureUrl { get; set; }
        public FacebookPageProfileDTO? FacebookProfile { get; set; }
        public string? InstagramBusinessId { get; set; }
        public InstagramProfileDTO? InstagramProfile { get; set; }
    }

    public class FacebookPageProfileDTO
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Username { get; set; }
        public string? About { get; set; }
        public string? Category { get; set; }
        public string? Link { get; set; }
        public string? Website { get; set; }
        public string? PictureUrl { get; set; }
        public int? FanCount { get; set; }
        public int? FollowersCount { get; set; }
    }

    public class InstagramProfileDTO
    {
        public string? Id { get; set; }
        public string? Username { get; set; }
        public string? Name { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? Biography { get; set; }
        public int? FollowersCount { get; set; }
        public int? FollowsCount { get; set; }
        public int? MediaCount { get; set; }
        public string? Website { get; set; }
    }

    public class InstagramResponseDTO
    {
        public bool IsInstagramConnected { get; set; }
        public DateTime? InstagramTokenExpiresAt { get; set; }
        public string? AccessToken { get; set; }
    }
}
