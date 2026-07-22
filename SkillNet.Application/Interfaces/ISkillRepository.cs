using System.Collections.Generic;
using System.Threading.Tasks;
using SkillNet.Domain.Entities;

namespace SkillNet.Application.Interfaces
{
    public interface ISkillRepository
    {
        Task<IEnumerable<Skill>> GetAllSkillsAsync();
        Task<Skill?> GetSkillByIdAsync(int skillId);
        Task<IEnumerable<Skill>> GetSkillsByCandidateIdAsync(int candidateId);
        Task<Skill> AddSkillAsync(Skill skill);
        Task<CandidateSkill> AssignSkillToCandidateAsync(CandidateSkill candidateSkill);
        Task RemoveSkillFromCandidateAsync(int candidateId, int skillId);
        Task<bool> SkillExistsAsync(string skillName);
        Task<System.Linq.ILookup<int, Skill>> GetSkillsByCandidateIdsAsync(IEnumerable<int> candidateIds);
    }
}
