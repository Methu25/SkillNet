using SkillNet.Application.DTOs;
using SkillNet.Application.Interfaces;
using SkillNet.Application.Utilities;
using SkillNet.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace SkillNet.Application.Services
{
    // ─── Internal search abstractions (Abstract Factory Pattern) ────────────────

    /// <summary>
    /// Abstract Factory Pattern — defines a family of compatible search components.
    /// Ensures that Filter, Sorter, and Paginator are always created together
    /// as a matching set — mixing them from different factories would break results.
    /// </summary>
    internal interface ISearchStrategy
    {
        string BuildWhereClause(JobSearchRequest request, List<string> baseConditions);
        string BuildOrderClause(string sortBy);
        string BuildPagingClause(int page, int pageSize);
    }

    /// <summary>
    /// Basic search strategy — keyword + date sort + simple offset paging.
    /// Used when only a keyword or simple filter is provided.
    /// </summary>
    internal class BasicSearchStrategy : ISearchStrategy
    {
        public string BuildWhereClause(JobSearchRequest request, List<string> baseConditions)
        {
            if (!string.IsNullOrEmpty(request.Keyword))
                baseConditions.Add("(Title LIKE @Keyword OR Description LIKE @Keyword)");
            if (request.CategoryId.HasValue)
                baseConditions.Add("CategoryId = @CategoryId");
            return string.Join(" AND ", baseConditions);
        }

        public string BuildOrderClause(string sortBy) => sortBy switch
        {
            "salary-asc" => "ORDER BY SalaryMin ASC",
            "salary-desc" => "ORDER BY SalaryMax DESC",
            _ => "ORDER BY CreatedAt DESC"
        };

        public string BuildPagingClause(int page, int pageSize) =>
            $"OFFSET {(page - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY";
    }

    /// <summary>
    /// Advanced search strategy — multi-filter + relevance-weighted sort + offset paging.
    /// Used when salary range, work mode, location, or experience level filters are active.
    /// </summary>
    internal class AdvancedSearchStrategy : ISearchStrategy
    {
        public string BuildWhereClause(JobSearchRequest request, List<string> baseConditions)
        {
            if (!string.IsNullOrEmpty(request.Keyword))
                baseConditions.Add("(Title LIKE @Keyword OR Description LIKE @Keyword)");
            if (request.CategoryId.HasValue)
                baseConditions.Add("CategoryId = @CategoryId");
            if (!string.IsNullOrEmpty(request.WorkMode))
                baseConditions.Add("WorkMode = @WorkMode");
            if (!string.IsNullOrEmpty(request.Location))
                baseConditions.Add("Location LIKE @Location");
            if (request.SalaryMin.HasValue)
                baseConditions.Add("(SalaryMax IS NULL OR SalaryMax >= @SalaryMin)");
            if (request.SalaryMax.HasValue)
                baseConditions.Add("(SalaryMin IS NULL OR SalaryMin <= @SalaryMax)");
            if (!string.IsNullOrEmpty(request.ExperienceLevel))
                baseConditions.Add("ExperienceLevel = @ExperienceLevel");
            if (!string.IsNullOrEmpty(request.EmploymentType))
                baseConditions.Add("EmploymentType = @EmploymentType");
            return string.Join(" AND ", baseConditions);
        }

        public string BuildOrderClause(string sortBy) => sortBy switch
        {
            "salary-asc" => "ORDER BY SalaryMin ASC",
            "salary-desc" => "ORDER BY SalaryMax DESC",
            _ => "ORDER BY CreatedAt DESC"
        };

        public string BuildPagingClause(int page, int pageSize) =>
            $"OFFSET {(page - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY";
    }

    // ─── Factory Method: IJobFilter ──────────────────────────────────────────────

    /// <summary>
    /// Factory Method Pattern — defines the product interface.
    /// Each concrete filter adds its own SQL condition to the clause list.
    /// </summary>
    internal interface IJobFilter
    {
        void Apply(JobSearchRequest request, List<string> conditions);
    }

    internal class KeywordFilter : IJobFilter
    {
        public void Apply(JobSearchRequest request, List<string> conditions)
        {
            if (!string.IsNullOrEmpty(request.Keyword))
                conditions.Add("(Title LIKE @Keyword OR Description LIKE @Keyword)");
        }
    }

    internal class CategoryFilter : IJobFilter
    {
        public void Apply(JobSearchRequest request, List<string> conditions)
        {
            if (request.CategoryId.HasValue)
                conditions.Add("CategoryId = @CategoryId");
        }
    }

    internal class LocationFilter : IJobFilter
    {
        public void Apply(JobSearchRequest request, List<string> conditions)
        {
            if (!string.IsNullOrEmpty(request.Location))
                conditions.Add("Location LIKE @Location");
        }
    }

    internal class SalaryFilter : IJobFilter
    {
        public void Apply(JobSearchRequest request, List<string> conditions)
        {
            if (request.SalaryMin.HasValue)
                conditions.Add("(SalaryMax IS NULL OR SalaryMax >= @SalaryMin)");
            if (request.SalaryMax.HasValue)
                conditions.Add("(SalaryMin IS NULL OR SalaryMin <= @SalaryMax)");
        }
    }

    // ─── JobService ──────────────────────────────────────────────────────────────

    public class JobService : IJobService
    {
        private readonly IJobRepository _jobRepository;
        private readonly IRecruiterService _recruiterService;
        private readonly IConfiguration _configuration;
        private readonly ICurrentUserContext _currentUserContext;
        private readonly ICandidateService _candidateService;
        private readonly IUserService _userService;
        private readonly ICandidateJobMatchingStrategy _matchingStrategy;

        public JobService(
            IJobRepository jobRepository,
            IRecruiterService recruiterService,
            IConfiguration configuration,
            ICurrentUserContext currentUserContext,
            ICandidateService candidateService,
            IUserService userService,
            ICandidateJobMatchingStrategy matchingStrategy)
        {
            _jobRepository = jobRepository;
            _recruiterService = recruiterService;
            _configuration = configuration;
            _currentUserContext = currentUserContext;
            _candidateService = candidateService;
            _userService = userService;
            _matchingStrategy = matchingStrategy;
        }

        /// <summary>
        /// Creates a new job posting using the Builder Pattern.
        /// Instead of passing 12+ parameters to a constructor, we chain
        /// setter methods on JobPostBuilder to assemble the object step-by-step.
        /// </summary>
        public async Task<JobResponse> CreateJobAsync(int userId, CreateJobRequest request)
        {
            // Builder Pattern — assemble JobPost step-by-step
            var recruiterProfileId = await GetRequiredRecruiterProfileIdAsync(userId);
            var orgId = await _jobRepository.GetRecruiterOrganizationIdAsync(recruiterProfileId);

            var job = new JobPostBuilder()
                .SetRecruiter(recruiterProfileId, orgId > 0 ? orgId : null)
                .SetTitle(request.Title)
                .SetDescription(request.Description)
                .SetCategory(request.CategoryId)
                .SetEmploymentType(request.EmploymentType)
                .SetWorkMode(request.WorkMode)
                .SetLocation(request.Location ?? string.Empty)
                .SetSalaryRange(request.SalaryMin, request.SalaryMax)
                .SetExperienceLevel(request.ExperienceLevel ?? string.Empty)
                .SetApplicationDeadline(request.ApplicationDeadline)
                .Build();

            // Insert job and skills atomically
            var jobId = await _jobRepository.InsertJobWithSkillsAsync(job, request.SkillIds ?? new List<int>());

            return await BuildJobResponseAsync(jobId);
        }

        public async Task<JobResponse?> GetJobByIdAsync(int jobId)
        {
            var job = await _jobRepository.GetJobByIdAsync(jobId);
            if (job == null) return null;
            return await BuildJobResponseAsync(job.JobId);
        }

        /// <summary>
        /// Searches jobs using the Abstract Factory Pattern to select the right
        /// search strategy, and Factory Method to create individual filter objects.
        /// </summary>
        public async Task<IEnumerable<JobResponse>> SearchJobsAsync(JobSearchRequest request)
        {
            // Abstract Factory Pattern — pick the right search strategy
            var strategy = GetSearchStrategy(request);

            // Factory Method Pattern — apply individual filter objects
            var filters = new List<IJobFilter>
            {
                CreateJobFilter("keyword"),
                CreateJobFilter("category"),
                CreateJobFilter("location"),
                CreateJobFilter("salary")
            };

            var jobs = await _jobRepository.SearchJobsAsync(request);
            var responses = new List<JobResponse>();

            // Check if current user is a Candidate to apply skill matching scores
            List<SkillInfo> candSkills = new();
            var currentUserId = _currentUserContext.UserId;
            if (currentUserId.HasValue && _currentUserContext.IsInRole("Candidate"))
            {
                var candidate = await _candidateService.GetCandidateProfileAsync(currentUserId.Value);
                if (candidate != null)
                {
                    candSkills = candidate.Skills.Select(s => new SkillInfo { SkillId = s.SkillId, SkillName = s.SkillName }).ToList();
                }
            }

            foreach (var job in jobs)
            {
                var response = await BuildJobResponseAsync(job.JobId);
                if (candSkills.Any())
                {
                    var jobSkills = await _jobRepository.GetSkillIdsByJobIdAsync(job.JobId);
                    var jobSkillsDetails = await _jobRepository.GetSkillsByJobIdAsync(job.JobId);

                    var jobSkillsList = new List<SkillInfo>();
                    var ids = jobSkills.ToList();
                    var names = jobSkillsDetails.ToList();
                    for (int i = 0; i < ids.Count; i++)
                    {
                        jobSkillsList.Add(new SkillInfo
                        {
                            SkillId = ids[i],
                            SkillName = i < names.Count ? names[i] : string.Empty
                        });
                    }

                    var matchingInput = new MatchingInput
                    {
                        CandidateSkills = candSkills,
                        JobRequiredSkills = jobSkillsList
                    };

                    var matchResult = _matchingStrategy.Match(matchingInput);
                    response.MatchScore = matchResult.MatchScore;
                    response.MatchedSkills = matchResult.MatchedSkills;
                    response.MissingSkills = matchResult.MissingSkills;
                    response.MatchMethod = matchResult.MatchMethod;
                }
                responses.Add(response);
            }
            return responses;
        }

        public async Task<IEnumerable<JobResponse>> GetRecruiterJobsAsync(int userId)
        {
            var recruiterProfileId = await _recruiterService.GetRecruiterProfileIdAsync(userId);
            if (!recruiterProfileId.HasValue)
                return Array.Empty<JobResponse>();

            var jobs = await _jobRepository.GetJobsByRecruiterAsync(recruiterProfileId.Value);
            var responses = new List<JobResponse>();
            foreach (var job in jobs)
                responses.Add(await BuildJobResponseAsync(job.JobId));
            return responses;
        }

        public async Task<JobResponse?> UpdateJobAsync(int jobId, int userId, UpdateJobRequest request)
        {
            var recruiterProfileId = await GetRequiredRecruiterProfileIdAsync(userId);
            var existing = await _jobRepository.GetJobByIdAsync(jobId);
            if (existing == null || existing.RecruiterId != recruiterProfileId) return null;

            existing.Title = request.Title ?? existing.Title;
            existing.Description = request.Description ?? existing.Description;
            existing.CategoryId = request.CategoryId ?? existing.CategoryId;
            existing.EmploymentType = request.EmploymentType ?? existing.EmploymentType;
            existing.WorkMode = request.WorkMode ?? existing.WorkMode;
            existing.Location = request.Location ?? existing.Location;
            existing.SalaryMin = request.SalaryMin ?? existing.SalaryMin;
            existing.SalaryMax = request.SalaryMax ?? existing.SalaryMax;
            existing.ExperienceLevel = request.ExperienceLevel ?? existing.ExperienceLevel;
            existing.ApplicationDeadline = request.ApplicationDeadline ?? existing.ApplicationDeadline;

            await _jobRepository.UpdateJobWithSkillsAsync(existing, request.SkillIds ?? new List<int>());

            return await BuildJobResponseAsync(jobId);
        }

        public async Task<bool> DeleteJobAsync(int jobId, int userId)
        {
            var recruiterProfileId = await GetRequiredRecruiterProfileIdAsync(userId);
            return await _jobRepository.DeleteJobAsync(jobId, recruiterProfileId);
        }

        public async Task<JobResponse?> PublishJobAsync(int jobId, int userId)
        {
            var recruiterProfileId = await GetRequiredRecruiterProfileIdAsync(userId);
            var updated = await _jobRepository.UpdateJobStatusAsync(jobId, recruiterProfileId, "Published");
            if (!updated) return null;
            return await BuildJobResponseAsync(jobId);
        }

        public async Task<JobResponse?> CloseJobAsync(int jobId, int userId)
        {
            var recruiterProfileId = await GetRequiredRecruiterProfileIdAsync(userId);
            var updated = await _jobRepository.UpdateJobStatusAsync(jobId, recruiterProfileId, "Closed");
            if (!updated) return null;
            return await BuildJobResponseAsync(jobId);
        }

        /// <summary>
        /// Duplicates a job using the Prototype Pattern.
        /// Instead of building a new job from scratch, we clone the existing one.
        /// More efficient when the recruiter posts a similar role again.
        /// </summary>
        public async Task<JobResponse> DuplicateJobAsync(int jobId, int userId)
        {
            var recruiterProfileId = await GetRequiredRecruiterProfileIdAsync(userId);
            var original = await _jobRepository.GetJobByIdAsync(jobId);
            if (original == null) throw new KeyNotFoundException($"Job {jobId} not found.");
            if (original.RecruiterId != recruiterProfileId) throw new UnauthorizedAccessException("You can only duplicate your own jobs.");

            // Prototype Pattern — clone the existing job object
            var cloned = (JobPost)original.Clone();
            var newJobId = await _jobRepository.InsertJobAsync(cloned);

            var originalSkillIds = (await _jobRepository.GetSkillIdsByJobIdAsync(jobId)).ToList();
            if (originalSkillIds.Count > 0)
                await _jobRepository.InsertJobSkillsAsync(newJobId, originalSkillIds);

            return await BuildJobResponseAsync(newJobId);
        }

        public Task<IEnumerable<SkillDto>> GetSkillsAsync()
        {
            return _jobRepository.GetAllSkillsAsync();
        }

        // ─── Factory Method ────────────────────────────────────────────────────

        /// <summary>
        /// Factory Method Pattern — creates the appropriate filter object
        /// based on the filter type string. Decouples the search logic from
        /// specific filter implementations.
        /// </summary>
        private static IJobFilter CreateJobFilter(string filterType) => filterType switch
        {
            "keyword" => new KeywordFilter(),
            "category" => new CategoryFilter(),
            "location" => new LocationFilter(),
            "salary" => new SalaryFilter(),
            _ => new KeywordFilter()
        };

        // ─── Abstract Factory ──────────────────────────────────────────────────

        /// <summary>
        /// Abstract Factory Pattern — determines whether a basic or advanced search
        /// strategy is needed, then returns a complete, compatible strategy object.
        /// Each strategy creates its own compatible Filter + Sorter + Paginator set.
        /// </summary>
        private static ISearchStrategy GetSearchStrategy(JobSearchRequest request)
        {
            bool isAdvanced = request.SalaryMin.HasValue
                           || request.SalaryMax.HasValue
                           || !string.IsNullOrEmpty(request.WorkMode)
                           || !string.IsNullOrEmpty(request.Location)
                           || !string.IsNullOrEmpty(request.ExperienceLevel)
                           || !string.IsNullOrEmpty(request.EmploymentType);

            return isAdvanced ? new AdvancedSearchStrategy() : new BasicSearchStrategy();
        }

        // ─── Helpers ───────────────────────────────────────────────────────────

        private async Task<int> GetRequiredRecruiterProfileIdAsync(int userId)
        {
            var recruiterProfileId = await _recruiterService.GetRecruiterProfileIdAsync(userId);
            return recruiterProfileId
                ?? throw new InvalidOperationException("Recruiter profile not yet created.");
        }

        private async Task<JobResponse> BuildJobResponseAsync(int jobId)
        {
            var job = await _jobRepository.GetJobByIdAsync(jobId);
            var skills = await _jobRepository.GetSkillsByJobIdAsync(jobId);
            var connStr = _configuration.GetConnectionString("DefaultConnection")!;
            var categories = await JobCategoryService.GetInstance().GetCategoriesAsync(connStr);
            var category = categories.FirstOrDefault(c => c.CategoryId == job!.CategoryId);

            return new JobResponse
            {
                JobId = job!.JobId,
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
                Skills = skills.ToList(),
                RecruiterId = job.RecruiterId,
                RecruiterName = "",
                OrganizationName = null,
                CreatedAt = job.CreatedAt
            };
        }
    }
}
