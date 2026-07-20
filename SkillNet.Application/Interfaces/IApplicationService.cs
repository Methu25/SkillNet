using SkillNet.Application.DTOs;

namespace SkillNet.Application.Interfaces
{
    public interface IApplicationService
    {
        Task<JobApplicationDto> ApplyForJobAsync(int candidateId, CreateJobApplicationDto dto);
        Task<IEnumerable<JobApplicationSummaryDto>> GetCandidateApplicationsAsync(int candidateId);
        Task<JobApplicationDto?> GetCandidateApplicationByIdAsync(int candidateId, int applicationId);
        Task<bool> WithdrawApplicationAsync(int candidateId, int applicationId, WithdrawApplicationDto dto);

        Task<IEnumerable<JobApplicationSummaryDto>> GetApplicationsForJobAsync(
            int jobId,
            int recruiterId,
            ApplicationSearchRequest request);
        Task<JobApplicationDto?> GetRecruiterApplicationByIdAsync(int recruiterId, int applicationId);
        Task<JobApplicationDto?> UpdateApplicationStatusAsync(
            int recruiterId,
            int applicationId,
            UpdateApplicationStatusDto dto);
        Task<RecruiterNoteDto?> AddRecruiterNoteAsync(
            int recruiterId,
            int applicationId,
            AddRecruiterNoteDto dto);
        Task<ApplicationStatisticsDto> GetApplicationStatisticsAsync(int recruiterId, int? jobId = null);
    }
}
