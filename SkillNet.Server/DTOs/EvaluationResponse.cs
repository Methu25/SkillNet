namespace SkillNet.Server.DTOs;

public class EvaluationResponse
{
    public int EvaluationId { get; set; }

    public int InterviewId { get; set; }

    public int TechnicalScore { get; set; }

    public int CommunicationScore { get; set; }

    public int ProblemSolvingScore { get; set; }

    public int CultureFitScore { get; set; }

    public string Recommendation { get; set; } = string.Empty;

    public string Comments { get; set; } = string.Empty;
}