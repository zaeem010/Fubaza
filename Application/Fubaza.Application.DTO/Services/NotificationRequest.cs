namespace Fubaza.Application.DTO.Services
{
    public class NotificationRequest
    {
        /// Token for single-device push
        public string? Token { get; set; }

        /// Tokens for multi-device push
        public List<string>? Tokens { get; set; }

        /// Topic name for topic push (e.g., "news")
        public string? Topic { get; set; }

        /// Notification title/body
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;

        /// Optional key-value payload
        public Dictionary<string, string>? Data { get; set; }

        /// Send as data-only (no banner)
        public bool DataOnly { get; set; } = false;
    }

    public class TopicSubscriptionRequest
    {
        public string Topic { get; set; } = string.Empty;
        public List<string> Tokens { get; set; } = new();
    }

    public class PerTokenResult
    {
        public int Index { get; set; }
        public string? Token { get; set; }
        public bool IsSuccess { get; set; }
        public string? MessageId { get; set; }
        public string? Error { get; set; }
    }

    public class NotificationDispatchResult
    {
        public string Mode { get; set; } = ""; // "token" | "tokens" | "topic"
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public string? MessageId { get; set; } // for single/token or topic send
        public List<PerTokenResult> Results { get; set; } = new(); // for multi-token
    }
}
