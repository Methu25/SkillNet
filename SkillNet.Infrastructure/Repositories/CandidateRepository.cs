using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillNet.Application.Interfaces;
using SkillNet.Domain.Entities;
using SkillNet.Infrastructure.Data;

namespace SkillNet.Infrastructure.Repositories
{
    public class CandidateRepository : ICandidateRepository
    {
        private readonly ApplicationDbContext _context;

        public CandidateRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Candidate?> GetCandidateByUserIdAsync(int userId)
        {
            return await _context.Candidates
                .Include(c => c.User)
                .Include(c => c.Resumes)
                .Include(c => c.CandidateSkills)
                    .ThenInclude(cs => cs.Skill)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<bool> CandidateExistsAsync(int userId)
        {
            return await _context.Candidates.AnyAsync(c => c.UserId == userId);
        }

        public async Task<Candidate> AddCandidateAsync(Candidate candidate)
        {
            await _context.Candidates.AddAsync(candidate);
            await _context.SaveChangesAsync();
            return candidate;
        }

        public async Task UpdateCandidateAsync(Candidate candidate)
        {
            _context.Candidates.Update(candidate);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCandidateAsync(int userId)
        {
            var candidate = await _context.Candidates.FindAsync(userId);
            if (candidate != null)
            {
                _context.Candidates.Remove(candidate);
                await _context.SaveChangesAsync();
            }
        }
    }
}
