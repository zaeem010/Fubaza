using System.Text.Json.Serialization;


namespace Fubaza.Application.DTO.DTO
{
    public class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<Candidate>? Candidates { get; set; }
    }

    public class Candidate
    {
        [JsonPropertyName("content")]
        public Content? Content { get; set; }
    }

    public class Content
    {
        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("parts")]
        public List<Part>? Parts { get; set; }
    }

    public class Part
    {
        // In responses you might see text parts as well.
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("inlineData")]
        public InlineData? InlineData { get; set; }
    }

    public class InlineData
    {
        [JsonPropertyName("mimeType")]
        public string? MimeType { get; set; }

        // Base64-encoded data
        [JsonPropertyName("data")]
        public string? Data { get; set; }
    }
}
