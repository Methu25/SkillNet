using System.Collections.Generic;
using System.Threading.Tasks;
using SkillNet.Domain.Entities;

namespace SkillNet.Application.Interfaces
{
    public interface ICandidateRepository
    {
        Task<Candidate?> GetCandidateByUserIdAsync(int userId);
        Task<bool> CandidateExistsAsync(int userId);
        Task<Candidate> AddCandidateAsync(Candidate candidate);
        Task UpdateCandidateAsync(Candidate candidate);
        Task DeleteCandidateAsync(int userId);
    }
}
