using System.Collections.Generic;
using System.Threading.Tasks;
using SkillNet.Domain.Entities;

namespace SkillNet.Application.Interfaces
{
    public interface IApplicationRepository
    {
        Task<JobApplication> AddApplicationAsync(JobApplication application);
        Task<JobApplication?> GetApplicationByIdAsync(int applicationId);
        Task<IEnumerable<JobApplication>> GetApplicationsByCandidateIdAsync(int candidateId);
        Task<bool> HasCandidateAppliedAsync(int candidateId, int jobId);
        Task<bool> WithdrawApplicationAsync(JobApplication application);

        Task<IEnumerable<JobApplication>> GetApplicationsByJobIdAsync(int jobId);
        Task UpdateApplicationAsync(JobApplication application);
        Task<RecruiterNote> AddRecruiterNoteAsync(RecruiterNote recruiterNote);
        Task<ApplicationStatusHistory> AddStatusHistoryAsync(ApplicationStatusHistory statusHistory);
        Task<Dictionary<string, int>> GetApplicationStatisticsAsync(int recruiterId, int? jobId = null);
    }
}
