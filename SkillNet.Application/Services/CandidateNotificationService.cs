using Microsoft.Extensions.Logging;
using SkillNet.Application.Interfaces;

namespace SkillNet.Application.Services
{
    public class CandidateNotificationService : ICandidateNotificationService
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly ILogger<CandidateNotificationService> _logger;

        public CandidateNotificationService(
            IUserService userService,
            IEmailService emailService,
            ILogger<CandidateNotificationService> logger)
        {
            _userService = userService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task NotifyProfileProgressAsync(
            int userId,
            int previousPercentage,
            int currentPercentage)
        {
            string? subject = null;
            string? body = null;
            string? eventType = null;
            if (previousPercentage < 100 && currentPercentage == 100)
            {
                subject = "Your SkillNet profile is complete";
                body = "Congratulations!\n\nYour profile is now fully completed and ready to be discovered by recruiters.";
                eventType = "Profile Completed";
            }
            else
            {
                var milestone = new[] { 75, 50, 25 }
                    .FirstOrDefault(value => previousPercentage < value && currentPercentage >= value);
                if (milestone > 0)
                {
                    subject = "Great progress!";
                    body = $"Your SkillNet profile is now {milestone}% complete.\n\nComplete your profile to increase your visibility to recruiters.";
                    eventType = $"Profile Progress {milestone}%";
                }
            }

            if (subject == null) return;
            var email = _userService.GetUserById(userId)?.Email;
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Profile progress email skipped because user {UserId} has no email.", userId);
                return;
            }

            try
            {
                var result = await _emailService.SendAsync(
                    email, subject, body!, eventType!);
                if (!result.Succeeded)
                {
                    _logger.LogWarning(
                        "Profile progress email was not delivered for user {UserId}: {Reason}",
                        userId,
                        result.ErrorMessage);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Profile progress email failed for user {UserId}.", userId);
            }
        }
    }
}
