using SkillNet.Application.DTOs;

namespace SkillNet.Application.Interfaces
{
    public interface ISkillService
    {
        Task<IEnumerable<SkillDto>> GetAllSkillsAsync();
        Task<IEnumerable<CandidateSkillDto>> GetCandidateSkillsAsync(int candidateId);
        Task<CandidateSkillDto> AddCandidateSkillAsync(int candidateId, AddCandidateSkillDto dto);
        Task<bool> RemoveCandidateSkillAsync(int candidateId, int skillId);
    }
}
