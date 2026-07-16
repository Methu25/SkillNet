namespace SkillNet.Application.Interfaces
{
    public interface ICandidateNotificationService
    {
        Task NotifyProfileProgressAsync(
            int userId,
            int previousPercentage,
            int currentPercentage);
    }
}
