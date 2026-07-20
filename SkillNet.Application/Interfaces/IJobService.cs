using SkillNet.Application.DTOs;

namespace SkillNet.Application.Interfaces
{
    public interface IJobService
    {
        Task<JobResponse> CreateJobAsync(int userId, CreateJobRequest request);
        Task<JobResponse?> GetJobByIdAsync(int jobId);
        Task<IEnumerable<JobResponse>> SearchJobsAsync(JobSearchRequest request);
        Task<IEnumerable<JobResponse>> GetRecruiterJobsAsync(int userId);
        Task<JobResponse?> UpdateJobAsync(int jobId, int userId, UpdateJobRequest request);
        Task<bool> DeleteJobAsync(int jobId, int userId);
        Task<JobResponse?> PublishJobAsync(int jobId, int userId);
        Task<JobResponse?> CloseJobAsync(int jobId, int userId);
        Task<JobResponse> DuplicateJobAsync(int jobId, int userId);
        Task<IEnumerable<SkillDto>> GetSkillsAsync();
    }
}
