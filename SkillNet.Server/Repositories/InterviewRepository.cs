using SkillNet.Server.DTOs;
using SkillNet.Server.Interfaces;
using SkillNet.Server.Models;

namespace SkillNet.Server.Repositories
{
    public class InterviewRepository : IInterviewRepository
    {
        private static readonly List<Interview> Interviews = new();
        private static readonly List<InterviewEvaluation> Evaluations = new();

        public Task<IEnumerable<Interview>> GetAllInterviewsAsync()
        {
            return Task.FromResult(Interviews.AsEnumerable());
        }

        public Task<Interview?> GetInterviewByIdAsync(int id)
        {
            return Task.FromResult(Interviews.FirstOrDefault(i => i.InterviewId == id));
        }

        public Task<Interview> CreateInterviewAsync(Interview interview)
        {
            interview.InterviewId = Interviews.Count + 1;
            interview.CreatedAt = DateTime.Now;
            Interviews.Add(interview);
            return Task.FromResult(interview);
        }

        public Task<Interview?> UpdateInterviewAsync(int id, Interview interview)
        {
            var existing = Interviews.FirstOrDefault(i => i.InterviewId == id);
            if (existing == null) return Task.FromResult<Interview?>(null);

            existing.ApplicationId = interview.ApplicationId;
            existing.InterviewType = interview.InterviewType;
            existing.InterviewRound = interview.InterviewRound;
            existing.ScheduledDate = interview.ScheduledDate;
            existing.Duration = interview.Duration;
            existing.Location = interview.Location;
            existing.MeetingLink = interview.MeetingLink;
            existing.Status = interview.Status;

            return Task.FromResult<Interview?>(existing);
        }

        public Task<bool> DeleteInterviewAsync(int id)
        {
            var interview = Interviews.FirstOrDefault(i => i.InterviewId == id);
            if (interview == null) return Task.FromResult(false);

            Interviews.Remove(interview);
            return Task.FromResult(true);
        }

        public Task<Interview?> UpdateInterviewStatusAsync(int id, string status)
        {
            var interview = Interviews.FirstOrDefault(i => i.InterviewId == id);
            if (interview == null) return Task.FromResult<Interview?>(null);

            interview.Status = status;
            return Task.FromResult<Interview?>(interview);
        }

        public Task<InterviewEvaluation> CreateEvaluationAsync(InterviewEvaluation evaluation)
        {
            evaluation.EvaluationId = Evaluations.Count + 1;
            evaluation.SubmittedAt = DateTime.Now;
            Evaluations.Add(evaluation);
            return Task.FromResult(evaluation);
        }

        public Task<InterviewEvaluation?> GetEvaluationByInterviewIdAsync(int interviewId)
        {
            return Task.FromResult(Evaluations.FirstOrDefault(e => e.InterviewId == interviewId));
        }

        public Task<InterviewEvaluation?> UpdateEvaluationAsync(int interviewId, InterviewEvaluation evaluation)
        {
            var existing = Evaluations.FirstOrDefault(e => e.InterviewId == interviewId);
            if (existing == null) return Task.FromResult<InterviewEvaluation?>(null);

            existing.InterviewerId = evaluation.InterviewerId;
            existing.TechnicalScore = evaluation.TechnicalScore;
            existing.CommunicationScore = evaluation.CommunicationScore;
            existing.ProblemSolvingScore = evaluation.ProblemSolvingScore;
            existing.CultureFitScore = evaluation.CultureFitScore;
            existing.OverallScore = evaluation.OverallScore;
            existing.Recommendation = evaluation.Recommendation;
            existing.Comments = evaluation.Comments;
            existing.SubmittedAt = DateTime.Now;

            return Task.FromResult<InterviewEvaluation?>(existing);
        }

        public Task<IEnumerable<Interview>> GetUpcomingInterviewsAsync()
        {
            var upcoming = Interviews.Where(i => i.ScheduledDate >= DateTime.Now);
            return Task.FromResult(upcoming.AsEnumerable());
        }

        public Task<HiringDashboardResponse> GetHiringDashboardAsync()
        {
            return Task.FromResult(new HiringDashboardResponse
            {
                TotalInterviews = Interviews.Count,
                TodaysInterviews = Interviews.Count(i => i.ScheduledDate.Date == DateTime.Today),
                UpcomingInterviews = Interviews.Count(i => i.ScheduledDate > DateTime.Now),
                CompletedInterviews = Interviews.Count(i => i.Status == "Completed"),
                PendingEvaluations = Interviews.Count - Evaluations.Count,
                CancelledInterviews = Interviews.Count(i => i.Status == "Cancelled")
            });
        }
    }
}