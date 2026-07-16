using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using SkillNet.Application.Interfaces;

namespace SkillNet.Infrastructure.Email
{
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpOptions _options;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(
            IOptions<SmtpOptions> options,
            ILogger<SmtpEmailService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task<EmailDeliveryResult> SendAsync(
            string recipientEmail,
            string subject,
            string body,
            string eventType)
        {
            _logger.LogInformation(
                "Email event {EventType} for {Recipient}. SMTP host {Host}, port {Port}, SSL {EnableSsl}.",
                eventType,
                recipientEmail,
                string.IsNullOrWhiteSpace(_options.Host) ? "(not configured)" : _options.Host,
                _options.Port,
                _options.EnableSsl);

            if (!_options.Enabled)
            {
                var disabledConfigurationError = GetConfigurationError();
                var disabledMessage = disabledConfigurationError == null
                    ? "Email delivery is disabled. Set Smtp:Enabled to true."
                    : $"Email delivery is disabled. Set Smtp:Enabled to true. {disabledConfigurationError}";
                _logger.LogWarning("Email event {EventType} was not attempted. {Reason}", eventType, disabledMessage);
                return new EmailDeliveryResult(false, false, disabledMessage);
            }

            var configurationError = GetConfigurationError();
            if (configurationError != null)
            {
                _logger.LogError("Email event {EventType} was not attempted. {Reason}", eventType, configurationError);
                return new EmailDeliveryResult(false, false, configurationError);
            }

            try
            {
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
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_options.Username, _options.Password)
                };

                await client.SendMailAsync(message);
                _logger.LogInformation(
                    "Email event {EventType} for {Recipient} was accepted by the SMTP provider.",
                    eventType,
                    recipientEmail);
                return new EmailDeliveryResult(true, true);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    "Email event {EventType} for {Recipient} failed. SMTP response: {ErrorMessage}",
                    eventType,
                    recipientEmail,
                    exception.Message);
                return new EmailDeliveryResult(true, false, exception.Message);
            }
        }

        private string? GetConfigurationError()
        {
            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(_options.Host)) missing.Add("Smtp:Host");
            if (_options.Port <= 0) missing.Add("Smtp:Port");
            if (string.IsNullOrWhiteSpace(_options.Username)) missing.Add("Smtp:Username");
            if (string.IsNullOrWhiteSpace(_options.Password)) missing.Add("Smtp:Password");
            if (string.IsNullOrWhiteSpace(_options.FromAddress)) missing.Add("Smtp:FromAddress");
            if (string.IsNullOrWhiteSpace(_options.FromName)) missing.Add("Smtp:FromName");

            if (missing.Count > 0)
            {
                return $"SMTP configuration is incomplete. Missing: {string.Join(", ", missing)}.";
            }

            return null;
        }
    }
}
