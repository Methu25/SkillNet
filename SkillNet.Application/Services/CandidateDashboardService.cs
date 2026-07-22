using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
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
        private readonly IJobRepository _jobRepository;
        private readonly ICandidateJobMatchingStrategy _matchingStrategy;
        private readonly IConfiguration _configuration;

        public CandidateDashboardService(
            ICandidateService candidateService,
            IResumeService resumeService,
            IJobService jobService,
            IJobRepository jobRepository,
            ICandidateJobMatchingStrategy matchingStrategy,
            IConfiguration configuration)
        {
            _candidateService = candidateService;
            _resumeService = resumeService;
            _jobService = jobService;
            _jobRepository = jobRepository;
            _matchingStrategy = matchingStrategy;
            _configuration = configuration;
        }

        public async Task<CandidateDashboardDto> GetDashboardAsync(int candidateId)
        {
            var candidate = await _candidateService.GetCandidateProfileAsync(candidateId);
            if (candidate == null)
            {
                return CreateFirstTimeDashboard();
            }

            // Fetch resumes sequentially because EF Core DbContext does not support concurrency
            var resumes = (await _resumeService.GetCandidateResumesAsync(candidateId)).ToList();
            var skills = candidate.Skills.ToList();
            var activeResume = resumes.FirstOrDefault(resume => resume.IsActive);

            // Fetch active jobs and required skills efficiently to avoid N+1 queries
            var activeJobs = (await _jobRepository.GetActiveJobsAsync()).ToList();
            var jobSkillsLookup = await _jobRepository.GetActiveJobSkillsAsync();

            var candSkills = skills.Select(s => new SkillInfo { SkillId = s.SkillId, SkillName = s.SkillName }).ToList();
            var matchedJobs = new List<JobResponse>();
            var connStr = _configuration.GetConnectionString("DefaultConnection")!;
            var categories = await JobCategoryService.GetInstance().GetCategoriesAsync(connStr);

            foreach (var job in activeJobs)
            {
                var jobReqSkills = jobSkillsLookup[job.JobId].ToList();
                var matchingInput = new MatchingInput
                {
                    CandidateSkills = candSkills,
                    JobRequiredSkills = jobReqSkills.Select(s => new SkillInfo { SkillId = s.SkillId, SkillName = s.SkillName }).ToList()
                };

                var matchResult = _matchingStrategy.Match(matchingInput);
                var category = categories.FirstOrDefault(c => c.CategoryId == job.CategoryId);

                matchedJobs.Add(new JobResponse
                {
                    JobId = job.JobId,
                    Title = job.Title,
                    Description = job.Description,
                    CategoryId = job.CategoryId,
                    CategoryName = category?.Name ?? "",
                    EmploymentType = job.EmploymentType,
                    WorkMode = job.WorkMode,
                    Location = job.Location,
                    SalaryMin = job.SalaryMin,
                    SalaryMax = job.SalaryMax,
                    ExperienceLevel = job.ExperienceLevel,
                    Status = job.Status,
                    ApplicationDeadline = job.ApplicationDeadline,
                    Skills = jobReqSkills.Select(s => s.SkillName).ToList(),
                    RecruiterId = job.RecruiterId,
                    CreatedAt = job.CreatedAt,

                    // Match Details
                    MatchScore = matchResult.MatchScore,
                    MatchedSkills = matchResult.MatchedSkills,
                    MissingSkills = matchResult.MissingSkills,
                    MatchMethod = matchResult.MatchMethod
                });
            }

            // Sort by MatchScore descending, then CreatedAt/PublishedAt descending
            var recommendedJobs = matchedJobs
                .OrderByDescending(j => j.MatchScore ?? 0)
                .ThenByDescending(j => j.CreatedAt)
                .Take(JobSuggestionCount)
                .ToList();

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
                RecommendedJobs = recommendedJobs
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
