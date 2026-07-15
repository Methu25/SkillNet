namespace SkillNet.Domain.Entities
{
    public class InterviewEvaluation
    {
        public int EvaluationId { get; set; }
        public int InterviewId { get; set; }
        public int InterviewerId { get; set; }
        public int TechnicalScore { get; set; }
        public int CommunicationScore { get; set; }
        public int ProblemSolvingScore { get; set; }
        public int CultureFitScore { get; set; }
        public int OverallScore { get; set; }
        public string? Recommendation { get; set; }
        public string? Comments { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}
