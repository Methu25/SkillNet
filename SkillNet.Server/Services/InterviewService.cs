using SkillNet.Server.DTOs;
using SkillNet.Server.Interfaces;
using SkillNet.Server.Models;

namespace SkillNet.Server.Services
{
    public class InterviewService : IInterviewService
    {
        private readonly IInterviewRepository _interviewRepository;

        public InterviewService(IInterviewRepository interviewRepository)
        {
            _interviewRepository = interviewRepository;
        }

        public async Task<IEnumerable<InterviewResponse>> GetAllInterviewsAsync()
        {
            var interviews = await _interviewRepository.GetAllInterviewsAsync();

            return interviews.Select(MapToInterviewResponse);
        }

        public async Task<InterviewResponse?> GetInterviewByIdAsync(int id)
        {
            var interview = await _interviewRepository.GetInterviewByIdAsync(id);

            if (interview == null)
                return null;

            return MapToInterviewResponse(interview);
        }

        public async Task<InterviewResponse> CreateInterviewAsync(CreateInterviewRequest request)
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
                Status = "Scheduled",
                CreatedAt = DateTime.Now
            };

            var createdInterview = await _interviewRepository.CreateInterviewAsync(interview);

            return MapToInterviewResponse(createdInterview);
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

            if (updatedInterview == null)
                return null;

            return MapToInterviewResponse(updatedInterview);
        }

        public async Task<bool> DeleteInterviewAsync(int id)
        {
            return await _interviewRepository.DeleteInterviewAsync(id);
        }

        public async Task<InterviewResponse?> ScheduleInterviewAsync(int id, ScheduleInterviewRequest request)
        {
            var interview = await _interviewRepository.GetInterviewByIdAsync(id);

            if (interview == null)
                return null;

            interview.ScheduledDate = request.ScheduledDate;
            interview.Duration = request.Duration;
            interview.Location = request.Location;
            interview.MeetingLink = request.MeetingLink;
            interview.Status = "Scheduled";

            var updatedInterview = await _interviewRepository.UpdateInterviewAsync(id, interview);

            if (updatedInterview == null)
                return null;

            return MapToInterviewResponse(updatedInterview);
        }

        public async Task<InterviewResponse?> RescheduleInterviewAsync(int id, ScheduleInterviewRequest request)
        {
            var interview = await _interviewRepository.GetInterviewByIdAsync(id);

            if (interview == null)
                return null;

            interview.ScheduledDate = request.ScheduledDate;
            interview.Duration = request.Duration;
            interview.Location = request.Location;
            interview.MeetingLink = request.MeetingLink;
            interview.Status = "Rescheduled";

            var updatedInterview = await _interviewRepository.UpdateInterviewAsync(id, interview);

            if (updatedInterview == null)
                return null;

            return MapToInterviewResponse(updatedInterview);
        }

        public async Task<InterviewResponse?> CancelInterviewAsync(int id)
        {
            var updatedInterview = await _interviewRepository.UpdateInterviewStatusAsync(id, "Cancelled");

            if (updatedInterview == null)
                return null;

            return MapToInterviewResponse(updatedInterview);
        }

        public async Task<EvaluationResponse> CreateEvaluationAsync(int interviewId, CreateEvaluationRequest request)
        {
            var evaluation = new InterviewEvaluation
            {
                InterviewId = interviewId,
                InterviewerId = request.InterviewerId,
                TechnicalScore = request.TechnicalScore,
                CommunicationScore = request.CommunicationScore,
                ProblemSolvingScore = request.ProblemSolvingScore,
                CultureFitScore = request.CultureFitScore,
                OverallScore = CalculateOverallScore(
                    request.TechnicalScore,
                    request.CommunicationScore,
                    request.ProblemSolvingScore,
                    request.CultureFitScore
                ),
                Recommendation = request.Recommendation,
                Comments = request.Comments,
                SubmittedAt = DateTime.Now
            };

            var createdEvaluation = await _interviewRepository.CreateEvaluationAsync(evaluation);

            return MapToEvaluationResponse(createdEvaluation);
        }

        public async Task<EvaluationResponse?> GetEvaluationByInterviewIdAsync(int interviewId)
        {
            var evaluation = await _interviewRepository.GetEvaluationByInterviewIdAsync(interviewId);

            if (evaluation == null)
                return null;

            return MapToEvaluationResponse(evaluation);
        }

        public async Task<EvaluationResponse?> UpdateEvaluationAsync(int interviewId, CreateEvaluationRequest request)
        {
            var evaluation = new InterviewEvaluation
            {
                InterviewId = interviewId,
                InterviewerId = request.InterviewerId,
                TechnicalScore = request.TechnicalScore,
                CommunicationScore = request.CommunicationScore,
                ProblemSolvingScore = request.ProblemSolvingScore,
                CultureFitScore = request.CultureFitScore,
                OverallScore = CalculateOverallScore(
                    request.TechnicalScore,
                    request.CommunicationScore,
                    request.ProblemSolvingScore,
                    request.CultureFitScore
                ),
                Recommendation = request.Recommendation,
                Comments = request.Comments,
                SubmittedAt = DateTime.Now
            };

            var updatedEvaluation = await _interviewRepository.UpdateEvaluationAsync(interviewId, evaluation);

            if (updatedEvaluation == null)
                return null;

            return MapToEvaluationResponse(updatedEvaluation);
        }

        public async Task<IEnumerable<InterviewResponse>> GetUpcomingInterviewsAsync()
        {
            var interviews = await _interviewRepository.GetUpcomingInterviewsAsync();

            return interviews.Select(MapToInterviewResponse);
        }

        public async Task<HiringDashboardResponse> GetHiringDashboardAsync()
        {
            return await _interviewRepository.GetHiringDashboardAsync();
        }

        private static InterviewResponse MapToInterviewResponse(Interview interview)
        {
            return new InterviewResponse
            {
                InterviewId = interview.InterviewId,
                ApplicationId = interview.ApplicationId,
                InterviewType = interview.InterviewType,
                InterviewRound = interview.InterviewRound,
                ScheduledDate = interview.ScheduledDate,
                Duration = interview.Duration,
                Location = interview.Location,
                MeetingLink = interview.MeetingLink,
                Status = interview.Status,
                CreatedAt = interview.CreatedAt
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
                Recommendation = evaluation.Recommendation,
                Comments = evaluation.Comments,
                SubmittedAt = evaluation.SubmittedAt
            };
        }

        private static int CalculateOverallScore(
            int technicalScore,
            int communicationScore,
            int problemSolvingScore,
            int cultureFitScore)
        {
            return (technicalScore + communicationScore + problemSolvingScore + cultureFitScore) / 4;
        }
    }
}