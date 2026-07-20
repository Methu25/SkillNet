using SkillNet.Application.DTOs;
using SkillNet.Application.Interfaces;
using SkillNet.Domain.Entities;

namespace SkillNet.Application.Services
{
    public class ApplicationService : IApplicationService
    {
        private const string AppliedStatus = "Applied";
        private const string WithdrawnStatus = "Withdrawn";

        private static readonly HashSet<string> TerminalStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "Hired",
            "Rejected",
            WithdrawnStatus
        };

        private readonly IApplicationRepository _applicationRepository;
        private readonly IJobRepository _jobRepository;
        private readonly IResumeRepository _resumeRepository;

        public ApplicationService(
            IApplicationRepository applicationRepository,
            IJobRepository jobRepository,
            IResumeRepository resumeRepository)
        {
            _applicationRepository = applicationRepository;
            _jobRepository = jobRepository;
            _resumeRepository = resumeRepository;
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
                CurrentStatus = AppliedStatus,
                CoverLetter = NormalizeOptionalValue(dto.CoverLetter),
                LastUpdated = now
            };

            var createdApplication = await _applicationRepository.AddApplicationAsync(application);
            await _applicationRepository.AddStatusHistoryAsync(new ApplicationStatusHistory
            {
                ApplicationId = createdApplication.ApplicationId,
                OldStatus = null,
                NewStatus = AppliedStatus,
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
            application.CurrentStatus = WithdrawnStatus;
            application.LastUpdated = changedAt;

            if (!await _applicationRepository.WithdrawApplicationAsync(application))
            {
                return false;
            }

            await _applicationRepository.AddStatusHistoryAsync(new ApplicationStatusHistory
            {
                ApplicationId = application.ApplicationId,
                OldStatus = previousStatus,
                NewStatus = WithdrawnStatus,
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

            query = query
                .OrderByDescending(application => application.AppliedDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize);

            return query.Select(MapToSummaryDto).ToList();
        }

        public async Task<JobApplicationDto?> GetRecruiterApplicationByIdAsync(
            int recruiterId,
            int applicationId)
        {
            ValidatePositiveId(recruiterId, nameof(recruiterId));
            ValidatePositiveId(applicationId, nameof(applicationId));

            var application = await _applicationRepository.GetApplicationByIdAsync(applicationId);
            return application != null && IsOwnedByRecruiter(application, recruiterId)
                ? MapToJobApplicationDto(application)
                : null;
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

            var application = await _applicationRepository.GetApplicationByIdAsync(applicationId);
            if (application == null || !IsOwnedByRecruiter(application, recruiterId))
            {
                return null;
            }

            if (TerminalStatuses.Contains(application.CurrentStatus))
            {
                throw new InvalidOperationException(
                    $"An application with status '{application.CurrentStatus}' cannot be changed.");
            }

            var newStatus = dto.Status.Trim();
            if (string.Equals(newStatus, WithdrawnStatus, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Only the candidate can withdraw an application.");
            }

            if (string.Equals(application.CurrentStatus, newStatus, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("The application already has the requested status.");
            }

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
            return MapToJobApplicationDto(updatedApplication ?? application);
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

        private static JobApplicationDto MapToJobApplicationDto(JobApplication application)
        {
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
                RecruiterNotes = application.RecruiterNotes
                    .OrderByDescending(note => note.CreatedAt)
                    .Select(MapToRecruiterNoteDto)
                    .ToList()
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
