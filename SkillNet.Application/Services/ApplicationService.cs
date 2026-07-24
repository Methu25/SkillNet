using SkillNet.Application.DTOs;
using SkillNet.Application.Interfaces;
using SkillNet.Application.Policies;
using SkillNet.Domain.Entities;

namespace SkillNet.Application.Services
{
    public class ApplicationService : IApplicationService
    {
        private const string AppliedStatus = "Applied";
        private const string ShortlistedStatus = "Shortlisted";
        private const string HiredStatus = "Hired";
        private const string RejectedStatus = "Rejected";
        private const string WithdrawnStatus = "Withdrawn";

        private static readonly IReadOnlyDictionary<string, string[]> ValidRecruiterTransitions =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                [AppliedStatus] = [ShortlistedStatus, RejectedStatus],
                [ShortlistedStatus] = [HiredStatus, RejectedStatus]
            };

        private static readonly HashSet<string> TerminalStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            HiredStatus,
            RejectedStatus,
            WithdrawnStatus
        };

        private readonly IApplicationRepository _applicationRepository;
        private readonly IJobRepository _jobRepository;
        private readonly IResumeRepository _resumeRepository;
        private readonly ISystemConfigurationService _systemConfig;
        private readonly ISkillRepository _skillRepository;
        private readonly ICandidateJobMatchingStrategy _matchingStrategy;
        private readonly IApplicationStatusTransitionPolicy _transitionPolicy;

        public ApplicationService(
            IApplicationRepository applicationRepository,
            IJobRepository jobRepository,
            IResumeRepository resumeRepository,
            ISystemConfigurationService systemConfig,
            ISkillRepository skillRepository,
            ICandidateJobMatchingStrategy matchingStrategy,
            IApplicationStatusTransitionPolicy transitionPolicy)
        {
            _applicationRepository = applicationRepository;
            _jobRepository = jobRepository;
            _resumeRepository = resumeRepository;
            _systemConfig = systemConfig;
            _skillRepository = skillRepository;
            _matchingStrategy = matchingStrategy;
            _transitionPolicy = transitionPolicy;
        }

        public async Task<JobApplicationDto> ApplyForJobAsync(
            int candidateId,
            CreateJobApplicationDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            ValidatePositiveId(candidateId, nameof(candidateId));
            ValidatePositiveId(dto.JobId, nameof(dto.JobId));
            ValidatePositiveId(dto.ResumeId, nameof(dto.ResumeId));

            if (await _applicationRepository.HasCandidateAppliedAsync(candidateId, dto.JobId))
            {
                throw new InvalidOperationException("The candidate has already applied for this job.");
            }

            var allowMultiple = _systemConfig.GetBoolSetting("AllowMultipleApplications", false);
            if (!allowMultiple)
            {
                var existingApps = await _applicationRepository.GetApplicationsByCandidateIdAsync(candidateId);
                if (existingApps.Any(a => !TerminalStatuses.Contains(a.CurrentStatus)))
                {
                    throw new InvalidOperationException("You already have an active application. Multiple simultaneous applications are disabled.");
                }
            }

            var job = await _jobRepository.GetJobByIdAsync(dto.JobId);
            if (job == null)
            {
                throw new KeyNotFoundException($"Job {dto.JobId} was not found.");
            }

            if (!string.Equals(job.Status, "Published", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Applications can only be submitted for published jobs.");
            }

            if (job.ApplicationDeadline.HasValue && job.ApplicationDeadline.Value < DateTime.UtcNow)
            {
                throw new InvalidOperationException("The application deadline for this job has passed.");
            }

            var resume = await _resumeRepository.GetResumeByIdAsync(dto.ResumeId);
            if (resume == null)
            {
                throw new KeyNotFoundException($"Resume {dto.ResumeId} was not found.");
            }

            if (resume.CandidateId != candidateId)
            {
                throw new InvalidOperationException("The selected resume does not belong to the candidate.");
            }

            var now = DateTime.UtcNow;
            var application = new JobApplication
            {
                CandidateId = candidateId,
                JobId = job.JobId,
                ResumeId = resume.ResumeId,
                AppliedDate = now,
                CurrentStatus = ApplicationStatusConstants.Applied,
                CoverLetter = NormalizeOptionalValue(dto.CoverLetter),
                LastUpdated = now
            };

            var createdApplication = await _applicationRepository.AddApplicationAsync(application);
            await _applicationRepository.AddStatusHistoryAsync(new ApplicationStatusHistory
            {
                ApplicationId = createdApplication.ApplicationId,
                OldStatus = null,
                NewStatus = ApplicationStatusConstants.Applied,
                ChangedBy = candidateId,
                ChangedAt = now
            });

            var savedApplication = await _applicationRepository
                .GetApplicationByIdAsync(createdApplication.ApplicationId);

            return MapToJobApplicationDto(savedApplication ?? createdApplication);
        }

        public async Task<IEnumerable<JobApplicationSummaryDto>> GetCandidateApplicationsAsync(
            int candidateId)
        {
            ValidatePositiveId(candidateId, nameof(candidateId));
            var applications = await _applicationRepository
                .GetApplicationsByCandidateIdAsync(candidateId);
            return applications.Select(MapToSummaryDto);
        }

        public async Task<JobApplicationDto?> GetCandidateApplicationByIdAsync(
            int candidateId,
            int applicationId)
        {
            ValidatePositiveId(candidateId, nameof(candidateId));
            ValidatePositiveId(applicationId, nameof(applicationId));

            var application = await _applicationRepository.GetApplicationByIdAsync(applicationId);
            return application?.CandidateId == candidateId
                ? MapToJobApplicationDto(application)
                : null;
        }

        public async Task<bool> WithdrawApplicationAsync(
            int candidateId,
            int applicationId,
            WithdrawApplicationDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            ValidatePositiveId(candidateId, nameof(candidateId));
            ValidatePositiveId(applicationId, nameof(applicationId));

            var application = await _applicationRepository.GetApplicationByIdAsync(applicationId);
            if (application == null || application.CandidateId != candidateId)
            {
                return false;
            }

            if (TerminalStatuses.Contains(application.CurrentStatus))
            {
                throw new InvalidOperationException(
                    $"An application with status '{application.CurrentStatus}' cannot be withdrawn.");
            }

            var previousStatus = application.CurrentStatus;
            var changedAt = DateTime.UtcNow;
            application.CurrentStatus = ApplicationStatusConstants.Withdrawn;
            application.LastUpdated = changedAt;

            if (!await _applicationRepository.WithdrawApplicationAsync(application))
            {
                return false;
            }

            await _applicationRepository.AddStatusHistoryAsync(new ApplicationStatusHistory
            {
                ApplicationId = application.ApplicationId,
                OldStatus = previousStatus,
                NewStatus = ApplicationStatusConstants.Withdrawn,
                ChangedBy = candidateId,
                ChangedAt = changedAt,
                Comment = NormalizeOptionalValue(dto.Reason)
            });

            return true;
        }

        public async Task<IEnumerable<JobApplicationSummaryDto>> GetApplicationsForJobAsync(
            int jobId,
            int recruiterId,
            ApplicationSearchRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            ValidatePositiveId(jobId, nameof(jobId));
            ValidatePositiveId(recruiterId, nameof(recruiterId));

            if (request.JobId.HasValue && request.JobId.Value != jobId)
            {
                throw new ArgumentException("The requested job filter does not match the route job.", nameof(request));
            }

            var job = await _jobRepository.GetJobByIdAsync(jobId);
            if (job == null)
            {
                throw new KeyNotFoundException($"Job {jobId} was not found.");
            }

            if (job.RecruiterId != recruiterId)
            {
                throw new InvalidOperationException("The recruiter does not own this job.");
            }

            var applications = await _applicationRepository.GetApplicationsByJobIdAsync(jobId);
            var query = applications.AsEnumerable();

            if (request.CandidateId.HasValue)
            {
                query = query.Where(application => application.CandidateId == request.CandidateId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(application => string.Equals(
                    application.CurrentStatus,
                    request.Status.Trim(),
                    StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim();
                query = query.Where(application =>
                    GetCandidateName(application.Candidate).Contains(
                        searchTerm,
                        StringComparison.OrdinalIgnoreCase));
            }

            // Load required job skills once
            var jobSkills = await _jobRepository.GetSkillIdsByJobIdAsync(jobId);
            var jobSkillsDetails = await _jobRepository.GetSkillsByJobIdAsync(jobId);
            var jobRequiredSkills = new List<SkillInfo>();
            var ids = jobSkills.ToList();
            var names = jobSkillsDetails.ToList();
            for (int i = 0; i < ids.Count; i++)
            {
                jobRequiredSkills.Add(new SkillInfo 
                { 
                    SkillId = ids[i], 
                    SkillName = i < names.Count ? names[i] : string.Empty 
                });
            }

            // Bulk load all applicant candidate skills to prevent N+1 query loops
            var candidateIds = query.Select(a => a.CandidateId).Distinct().ToList();
            var candSkillsLookup = await _skillRepository.GetSkillsByCandidateIdsAsync(candidateIds);

            var mappedList = new List<JobApplicationSummaryDto>();
            foreach (var app in query)
            {
                var candSkills = candSkillsLookup[app.CandidateId]
                    .Select(s => new SkillInfo { SkillId = s.SkillId, SkillName = s.SkillName })
                    .ToList();

                var matchingInput = new MatchingInput
                {
                    CandidateSkills = candSkills,
                    JobRequiredSkills = jobRequiredSkills
                };

                var matchResult = _matchingStrategy.Match(matchingInput);

                var summaryDto = MapToSummaryDto(app);
                summaryDto.MatchScore = matchResult.MatchScore;
                summaryDto.MatchedSkills = matchResult.MatchedSkills;
                summaryDto.MissingSkills = matchResult.MissingSkills;
                summaryDto.MatchMethod = matchResult.MatchMethod;
                summaryDto.MatchedRequiredSkillCount = matchResult.MatchedRequiredSkillCount;
                summaryDto.TotalRequiredSkills = matchResult.TotalRequiredSkills;

                mappedList.Add(summaryDto);
            }

            // Sort: MatchScore descending, AppliedDate descending
            var sortedList = mappedList
                .OrderByDescending(x => x.MatchScore ?? 0)
                .ThenByDescending(x => x.AppliedDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return sortedList;
        }

        public async Task<JobApplicationDto?> GetRecruiterApplicationByIdAsync(
            int recruiterId,
            int applicationId)
        {
            ValidatePositiveId(recruiterId, nameof(recruiterId));
            ValidatePositiveId(applicationId, nameof(applicationId));

            var application = await _applicationRepository.GetApplicationByIdAsync(applicationId);
            if (application == null || !IsOwnedByRecruiter(application, recruiterId))
            {
                return null;
            }

            var dto = MapToJobApplicationDto(application, includeRecruiterNotes: true);

            // Load required job skills
            var jobSkills = await _jobRepository.GetSkillIdsByJobIdAsync(application.JobId);
            var jobSkillsDetails = await _jobRepository.GetSkillsByJobIdAsync(application.JobId);
            var jobRequiredSkills = new List<SkillInfo>();
            var ids = jobSkills.ToList();
            var names = jobSkillsDetails.ToList();
            for (int i = 0; i < ids.Count; i++)
            {
                jobRequiredSkills.Add(new SkillInfo 
                { 
                    SkillId = ids[i], 
                    SkillName = i < names.Count ? names[i] : string.Empty 
                });
            }

            // Load candidate skills
            var candSkillsEntity = await _skillRepository.GetSkillsByCandidateIdAsync(application.CandidateId);
            var candSkills = candSkillsEntity.Select(s => new SkillInfo { SkillId = s.SkillId, SkillName = s.SkillName }).ToList();

            var matchingInput = new MatchingInput
            {
                CandidateSkills = candSkills,
                JobRequiredSkills = jobRequiredSkills
            };

            var matchResult = _matchingStrategy.Match(matchingInput);
            dto.MatchScore = matchResult.MatchScore;
            dto.MatchedSkills = matchResult.MatchedSkills;
            dto.MissingSkills = matchResult.MissingSkills;
            dto.MatchMethod = matchResult.MatchMethod;
            dto.MatchedRequiredSkillCount = matchResult.MatchedRequiredSkillCount;
            dto.TotalRequiredSkills = matchResult.TotalRequiredSkills;

            return dto;
        }

        public async Task<JobApplicationDto?> UpdateApplicationStatusAsync(
            int recruiterId,
            int applicationId,
            UpdateApplicationStatusDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            ValidatePositiveId(recruiterId, nameof(recruiterId));
            ValidatePositiveId(applicationId, nameof(applicationId));

            if (string.IsNullOrWhiteSpace(dto.Status))
            {
                throw new ArgumentException("Application status is required.", nameof(dto));
            }

            // Ownership check — returns null (404) when not found or not owned by this Recruiter.
            var application = await _applicationRepository.GetApplicationByIdAsync(applicationId);
            if (application == null || !IsOwnedByRecruiter(application, recruiterId))
            {
                return null;
            }

            var newStatus = dto.Status.Trim();

            // Validate the requested status is a recognised canonical value.
            if (!_transitionPolicy.IsKnownStatus(newStatus))
            {
                throw new ArgumentException(
                    $"'{newStatus}' is not a recognised application status.", nameof(dto));
            }

            // Recruiters cannot set Withdrawn — that is a candidate-only action.
            if (string.Equals(newStatus, ApplicationStatusConstants.Withdrawn, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Only the candidate can withdraw an application.");
            }

            // Terminal statuses accept no further changes.
            if (TerminalStatuses.Contains(application.CurrentStatus))
            {
                throw new InvalidOperationException(
                    $"An application with status '{application.CurrentStatus}' cannot be changed.");
            }

            // Idempotent: same status requested — return existing application without writing.
            if (string.Equals(application.CurrentStatus, newStatus, StringComparison.OrdinalIgnoreCase))
            {
                var existingApplication = await _applicationRepository.GetApplicationByIdAsync(applicationId);
                return MapToJobApplicationDto(existingApplication ?? application);
            }

            // Enforce legal transition graph.
            if (!_transitionPolicy.CanRecruiterTransition(application.CurrentStatus, newStatus))
            {
                throw new InvalidOperationException(
                    $"Transitioning from '{application.CurrentStatus}' to '{newStatus}' is not permitted.");
            }

            var validNextStatuses = GetValidNextStatuses(application.CurrentStatus);
            var canonicalStatus = validNextStatuses.FirstOrDefault(status =>
                string.Equals(status, newStatus, StringComparison.OrdinalIgnoreCase));
            if (canonicalStatus == null)
            {
                throw new InvalidOperationException(
                    $"The status cannot change from '{application.CurrentStatus}' to '{newStatus}'.");
            }

            newStatus = canonicalStatus;

            var previousStatus = application.CurrentStatus;
            var changedAt = DateTime.UtcNow;
            application.CurrentStatus = newStatus;
            application.LastUpdated = changedAt;

            await _applicationRepository.UpdateApplicationAsync(application);
            await _applicationRepository.AddStatusHistoryAsync(new ApplicationStatusHistory
            {
                ApplicationId = application.ApplicationId,
                OldStatus = previousStatus,
                NewStatus = newStatus,
                ChangedBy = application.Job.RecruiterProfile.UserId,
                ChangedAt = changedAt,
                Comment = NormalizeOptionalValue(dto.Comment)
            });

            var updatedApplication = await _applicationRepository.GetApplicationByIdAsync(applicationId);
            return MapToJobApplicationDto(updatedApplication ?? application, includeRecruiterNotes: true);
        }

        public async Task<RecruiterNoteDto?> AddRecruiterNoteAsync(
            int recruiterId,
            int applicationId,
            AddRecruiterNoteDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            ValidatePositiveId(recruiterId, nameof(recruiterId));
            ValidatePositiveId(applicationId, nameof(applicationId));

            if (string.IsNullOrWhiteSpace(dto.Comment))
            {
                throw new ArgumentException("Recruiter note comment is required.", nameof(dto));
            }

            var application = await _applicationRepository.GetApplicationByIdAsync(applicationId);
            if (application == null || !IsOwnedByRecruiter(application, recruiterId))
            {
                return null;
            }

            var recruiterNote = new RecruiterNote
            {
                ApplicationId = application.ApplicationId,
                RecruiterId = recruiterId,
                Comment = dto.Comment.Trim(),
                CreatedAt = DateTime.UtcNow,
                Recruiter = application.Job.RecruiterProfile
            };

            var createdNote = await _applicationRepository.AddRecruiterNoteAsync(recruiterNote);
            return MapToRecruiterNoteDto(createdNote);
        }

        public async Task<ApplicationStatisticsDto> GetApplicationStatisticsAsync(
            int recruiterId,
            int? jobId = null)
        {
            ValidatePositiveId(recruiterId, nameof(recruiterId));
            if (jobId.HasValue)
            {
                ValidatePositiveId(jobId.Value, nameof(jobId));
                var job = await _jobRepository.GetJobByIdAsync(jobId.Value);
                if (job == null)
                {
                    throw new KeyNotFoundException($"Job {jobId.Value} was not found.");
                }

                if (job.RecruiterId != recruiterId)
                {
                    throw new InvalidOperationException("The recruiter does not own this job.");
                }
            }

            var counts = await _applicationRepository
                .GetApplicationStatisticsAsync(recruiterId, jobId);

            return new ApplicationStatisticsDto
            {
                Total = counts.Values.Sum(),
                ByStatus = new Dictionary<string, int>(counts, StringComparer.OrdinalIgnoreCase)
            };
        }

        private static JobApplicationDto MapToJobApplicationDto(
            JobApplication application,
            bool includeRecruiterNotes = false)
        {
            var activeInterview = application.Interviews?
                .Where(i => i.Status == "Scheduled" || i.Status == "Rescheduled" || i.Status == "Interviewing" || i.Status == "EvaluationSubmitted")
                .OrderByDescending(i => i.ScheduledDate)
                .FirstOrDefault() ?? application.Interviews?.OrderByDescending(i => i.ScheduledDate).FirstOrDefault();

            return new JobApplicationDto
            {
                ApplicationId = application.ApplicationId,
                CandidateId = application.CandidateId,
                JobId = application.JobId,
                ResumeId = application.ResumeId,
                AppliedDate = application.AppliedDate,
                CurrentStatus = application.CurrentStatus,
                CoverLetter = application.CoverLetter,
                Source = application.Source,
                LastUpdated = application.LastUpdated,
                CandidateName = GetCandidateName(application.Candidate),
                CandidateEmail = application.Candidate?.User?.Email ?? string.Empty,
                CandidateProfessionalTitle = application.Candidate?.ProfessionalTitle,
                JobTitle = application.Job?.Title ?? string.Empty,
                JobStatus = application.Job?.Status ?? string.Empty,
                ResumeFileName = application.Resume?.FileName ?? string.Empty,
                ResumeFileType = application.Resume?.FileType ?? string.Empty,
                ResumeFileSize = application.Resume?.FileSize ?? 0,
                ResumeUploadedDate = application.Resume?.UploadedDate ?? default,
                StatusHistory = application.StatusHistory
                    .OrderBy(history => history.ChangedAt)
                    .Select(MapToHistoryDto)
                    .ToList(),
                ValidNextStatuses = GetValidNextStatuses(application.CurrentStatus).ToList(),
                RecruiterNotes = includeRecruiterNotes
                    ? application.RecruiterNotes
                        .OrderByDescending(note => note.CreatedAt)
                        .Select(MapToRecruiterNoteDto)
                        .ToList()
                    : null,
                ScheduledInterview = activeInterview == null ? null : new ScheduledInterviewDto
                {
                    InterviewId = activeInterview.InterviewId,
                    InterviewType = activeInterview.InterviewType,
                    InterviewRound = activeInterview.InterviewRound,
                    ScheduledDate = activeInterview.ScheduledDate,
                    Duration = activeInterview.Duration,
                    Location = activeInterview.Location,
                    MeetingLink = activeInterview.MeetingLink,
                    Status = activeInterview.Status
                }
            };
        }

        private static JobApplicationSummaryDto MapToSummaryDto(JobApplication application)
        {
            return new JobApplicationSummaryDto
            {
                ApplicationId = application.ApplicationId,
                CandidateId = application.CandidateId,
                CandidateName = GetCandidateName(application.Candidate),
                JobId = application.JobId,
                JobTitle = application.Job?.Title ?? string.Empty,
                ResumeId = application.ResumeId,
                CurrentStatus = application.CurrentStatus,
                AppliedDate = application.AppliedDate,
                LastUpdated = application.LastUpdated
            };
        }

        private static ApplicationStatusHistoryDto MapToHistoryDto(
            ApplicationStatusHistory history)
        {
            return new ApplicationStatusHistoryDto
            {
                StatusHistoryId = history.StatusHistoryId,
                ApplicationId = history.ApplicationId,
                OldStatus = history.OldStatus,
                NewStatus = history.NewStatus,
                ChangedBy = history.ChangedBy,
                ChangedByName = GetUserName(history.ChangedByUser),
                ChangedByEmail = history.ChangedByUser?.Email ?? string.Empty,
                ChangedAt = history.ChangedAt,
                Comment = history.Comment
            };
        }

        private static RecruiterNoteDto MapToRecruiterNoteDto(RecruiterNote recruiterNote)
        {
            return new RecruiterNoteDto
            {
                NoteId = recruiterNote.NoteId,
                ApplicationId = recruiterNote.ApplicationId,
                RecruiterId = recruiterNote.RecruiterId,
                RecruiterName = GetUserName(recruiterNote.Recruiter?.User),
                RecruiterEmail = recruiterNote.Recruiter?.User?.Email ?? string.Empty,
                Comment = recruiterNote.Comment,
                CreatedAt = recruiterNote.CreatedAt
            };
        }

        private static bool IsOwnedByRecruiter(JobApplication application, int recruiterId)
        {
            return application.Job?.RecruiterId == recruiterId;
        }

        private static IReadOnlyCollection<string> GetValidNextStatuses(string currentStatus)
        {
            return ValidRecruiterTransitions.TryGetValue(currentStatus, out var statuses)
                ? statuses
                : Array.Empty<string>();
        }

        private static string GetCandidateName(Candidate? candidate)
        {
            return candidate == null
                ? string.Empty
                : $"{candidate.FirstName} {candidate.LastName}".Trim();
        }

        private static string GetUserName(User? user)
        {
            return user == null
                ? string.Empty
                : $"{user.FirstName} {user.LastName}".Trim();
        }

        private static string? NormalizeOptionalValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static void ValidatePositiveId(int value, string parameterName)
        {
            if (value <= 0)
            {
                throw new ArgumentException("The identifier must be greater than zero.", parameterName);
            }
        }
    }
}
