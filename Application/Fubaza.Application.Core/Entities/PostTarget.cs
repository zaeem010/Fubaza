namespace Fubaza.Application.Core.Entities
{
    public class PostTarget
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PostId { get; set; }
        public Post? Post { get; set; }
        public string? PageId { get; set; }                 // Facebook Page ID
        public string? InstagramBusinessId { get; set; }    // Instagram Business ID
        public string? AccessToken { get; set; }            // Token for this page/account
        public bool IsFacebook { get; set; }
        public bool IsInstagram { get; set; }
        public string? FacebookPostId { get; set; }
        public string? InstagramPostId { get; set; }        // Returned post ID
        public bool IsPublished { get; set; }
    }
}
