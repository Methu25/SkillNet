using SkillNet.Server.Models;
using SkillNet.Server.DTOs;

namespace SkillNet.Server.Interfaces
{
    public interface IInterviewService
    {
        Task<IEnumerable<Interview>> GetAllAsync();

        Task<Interview?> GetByIdAsync(int id);

        Task<Interview> CreateAsync(Interview interview);

        Task<Interview?> UpdateAsync(Interview interview);

        Task<bool> DeleteAsync(int id);

        Task<bool> ScheduleInterviewAsync(
            int interviewId,
            ScheduleInterviewDto dto);

        Task<InterviewEvaluation> CreateEvaluationAsync(int interviewId, CreateEvaluationRequest request);
        Task<InterviewEvaluation?> GetEvaluationAsync(int interviewId);
        Task<InterviewEvaluation?> UpdateEvaluationAsync(int interviewId, CreateEvaluationRequest request);
    }
}