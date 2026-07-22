using SkillNet.Application.DTOs;

namespace SkillNet.Application.Interfaces
{
    public interface IInterviewService
    {
        Task<IEnumerable<InterviewResponse>> GetAllInterviewsAsync();
        Task<InterviewResponse?> GetInterviewByIdAsync(int id);
        Task<InterviewResponse> CreateInterviewAsync(CreateInterviewRequest request);
        Task<IEnumerable<EligibleInterviewerResponse>> GetEligibleInterviewersAsync();
        Task<InterviewResponse?> UpdateInterviewAsync(int id, UpdateInterviewRequest request);
        Task<bool> DeleteInterviewAsync(int id);
        Task<InterviewResponse?> ScheduleInterviewAsync(int id, ScheduleInterviewRequest request);
        Task<InterviewResponse?> RescheduleInterviewAsync(int id, ScheduleInterviewRequest request);
        Task<InterviewResponse?> CancelInterviewAsync(int id);
        Task<EvaluationResponse> CreateEvaluationAsync(int interviewId, CreateEvaluationRequest request);
        Task<EvaluationResponse?> GetEvaluationByInterviewIdAsync(int interviewId);
        Task<EvaluationResponse?> UpdateEvaluationAsync(int interviewId, CreateEvaluationRequest request);
        Task<IEnumerable<InterviewResponse>> GetUpcomingInterviewsAsync();
        Task<IEnumerable<InterviewResponse>> GetAssignedInterviewsAsync();
        Task<InterviewResponse?> GetAssignedInterviewAsync(int interviewId);
        Task<string> RecordDecisionAsync(int interviewId, InterviewDecisionRequest request);
        Task<HiringDashboardResponse> GetHiringDashboardAsync();
        Task<bool> AssignInterviewerAsync(int interviewId, AssignInterviewerRequest request);
    }
}
