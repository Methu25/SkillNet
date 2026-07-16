using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using SkillNet.Application.Interfaces;

namespace SkillNet.Infrastructure.Email
{
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpOptions _options;

        public SmtpEmailService(IOptions<SmtpOptions> options)
        {
            _options = options.Value;
        }

        public async Task SendAsync(string recipientEmail, string subject, string body)
        {
            if (!_options.Enabled)
            {
                return;
            }

            ValidateConfiguration();

            using var message = new MailMessage
            {
                From = new MailAddress(_options.FromAddress, _options.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            message.To.Add(new MailAddress(recipientEmail));

            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = string.IsNullOrWhiteSpace(_options.Username)
            };

            if (!string.IsNullOrWhiteSpace(_options.Username))
            {
                client.Credentials = new NetworkCredential(_options.Username, _options.Password);
            }

            await client.SendMailAsync(message);
        }

        private void ValidateConfiguration()
        {
            if (string.IsNullOrWhiteSpace(_options.Host) ||
                _options.Port <= 0 ||
                string.IsNullOrWhiteSpace(_options.FromAddress))
            {
                throw new InvalidOperationException("SMTP configuration is incomplete.");
            }
        }
    }
}
