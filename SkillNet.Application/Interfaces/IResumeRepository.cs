using System.Collections.Generic;
using System.Threading.Tasks;
using SkillNet.Domain.Entities;

namespace SkillNet.Application.Interfaces
{
    public interface IResumeRepository
    {
        Task<IEnumerable<Resume>> GetAllResumesByCandidateIdAsync(int candidateId);
        Task<Resume?> GetActiveResumeByCandidateIdAsync(int candidateId);
        Task<Resume?> GetResumeByIdAsync(int resumeId);
        Task<Resume> AddResumeAsync(Resume resume);
        Task UpdateResumeAsync(Resume resume);
        Task DeleteResumeAsync(int resumeId);
    }
}
