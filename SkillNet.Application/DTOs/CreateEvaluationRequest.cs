namespace SkillNet.Application.DTOs
{
    public class CreateEvaluationRequest
    {
        public int InterviewerId { get; set; }
        public int TechnicalScore { get; set; }
        public int CommunicationScore { get; set; }
        public int ProblemSolvingScore { get; set; }
        public int CultureFitScore { get; set; }
        public string Recommendation { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
    }
}
