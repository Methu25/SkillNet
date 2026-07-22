using SkillNet.Application.DTOs;
using SkillNet.Application.Interfaces;
using SkillNet.Domain.Entities;

namespace SkillNet.Application.Services
{
    public class InterviewService : IInterviewService
    {
        private readonly IInterviewRepository _interviewRepository;
        private readonly ICurrentUserContext _currentUser;

        public InterviewService(IInterviewRepository interviewRepository, ICurrentUserContext currentUser)
        {
            _interviewRepository = interviewRepository;
            _currentUser = currentUser;
        }

        public async Task<IEnumerable<InterviewResponse>> GetAllInterviewsAsync()
        {
            var interviews = await _interviewRepository.GetAllInterviewsAsync();
            return interviews.Select(MapToInterviewResponse);
        }

        public async Task<InterviewResponse?> GetInterviewByIdAsync(int id)
        {
            var interview = await _interviewRepository.GetInterviewByIdAsync(id);
            if (interview == null) return null;
            return MapToInterviewResponse(interview);
        }

        public async Task<InterviewResponse> CreateInterviewAsync(CreateInterviewRequest request)
        {
            if (!_currentUser.IsInRole("Recruiter"))
                throw new UnauthorizedAccessException("Only recruiters can schedule interviews.");

            var userId = _currentUser.UserId
                ?? throw new UnauthorizedAccessException("The authenticated user could not be resolved.");
            var context = await _interviewRepository.GetSchedulingContextAsync(request.ApplicationId)
                ?? throw new KeyNotFoundException("Application not found.");

            if (context.RecruiterUserId != userId)
                throw new UnauthorizedAccessException("This application is not owned by the authenticated recruiter.");
            if (!string.Equals(context.CurrentStatus, "Shortlisted", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Only Shortlisted applications can be scheduled for an interview.");

            var scheduledUtc = ValidateRequest(request);
            var interviewerIds = request.InterviewerIds.Where(id => id > 0).Distinct().ToArray();
            if (interviewerIds.Length == 0)
                throw new ArgumentException("At least one eligible interviewer is required.");

            var eligibleInterviewers = (await _interviewRepository.GetEligibleInterviewersAsync()).ToList();
            var eligibleById = eligibleInterviewers.ToDictionary(item => item.InterviewerId);
            var missingIds = interviewerIds.Where(id => !eligibleById.ContainsKey(id)).ToArray();
            if (missingIds.Length > 0)
                throw new KeyNotFoundException($"Eligible interviewer not found: {string.Join(", ", missingIds)}.");

            var interview = new Interview
            {
                ApplicationId = request.ApplicationId,
                InterviewType = request.InterviewType!.Trim(),
                InterviewRound = request.InterviewRound,
                ScheduledDate = scheduledUtc,
                Duration = request.Duration,
                Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim(),
                MeetingLink = string.IsNullOrWhiteSpace(request.MeetingLink) ? null : request.MeetingLink.Trim(),
                Status = "Scheduled",
                CreatedAt = DateTime.UtcNow
            };

            var createdInterview = await _interviewRepository.CreateScheduledInterviewAsync(
                interview, interviewerIds, userId, request.Notes?.Trim());
            var response = MapToInterviewResponse(createdInterview);
            response.AssignedInterviewers = interviewerIds.Select(id => new AssignedInterviewerResponse
            {
                InterviewerId = id,
                Name = eligibleById[id].Name,
                Position = eligibleById[id].Position
            }).ToList();
            return response;
        }

        public Task<IEnumerable<EligibleInterviewerResponse>> GetEligibleInterviewersAsync()
        {
            if (!_currentUser.IsInRole("Recruiter"))
                throw new UnauthorizedAccessException("Only recruiters can view eligible interviewers.");
            return _interviewRepository.GetEligibleInterviewersAsync();
        }

        private static DateTime ValidateRequest(CreateInterviewRequest request)
        {
            if (request.ApplicationId <= 0) throw new ArgumentException("A valid ApplicationId is required.");
            if (request.ScheduledDate == default) throw new ArgumentException("ScheduledDate is required.");
            if (request.ScheduledDate.Kind == DateTimeKind.Unspecified)
                throw new ArgumentException("ScheduledDate must include a UTC offset.");

            var scheduledUtc = request.ScheduledDate.ToUniversalTime();
            if (scheduledUtc <= DateTime.UtcNow) throw new ArgumentException("ScheduledDate must be in the future.");

            var type = request.InterviewType?.Trim();
            var allowedTypes = new[] { "Online", "In-Person", "Phone" };
            if (string.IsNullOrWhiteSpace(type) || !allowedTypes.Contains(type, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException($"InterviewType must be one of: {string.Join(", ", allowedTypes)}.");
            if (request.InterviewRound <= 0) throw new ArgumentException("InterviewRound must be greater than zero.");
            if (request.Duration is < 15 or > 480) throw new ArgumentException("Duration must be between 15 and 480 minutes.");
            if (string.Equals(type, "In-Person", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(request.Location))
                throw new ArgumentException("Location is required for an in-person interview.");
            if (string.Equals(type, "Online", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(request.MeetingLink))
                    throw new ArgumentException("MeetingLink is required for an online interview.");
                if (!Uri.TryCreate(request.MeetingLink, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
                    throw new ArgumentException("MeetingLink must be an absolute HTTPS URL.");
            }
            if (request.Notes?.Length > 2000) throw new ArgumentException("Notes must be 2000 characters or fewer.");
            if (request.Location?.Length > 255) throw new ArgumentException("Location must be 255 characters or fewer.");
            return scheduledUtc;
        }

        public async Task<InterviewResponse?> UpdateInterviewAsync(int id, UpdateInterviewRequest request)
        {
            var interview = new Interview
            {
                ApplicationId = request.ApplicationId,
                InterviewType = request.InterviewType,
                InterviewRound = request.InterviewRound,
                ScheduledDate = request.ScheduledDate,
                Duration = request.Duration,
                Location = request.Location,
                MeetingLink = request.MeetingLink,
                Status = request.Status
            };

            var updatedInterview = await _interviewRepository.UpdateInterviewAsync(id, interview);
            if (updatedInterview == null) return null;
            return MapToInterviewResponse(updatedInterview);
        }

        public async Task<bool> DeleteInterviewAsync(int id)
        {
            return await _interviewRepository.DeleteInterviewAsync(id);
        }

        public async Task<InterviewResponse?> ScheduleInterviewAsync(int id, ScheduleInterviewRequest request)
        {
            var interview = await _interviewRepository.GetInterviewByIdAsync(id);
            if (interview == null) return null;

            interview.ScheduledDate = request.ScheduledDate;
            interview.Duration = request.Duration;
            interview.Location = request.Location;
            interview.MeetingLink = request.MeetingLink;
            interview.Status = "Scheduled";

            var updatedInterview = await _interviewRepository.UpdateInterviewAsync(id, interview);
            if (updatedInterview == null) return null;
            return MapToInterviewResponse(updatedInterview);
        }

        public async Task<InterviewResponse?> RescheduleInterviewAsync(int id, ScheduleInterviewRequest request)
        {
            var interview = await _interviewRepository.GetInterviewByIdAsync(id);
            if (interview == null) return null;

            interview.ScheduledDate = request.ScheduledDate;
            interview.Duration = request.Duration;
            interview.Location = request.Location;
            interview.MeetingLink = request.MeetingLink;
            interview.Status = "Rescheduled";

            var updatedInterview = await _interviewRepository.UpdateInterviewAsync(id, interview);
            if (updatedInterview == null) return null;
            return MapToInterviewResponse(updatedInterview);
        }

        public async Task<InterviewResponse?> CancelInterviewAsync(int id)
        {
            var updatedInterview = await _interviewRepository.UpdateInterviewStatusAsync(id, "Cancelled");
            if (updatedInterview == null) return null;
            return MapToInterviewResponse(updatedInterview);
        }

        public async Task<EvaluationResponse> CreateEvaluationAsync(int interviewId, CreateEvaluationRequest request)
        {
            if (!_currentUser.IsInRole("HiringManager"))
                throw new UnauthorizedAccessException("Only Hiring Managers can submit evaluations.");
            var userId = _currentUser.UserId
                ?? throw new UnauthorizedAccessException("The authenticated user could not be resolved.");
            ValidateEvaluation(request);

            var evaluation = new InterviewEvaluation
            {
                InterviewId = interviewId,
                TechnicalScore = request.TechnicalScore,
                CommunicationScore = request.CommunicationScore,
                ProblemSolvingScore = request.ProblemSolvingScore,
                CultureFitScore = request.CultureFitScore,
                OverallScore = CalculateOverallScore(request.TechnicalScore, request.CommunicationScore, request.ProblemSolvingScore, request.CultureFitScore),
                Recommendation = request.Recommendation.Trim(),
                Comments = request.Comments.Trim(),
                SubmittedAt = DateTime.UtcNow
            };

            var createdEvaluation = await _interviewRepository.CreateEvaluationAndTransitionAsync(evaluation, userId);
            return MapToEvaluationResponse(createdEvaluation);
        }

        public async Task<EvaluationResponse?> GetEvaluationByInterviewIdAsync(int interviewId)
        {
            if (await _interviewRepository.GetAssignedInterviewAsync(interviewId, RequireHiringManager()) == null)
                throw new UnauthorizedAccessException("The Hiring Manager is not assigned to this interview.");
            var evaluation = await _interviewRepository.GetEvaluationByInterviewIdAsync(interviewId);
            if (evaluation == null) return null;
            return MapToEvaluationResponse(evaluation);
        }

        public Task<EvaluationResponse?> UpdateEvaluationAsync(int interviewId, CreateEvaluationRequest request) =>
            throw new InvalidOperationException("Submitted evaluations cannot be edited.");

        public async Task<IEnumerable<InterviewResponse>> GetUpcomingInterviewsAsync()
        {
            var interviews = await _interviewRepository.GetUpcomingInterviewsAsync();
            return interviews.Select(MapToInterviewResponse);
        }

        public async Task<IEnumerable<InterviewResponse>> GetAssignedInterviewsAsync()
        {
            var userId = RequireHiringManager();
            var interviews = await _interviewRepository.GetAssignedInterviewsAsync(userId);
            return interviews.Select(MapToInterviewResponse);
        }

        public async Task<InterviewResponse?> GetAssignedInterviewAsync(int interviewId)
        {
            var interview = await _interviewRepository.GetAssignedInterviewAsync(interviewId, RequireHiringManager());
            return interview == null ? null : MapToInterviewResponse(interview);
        }

        public Task<string> RecordDecisionAsync(int interviewId, InterviewDecisionRequest request)
        {
            var decision = request.Decision?.Trim();
            if (decision is not ("Hired" or "Rejected"))
                throw new ArgumentException("Decision must be Hired or Rejected.");
            return _interviewRepository.RecordDecisionAsync(interviewId, RequireHiringManager(), decision);
        }

        private int RequireHiringManager()
        {
            if (!_currentUser.IsInRole("HiringManager"))
                throw new UnauthorizedAccessException("Only Hiring Managers can access assigned interviews.");
            return _currentUser.UserId
                ?? throw new UnauthorizedAccessException("The authenticated user could not be resolved.");
        }

        private static void ValidateEvaluation(CreateEvaluationRequest request)
        {
            var scores = new[] { request.TechnicalScore, request.CommunicationScore, request.ProblemSolvingScore, request.CultureFitScore };
            if (scores.Any(score => score is < 1 or > 10))
                throw new ArgumentException("Every evaluation score must be between 1 and 10.");
            if (request.Recommendation?.Trim() is not ("Hire" or "Reject"))
                throw new ArgumentException("Recommendation must be Hire or Reject.");
            if (string.IsNullOrWhiteSpace(request.Comments))
                throw new ArgumentException("Comments are required.");
            if (request.Comments.Trim().Length > 2000)
                throw new ArgumentException("Comments must be 2000 characters or fewer.");
        }

        public async Task<HiringDashboardResponse> GetHiringDashboardAsync()
        {
            return await _interviewRepository.GetHiringDashboardAsync();
        }

        public async Task<bool> AssignInterviewerAsync(int interviewId, AssignInterviewerRequest request)
        {
            var interview = await _interviewRepository.GetInterviewByIdAsync(interviewId);
            if (interview == null) return false;

            var assignment = new InterviewAssignment
            {
                InterviewId = interviewId,
                InterviewerId = request.InterviewerId,
                Role = request.Role
            };

            await _interviewRepository.AssignInterviewerAsync(assignment);
            return true;
        }

        private static InterviewResponse MapToInterviewResponse(Interview interview)
        {
            return new InterviewResponse
            {
                InterviewId = interview.InterviewId,
                ApplicationId = interview.ApplicationId,

                CandidateName = interview.CandidateName,
                CandidateEmail = interview.CandidateEmail,
                JobTitle = interview.JobTitle,
                CandidateSummary = interview.CandidateSummary,
                CandidateSkills = interview.CandidateSkills,
                ExperienceYears = interview.ExperienceYears,

                InterviewType = interview.InterviewType,
                InterviewRound = interview.InterviewRound,
                ScheduledDate = interview.ScheduledDate,
                Duration = interview.Duration,
                Location = interview.Location,
                MeetingLink = interview.MeetingLink,
                Status = interview.Status,
                CreatedAt = interview.CreatedAt,
                ApplicationStatus = interview.ApplicationStatus,
                HasEvaluation = interview.HasEvaluation
            };
        }

        private static EvaluationResponse MapToEvaluationResponse(InterviewEvaluation evaluation)
        {
            return new EvaluationResponse
            {
                EvaluationId = evaluation.EvaluationId,
                InterviewId = evaluation.InterviewId,
                InterviewerId = evaluation.InterviewerId,
                TechnicalScore = evaluation.TechnicalScore,
                CommunicationScore = evaluation.CommunicationScore,
                ProblemSolvingScore = evaluation.ProblemSolvingScore,
                CultureFitScore = evaluation.CultureFitScore,
                OverallScore = evaluation.OverallScore,
                Recommendation = evaluation.Recommendation ?? string.Empty,
                Comments = evaluation.Comments ?? string.Empty,
                SubmittedAt = evaluation.SubmittedAt
            };
        }

        private static decimal CalculateOverallScore(int technicalScore, int communicationScore, int problemSolvingScore, int cultureFitScore)
        {
            return Math.Round((technicalScore + communicationScore + problemSolvingScore + cultureFitScore) / 4m, 2, MidpointRounding.AwayFromZero);
        }
    }
}
