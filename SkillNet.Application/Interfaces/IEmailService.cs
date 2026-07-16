namespace SkillNet.Application.Interfaces
{
    public record EmailDeliveryResult(
        bool Attempted,
        bool Succeeded,
        string? ErrorMessage = null);

    public interface IEmailService
    {
        Task<EmailDeliveryResult> SendAsync(
            string recipientEmail,
            string subject,
            string body,
            string eventType);
    }
}
