using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

using Fubaza.Application.Core.Contracts.Services;
using Fubaza.Application.Core.Settings;
using Fubaza.Application.DTO.Services;

namespace Fubaza.Application.Utilities
{
    public class SmtpMailService : IMailService
    {
        private readonly MailSettings _settings;
        private readonly ILogger<SmtpMailService> _logger;

        public SmtpMailService(IOptions<MailSettings> settings, ILogger<SmtpMailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendAsync(MailRequest request)
        {
            try
            {
                _logger.LogInformation("Preparing to send email: {Subject} to {To}", request.Subject, request.To);

                var fromAddress = request.From ?? _settings.From;
                var displayName = request.DisplayName ?? _settings.DisplayName;
                var from = new MailboxAddress(displayName, fromAddress);
                _logger.LogInformation("Email sender set as: {DisplayName} <{FromAddress}>", displayName, fromAddress);

                var email = new MimeMessage
                {
                    Sender = from,
                    Subject = request.Subject,
                    From = { from }
                };

                var recipient = request.To ?? _settings.DefaultTo;
                email.To.Add(MailboxAddress.Parse(recipient));
                _logger.LogInformation("Email recipient: {Recipient}", recipient);

                if (request.Cc is { Count: > 0 })
                {
                    _logger.LogInformation("Adding CC recipients: {CcList}", string.Join(", ", request.Cc));
                    email.Cc.AddRange(request.Cc.Select(MailboxAddress.Parse));
                }

                var builder = new BodyBuilder
                {
                    HtmlBody = request.Body
                };

                _logger.LogInformation("Email body content length: {Length}", request.Body?.Length ?? 0);

                if (request.Attachments is { Count: > 0 })
                {
                    _logger.LogInformation("Processing {AttachmentCount} attachments", request.Attachments.Count);

                    foreach (var attachment in request.Attachments)
                    {
                        if (string.IsNullOrEmpty(attachment.FilePath))
                        {
                            _logger.LogWarning("Attachment FilePath is null or empty. Skipping attachment: {FileName}", attachment.FileName);
                            continue;
                        }

                        _logger.LogInformation("Attaching file: {FileName}", attachment.FileName);
                        await using var fileStream = new FileStream(attachment.FilePath, FileMode.Open);
                        builder.Attachments.Add(attachment.FileName, fileStream);
                    }
                }

                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();

                _logger.LogInformation("Connecting to SMTP server: {Host}:{Port}", _settings.Host, _settings.Port);
                smtp.ServerCertificateValidationCallback = SslCertificateValidationCallback;

                await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.SslOnConnect);
                _logger.LogInformation("Connected to SMTP server");

                await smtp.AuthenticateAsync(_settings.UserName, _settings.Password);
                _logger.LogInformation("Authenticated to SMTP server as: {User}", _settings.UserName);

                await smtp.SendAsync(email);
                _logger.LogInformation("Email sent successfully: {Subject}", request.Subject);

                await smtp.DisconnectAsync(true);
                _logger.LogInformation("Disconnected from SMTP server");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email sending failed for subject: {Subject}", request.Subject);
                throw;
            }
        }

        public bool SslCertificateValidationCallback(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            var host = sender as string ?? "Unknown Host";

            if (certificate == null)
            {
                _logger.LogError("SSL certificate is null for {Host}", host);
                return false;
            }

            if (chain == null)
            {
                _logger.LogError("SSL certificate chain is null for {Host}", host);
                return false;
            }

            if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNotAvailable) != 0)
            {
                _logger.LogError("The SSL certificate was not available for {Host}", host);
                return false;
            }

            if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) != 0)
            {
                var cert2 = certificate as X509Certificate2;
                var cn = cert2 != null ? cert2.GetNameInfo(X509NameType.SimpleName, false) : certificate.Subject;

                _logger.LogError("The Common Name for the SSL certificate did not match {Host}. Instead, it was {CN}.", host, cn);
                return false;
            }

            _logger.LogInformation("The SSL certificate for the server could not be validated for the following reasons:");

            foreach (var element in chain.ChainElements)
            {
                if (element.ChainElementStatus.Length == 0)
                    continue;

                _logger.LogInformation("• {Subject}", element.Certificate.Subject);
                foreach (var error in element.ChainElementStatus)
                {
                    _logger.LogError("\t• {Error}", error.StatusInformation);
                }
            }
            return false;
        }


        


    }
}
