using SkillNet.Application.DTOs;
using SkillNet.Domain.Entities;

namespace SkillNet.Application.Interfaces
{
    public interface IJobRepository
    {
        Task<int> InsertJobAsync(JobPost job);
        Task<JobPost?> GetJobByIdAsync(int jobId);
        Task<IEnumerable<JobPost>> GetJobsByRecruiterAsync(int recruiterId);
        Task<IEnumerable<JobPost>> SearchJobsAsync(JobSearchRequest request);
        Task<bool> UpdateJobAsync(JobPost job);
        Task<bool> DeleteJobAsync(int jobId, int recruiterId);
        Task<bool> UpdateJobStatusAsync(int jobId, int recruiterId, string status);
        Task InsertJobSkillsAsync(int jobId, List<int> skillIds);
        Task DeleteJobSkillsAsync(int jobId);
        Task<IEnumerable<string>> GetSkillsByJobIdAsync(int jobId);
        Task<int> GetRecruiterOrganizationIdAsync(int recruiterId);
    }
}
