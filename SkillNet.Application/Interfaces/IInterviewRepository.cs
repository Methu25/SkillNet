using SkillNet.Application.DTOs;
using SkillNet.Domain.Entities;

namespace SkillNet.Application.Interfaces
{
    public interface IInterviewRepository
    {
        Task<IEnumerable<Interview>> GetAllInterviewsAsync();
        Task<Interview?> GetInterviewByIdAsync(int id);
        Task<Interview> CreateInterviewAsync(Interview interview);
        Task<InterviewSchedulingContext?> GetSchedulingContextAsync(int applicationId);
        Task<IEnumerable<EligibleInterviewerResponse>> GetEligibleInterviewersAsync();
        Task<Interview> CreateScheduledInterviewAsync(Interview interview, IReadOnlyCollection<int> interviewerIds, int changedByUserId, string? note);
        Task<Interview?> UpdateInterviewAsync(int id, Interview interview);
        Task<bool> DeleteInterviewAsync(int id);
        Task<Interview?> UpdateInterviewStatusAsync(int id, string status);
        Task<InterviewEvaluation> CreateEvaluationAsync(InterviewEvaluation evaluation);
        Task<InterviewEvaluation?> GetEvaluationByInterviewIdAsync(int interviewId);
        Task<InterviewEvaluation?> UpdateEvaluationAsync(int interviewId, InterviewEvaluation evaluation);
        Task<IEnumerable<Interview>> GetUpcomingInterviewsAsync();
        Task<IEnumerable<Interview>> GetAssignedInterviewsAsync(int hiringManagerUserId);
        Task<Interview?> GetAssignedInterviewAsync(int interviewId, int hiringManagerUserId);
        Task<InterviewEvaluation> CreateEvaluationAndTransitionAsync(InterviewEvaluation evaluation, int hiringManagerUserId);
        Task<string> RecordDecisionAsync(int interviewId, int hiringManagerUserId, string decision);
        Task<HiringDashboardResponse> GetHiringDashboardAsync();
        Task<InterviewAssignment> AssignInterviewerAsync(InterviewAssignment assignment);
        Task<IEnumerable<InterviewAssignment>> GetInterviewAssignmentsAsync(int interviewId);
    }

    public class InterviewSchedulingContext
    {
        public int ApplicationId { get; set; }
        public int RecruiterUserId { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
    }
}
