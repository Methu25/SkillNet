namespace SkillNet.Server.DTOs
{
    public class CreateEvaluationRequest
    {
        public int TechnicalScore { get; set; }

        public int CommunicationScore { get; set; }

        public int ProblemSolvingScore { get; set; }

        public int CultureFitScore { get; set; }

        public string? Recommendation { get; set; }

        public string? Comments { get; set; }
    }
}