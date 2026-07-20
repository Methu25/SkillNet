using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillNet.Application.Interfaces;
using SkillNet.Domain.Entities;
using SkillNet.Infrastructure.Data;

namespace SkillNet.Infrastructure.Repositories
{
    public class ResumeRepository : IResumeRepository
    {
        private readonly ApplicationDbContext _context;

        public ResumeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Resume>> GetAllResumesByCandidateIdAsync(int candidateId)
        {
            return await _context.Resumes
                .Where(r => r.CandidateId == candidateId)
                .OrderByDescending(r => r.UploadedDate)
                .ToListAsync();
        }

        public async Task<Resume?> GetActiveResumeByCandidateIdAsync(int candidateId)
        {
            return await _context.Resumes
                .Where(r => r.CandidateId == candidateId && r.IsActive)
                .FirstOrDefaultAsync();
        }

        public async Task<Resume?> GetResumeByIdAsync(int resumeId)
        {
            return await _context.Resumes.FindAsync(resumeId);
        }

        public async Task<Resume> AddResumeAsync(Resume resume)
        {
            await _context.Resumes.AddAsync(resume);
            await _context.SaveChangesAsync();
            return resume;
        }

        public async Task UpdateResumeAsync(Resume resume)
        {
            _context.Resumes.Update(resume);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteResumeAsync(int resumeId)
        {
            var resume = await _context.Resumes.FindAsync(resumeId);
            if (resume != null)
            {
                _context.Resumes.Remove(resume);
                await _context.SaveChangesAsync();
            }
        }
    }
}
