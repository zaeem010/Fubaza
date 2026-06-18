using System.Text.Json.Serialization;

namespace Fubaza.Application.DTO.DTO
{
    public class FacebookUserInfoDTO
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("picture")]
        public FacebookPicture Picture { get; set; }
    }

    public class FacebookPicture
    {
        [JsonPropertyName("data")]
        public FacebookPictureData Data { get; set; }
    }

    public class FacebookPictureData
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
