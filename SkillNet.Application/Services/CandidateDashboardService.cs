using SkillNet.Application.DTOs;
using SkillNet.Application.Interfaces;

namespace SkillNet.Application.Services
{
    public class CandidateDashboardService : ICandidateDashboardService
    {
        private const int JobSuggestionCount = 5;

        private readonly ICandidateService _candidateService;
        private readonly IResumeService _resumeService;
        private readonly IJobService _jobService;

        public CandidateDashboardService(
            ICandidateService candidateService,
            IResumeService resumeService,
            IJobService jobService)
        {
            _candidateService = candidateService;
            _resumeService = resumeService;
            _jobService = jobService;
        }

        public async Task<CandidateDashboardDto> GetDashboardAsync(int candidateId)
        {
            var candidate = await _candidateService.GetCandidateProfileAsync(candidateId);
            if (candidate == null)
            {
                return CreateFirstTimeDashboard();
            }

            var resumesTask = _resumeService.GetCandidateResumesAsync(candidateId);
            var jobsTask = _jobService.SearchJobsAsync(new JobSearchRequest
            {
                SortBy = "newest",
                Page = 1,
                PageSize = JobSuggestionCount
            });

            await Task.WhenAll(resumesTask, jobsTask);

            var resumes = (await resumesTask).ToList();
            var skills = candidate.Skills.ToList();
            var activeResume = resumes.FirstOrDefault(resume => resume.IsActive);

            return new CandidateDashboardDto
            {
                HasProfile = true,
                IsFirstTimeUser = false,
                Profile = MapToProfileSummary(candidate),
                ProfileCompletion = candidate.ProfileCompletion,
                TotalResumes = resumes.Count,
                HasActiveResume = activeResume != null,
                ActiveResume = activeResume,
                LatestResume = resumes
                    .OrderByDescending(resume => resume.UploadedDate)
                    .FirstOrDefault(),
                TotalSkills = skills.Count,
                Skills = skills,
                RecommendedJobs = (await jobsTask).ToList()
            };
        }

        private static CandidateDashboardDto CreateFirstTimeDashboard()
        {
            return new CandidateDashboardDto
            {
                HasProfile = false,
                IsFirstTimeUser = true,
                WelcomeMessage = "Welcome to SkillNet. Create your profile to start your professional journey.",
                ProfileCompletion = new ProfileCompletionResultDto
                {
                    CompletionPercentage = 0,
                    CompletionLevel = 0,
                    IsComplete = false
                }
            };
        }

        private static CandidateProfileSummaryDto MapToProfileSummary(CandidateProfileDto candidate)
        {
            return new CandidateProfileSummaryDto
            {
                UserId = candidate.UserId,
                FullName = $"{candidate.FirstName} {candidate.LastName}".Trim(),
                ProfessionalTitle = candidate.ProfessionalTitle,
                ProfessionalSummary = candidate.ProfessionalSummary,
                Education = candidate.Education,
                Degree = candidate.Degree,
                Location = candidate.Location,
                ExperienceYears = candidate.ExperienceYears,
                ProfileImagePath = candidate.ProfileImagePath,
                IsProfileCompleted = candidate.ProfileCompletion.IsComplete,
                ProfileCompletionPercentage = candidate.ProfileCompletion.CompletionPercentage,
                ProfileCompletionLevel = candidate.ProfileCompletion.CompletionLevel
            };
        }
    }
}
