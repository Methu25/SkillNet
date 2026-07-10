using SkillNet.Server.DTOs;
using SkillNet.Server.Models;

namespace SkillNet.Server.Interfaces
{
    public interface IInterviewRepository
    {
        Task<IEnumerable<Interview>> GetAllInterviewsAsync();

        Task<Interview?> GetInterviewByIdAsync(int id);

        Task<Interview> CreateInterviewAsync(Interview interview);

        Task<Interview?> UpdateInterviewAsync(int id, Interview interview);

        Task<bool> DeleteInterviewAsync(int id);

        Task<Interview?> UpdateInterviewStatusAsync(int id, string status);

        Task<InterviewEvaluation> CreateEvaluationAsync(InterviewEvaluation evaluation);

        Task<InterviewEvaluation?> GetEvaluationByInterviewIdAsync(int interviewId);

        Task<InterviewEvaluation?> UpdateEvaluationAsync(int interviewId, InterviewEvaluation evaluation);

        Task<IEnumerable<Interview>> GetUpcomingInterviewsAsync();

        Task<HiringDashboardResponse> GetHiringDashboardAsync();

        Task<InterviewAssignment> AssignInterviewerAsync(InterviewAssignment assignment);

        Task<IEnumerable<InterviewAssignment>> GetInterviewAssignmentsAsync(int interviewId);
    }
}