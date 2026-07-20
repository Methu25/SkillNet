using SkillNet.Application.DTOs;
using SkillNet.Domain.Entities;

namespace SkillNet.Application.Interfaces
{
    public interface IJobRepository
    {
        Task<int> InsertJobAsync(JobPost job);
        Task<JobPost?> GetJobByIdAsync(int jobId);
        Task<IEnumerable<JobPost>> GetJobsByRecruiterAsync(int recruiterProfileId);
        Task<IEnumerable<JobPost>> SearchJobsAsync(JobSearchRequest request);
        Task<bool> UpdateJobAsync(JobPost job);
        Task<bool> DeleteJobAsync(int jobId, int recruiterProfileId);
        Task<bool> UpdateJobStatusAsync(int jobId, int recruiterProfileId, string status);
        Task InsertJobSkillsAsync(int jobId, List<int> skillIds);
        Task DeleteJobSkillsAsync(int jobId);
        Task<IEnumerable<int>> GetSkillIdsByJobIdAsync(int jobId);
        Task<IEnumerable<string>> GetSkillsByJobIdAsync(int jobId);
        Task<IEnumerable<SkillDto>> GetAllSkillsAsync();
        Task<int> GetRecruiterOrganizationIdAsync(int recruiterProfileId);
    }
}
