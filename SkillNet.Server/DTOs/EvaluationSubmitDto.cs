namespace SkillNet.Server.DTOs
{
    public class EvaluationSubmitDto
    {
        public int InterviewId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public EvaluationScoresDto Evaluations { get; set; } = new EvaluationScoresDto();
        public string Recommendation { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
    }

    public class EvaluationScoresDto
    {
        public int TechnicalScore { get; set; }
        public int CommunicationScore { get; set; }
        public int ProblemSolvingScore { get; set; }
        public int CultureFitScore { get; set; }
    }
}