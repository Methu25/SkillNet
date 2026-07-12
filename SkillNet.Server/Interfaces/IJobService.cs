using SkillNet.Server.DTOs;

namespace SkillNet.Server.Interfaces
{
    public interface IJobService
    {
        Task<JobResponse> CreateJobAsync(int recruiterId, CreateJobRequest request);
        Task<JobResponse?> GetJobByIdAsync(int jobId);
        Task<IEnumerable<JobResponse>> SearchJobsAsync(JobSearchRequest request);
        Task<IEnumerable<JobResponse>> GetRecruiterJobsAsync(int recruiterId);
        Task<JobResponse?> UpdateJobAsync(int jobId, int recruiterId, UpdateJobRequest request);
        Task<bool> DeleteJobAsync(int jobId, int recruiterId);
        Task<JobResponse?> PublishJobAsync(int jobId, int recruiterId);
        Task<JobResponse?> CloseJobAsync(int jobId, int recruiterId);
        Task<JobResponse> DuplicateJobAsync(int jobId, int recruiterId);
    }
}
