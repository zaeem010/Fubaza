namespace Fubaza.Application.DTO.Services
{
    public class MailRequest
    {
        public string? To { get; set; }

        public string? Subject { get; set; }

        public string? Body { get; set; }

        public string? From { get; set; }

        public string? DisplayName { get; set; }

        public List<MailAttachment> Attachments { get; set; }

        public List<string> Cc { get; set; }

        public MailRequest()
        {
            Attachments = new List<MailAttachment>();
            Cc = new List<string>();
        }
    }

    public class MailAttachment
    {
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
    }
}
