namespace SkillNet.Application.Interfaces
{
    public interface ICandidateNotificationService
    {
        Task SendProfileCompletionReminderAsync(int candidateId);
        Task SendApplicationStatusChangedAsync(int candidateId, int applicationId, string status);
        Task SendInterviewScheduledAsync(int candidateId, int interviewId);
    }
}
