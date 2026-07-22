namespace SkillNet.Application.DTOs;

public class MatchAnalysisRequestData
{
    public string JobTitle { get; set; } = string.Empty;
    public string JobDescription { get; set; } = string.Empty;
    public List<string> RequiredSkills { get; set; } = [];
    public List<string> CandidateSkills { get; set; } = [];
    public string? CandidateProfessionalTitle { get; set; }
    public string? CandidateExperienceSummary { get; set; }
}

public class MatchAnalysisResult
{
    public int AiScore { get; set; }
    public List<string> Strengths { get; set; } = [];
    public List<string> SkillGaps { get; set; } = [];
    public string ConciseExplanation { get; set; } = string.Empty;
    public string RecommendedAction { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public bool IsFallback { get; set; }
}
