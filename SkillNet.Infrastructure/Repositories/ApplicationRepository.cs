using Microsoft.EntityFrameworkCore;
using SkillNet.Application.Interfaces;
using SkillNet.Domain.Entities;
using SkillNet.Infrastructure.Data;

namespace SkillNet.Infrastructure.Repositories
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly ApplicationDbContext _context;

        public ApplicationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<JobApplication> AddApplicationAsync(JobApplication application)
        {
            await _context.Set<JobApplication>().AddAsync(application);
            await _context.SaveChangesAsync();
            return application;
        }

        public async Task<JobApplication?> GetApplicationByIdAsync(int applicationId)
        {
            return await ApplicationQuery()
                .FirstOrDefaultAsync(application => application.ApplicationId == applicationId);
        }

        public async Task<IEnumerable<JobApplication>> GetApplicationsByCandidateIdAsync(
            int candidateId)
        {
            return await ApplicationQuery()
                .AsNoTracking()
                .Where(application => application.CandidateId == candidateId)
                .OrderByDescending(application => application.AppliedDate)
                .ToListAsync();
        }

        public async Task<bool> HasCandidateAppliedAsync(int candidateId, int jobId)
        {
            return await _context.Set<JobApplication>()
                .AsNoTracking()
                .AnyAsync(application =>
                    application.CandidateId == candidateId && application.JobId == jobId);
        }

        public async Task<bool> WithdrawApplicationAsync(JobApplication application)
        {
            _context.Set<JobApplication>().Update(application);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<JobApplication>> GetApplicationsByJobIdAsync(int jobId)
        {
            return await ApplicationQuery()
                .AsNoTracking()
                .Where(application => application.JobId == jobId)
                .OrderByDescending(application => application.AppliedDate)
                .ToListAsync();
        }

        public async Task UpdateApplicationAsync(JobApplication application)
        {
            _context.Set<JobApplication>().Update(application);
            await _context.SaveChangesAsync();
        }

        public async Task<RecruiterNote> AddRecruiterNoteAsync(RecruiterNote recruiterNote)
        {
            await _context.Set<RecruiterNote>().AddAsync(recruiterNote);
            await _context.SaveChangesAsync();
            return recruiterNote;
        }

        public async Task<ApplicationStatusHistory> AddStatusHistoryAsync(
            ApplicationStatusHistory statusHistory)
        {
            await _context.Set<ApplicationStatusHistory>().AddAsync(statusHistory);
            await _context.SaveChangesAsync();
            return statusHistory;
        }

        public async Task<Dictionary<string, int>> GetApplicationStatisticsAsync(
            int recruiterId,
            int? jobId = null)
        {
            var query = _context.Set<JobApplication>()
                .AsNoTracking()
                .Where(application => application.Job.RecruiterId == recruiterId);

            if (jobId.HasValue)
            {
                query = query.Where(application => application.JobId == jobId.Value);
            }

            return await query
                .GroupBy(application => application.CurrentStatus)
                .Select(group => new
                {
                    Status = group.Key,
                    Count = group.Count()
                })
                .ToDictionaryAsync(item => item.Status, item => item.Count);
        }

        private IQueryable<JobApplication> ApplicationQuery()
        {
            return _context.Set<JobApplication>()
                .Include(application => application.Candidate)
                    .ThenInclude(candidate => candidate.User)
                .Include(application => application.Resume)
                .Include(application => application.Job)
                    .ThenInclude(job => job.RecruiterProfile)
                        .ThenInclude(recruiter => recruiter.User)
                .Include(application => application.StatusHistory)
                    .ThenInclude(history => history.ChangedByUser)
                .Include(application => application.RecruiterNotes)
                    .ThenInclude(note => note.Recruiter)
                        .ThenInclude(recruiter => recruiter.User)
                .AsSplitQuery();
        }
    }
}
