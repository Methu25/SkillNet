using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillNet.Application.Interfaces;
using SkillNet.Domain.Entities;
using SkillNet.Infrastructure.Data;

namespace SkillNet.Infrastructure.Repositories
{
    public class SkillRepository : ISkillRepository
    {
        private readonly ApplicationDbContext _context;

        public SkillRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Skill>> GetAllSkillsAsync()
        {
            return await _context.Skills
                .OrderBy(s => s.SkillName)
                .ToListAsync();
        }

        public async Task<Skill?> GetSkillByIdAsync(int skillId)
        {
            return await _context.Skills.FindAsync(skillId);
        }

        public async Task<IEnumerable<Skill>> GetSkillsByCandidateIdAsync(int candidateId)
        {
            return await _context.CandidateSkills
                .Where(cs => cs.CandidateId == candidateId)
                .Include(cs => cs.Skill)
                .Select(cs => cs.Skill)
                .OrderBy(s => s.SkillName)
                .ToListAsync();
        }

        public async Task<Skill> AddSkillAsync(Skill skill)
        {
            await _context.Skills.AddAsync(skill);
            await _context.SaveChangesAsync();
            return skill;
        }

        public async Task<CandidateSkill> AssignSkillToCandidateAsync(CandidateSkill candidateSkill)
        {
            await _context.CandidateSkills.AddAsync(candidateSkill);
            await _context.SaveChangesAsync();
            return candidateSkill;
        }

        public async Task RemoveSkillFromCandidateAsync(int candidateId, int skillId)
        {
            var candidateSkill = await _context.CandidateSkills
                .FirstOrDefaultAsync(cs => cs.CandidateId == candidateId && cs.SkillId == skillId);

            if (candidateSkill != null)
            {
                _context.CandidateSkills.Remove(candidateSkill);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> SkillExistsAsync(string skillName)
        {
            return await _context.Skills
                .AnyAsync(s => s.SkillName == skillName);
        }

        public async Task<System.Linq.ILookup<int, Skill>> GetSkillsByCandidateIdsAsync(IEnumerable<int> candidateIds)
        {
            var ids = candidateIds?.ToList() ?? new List<int>();
            if (ids.Count == 0)
            {
                var emptyList = new List<KeyValuePair<int, Skill>>();
                return emptyList.ToLookup(x => x.Key, x => x.Value);
            }

            var list = await _context.CandidateSkills
                .AsNoTracking()
                .Where(cs => ids.Contains(cs.CandidateId))
                .Include(cs => cs.Skill)
                .Select(cs => new { cs.CandidateId, cs.Skill })
                .ToListAsync();

            return list.ToLookup(x => x.CandidateId, x => x.Skill);
        }
    }
}
