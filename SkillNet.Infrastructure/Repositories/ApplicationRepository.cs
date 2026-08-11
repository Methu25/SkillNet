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
            var application = await ApplicationQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(application => application.ApplicationId == applicationId);

            if (application != null)
            {
                await PopulateUserDisplayDataAsync([application]);
            }

            return application;
        }

        public async Task<IEnumerable<JobApplication>> GetApplicationsByCandidateIdAsync(
            int candidateId)
        {
            var applications = await ApplicationQuery()
                .AsNoTracking()
                .Where(application => application.CandidateId == candidateId)
                .OrderByDescending(application => application.AppliedDate)
                .ToListAsync();

            await PopulateUserDisplayDataAsync(applications);
            return applications;
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
            var persistedApplication = new JobApplication
            {
                ApplicationId = application.ApplicationId,
                CurrentStatus = application.CurrentStatus,
                LastUpdated = application.LastUpdated
            };

            _context.Set<JobApplication>().Attach(persistedApplication);
            _context.Entry(persistedApplication)
                .Property(item => item.CurrentStatus)
                .IsModified = true;
            _context.Entry(persistedApplication)
                .Property(item => item.LastUpdated)
                .IsModified = true;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<JobApplication>> GetApplicationsByJobIdAsync(int jobId)
        {
            var applications = await ApplicationQuery()
                .AsNoTracking()
                .Where(application => application.JobId == jobId)
                .OrderByDescending(application => application.AppliedDate)
                .ToListAsync();

            await PopulateUserDisplayDataAsync(applications);
            return applications;
        }

        public async Task UpdateApplicationAsync(JobApplication application)
        {
            var persistedApplication = new JobApplication
            {
                ApplicationId = application.ApplicationId,
                CurrentStatus = application.CurrentStatus,
                LastUpdated = application.LastUpdated
            };

            _context.Set<JobApplication>().Attach(persistedApplication);
            _context.Entry(persistedApplication)
                .Property(item => item.CurrentStatus)
                .IsModified = true;
            _context.Entry(persistedApplication)
                .Property(item => item.LastUpdated)
                .IsModified = true;

            await _context.SaveChangesAsync();
        }

        public async Task<RecruiterNote> AddRecruiterNoteAsync(RecruiterNote recruiterNote)
        {
            var recruiter = recruiterNote.Recruiter;
            recruiterNote.Recruiter = null!;

            await _context.Set<RecruiterNote>().AddAsync(recruiterNote);
            await _context.SaveChangesAsync();

            recruiterNote.Recruiter = recruiter;
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
                .Include(application => application.Resume)
                .Include(application => application.Job)
                    .ThenInclude(job => job.RecruiterProfile)
                .Include(application => application.StatusHistory)
                .Include(application => application.RecruiterNotes)
                    .ThenInclude(note => note.Recruiter)
                .Include(application => application.Interviews)
                .AsSplitQuery();
        }

        private async Task PopulateUserDisplayDataAsync(
            IReadOnlyCollection<JobApplication> applications)
        {
            var userIds = applications
                .SelectMany(application =>
                    new[]
                    {
                        application.Candidate?.UserId,
                        application.Job?.RecruiterProfile?.UserId
                    }
                    .Concat(application.StatusHistory.Select(history => (int?)history.ChangedBy))
                    .Concat(application.RecruiterNotes.Select(note => (int?)note.Recruiter?.UserId)))
                .Where(userId => userId.HasValue)
                .Select(userId => userId!.Value)
                .Distinct()
                .ToList();

            if (userIds.Count == 0)
            {
                return;
            }

            var users = await _context.Users
                .AsNoTracking()
                .Where(user => userIds.Contains(user.UserID))
                .Select(user => new User
                {
                    UserID = user.UserID,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                })
                .ToDictionaryAsync(user => user.UserID);

            foreach (var application in applications)
            {
                if (application.Candidate != null &&
                    users.TryGetValue(application.Candidate.UserId, out var candidateUser))
                {
                    application.Candidate.User = candidateUser;
                }

                var recruiterProfile = application.Job?.RecruiterProfile;
                if (recruiterProfile != null &&
                    users.TryGetValue(recruiterProfile.UserId, out var recruiterUser))
                {
                    recruiterProfile.User = recruiterUser;
                }

                foreach (var history in application.StatusHistory)
                {
                    if (users.TryGetValue(history.ChangedBy, out var changedByUser))
                    {
                        history.ChangedByUser = changedByUser;
                    }
                }

                foreach (var note in application.RecruiterNotes)
                {
                    if (note.Recruiter != null &&
                        users.TryGetValue(note.Recruiter.UserId, out var noteRecruiterUser))
                    {
                        note.Recruiter.User = noteRecruiterUser;
                    }
                }
            }
        }
    }
}
