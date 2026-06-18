using Microsoft.AspNetCore.Http;

namespace Fubaza.Application.DTO.Services
{
    public class PostRequest
    {
        public Guid? PostId { get; set; }
        public string? Caption { get; set; }
        public DateTime? ScheduleDateTime { get; set; }
        public bool IsDraft { get; set; }
        public bool IsFacebookLogin { get; set; }
        public IFormFile? File { get; set; }
        public List<PostTargetRequest> PostTarget { get; set; } = new();
    }

    public class PostTargetRequest
    {
        public string? PageId { get; set; }                  // Facebook Page ID
        public string? InstagramBusinessId { get; set; }     // Instagram Business Account ID
        public string? AccessToken { get; set; }             // Page/IG access token
        public bool IsFacebook { get; set; }                 // true → Facebook post
        public bool IsInstagram { get; set; }                // true → Instagram post
    }
}
