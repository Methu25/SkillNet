using SkillNet.Application.DTOs;
using SkillNet.Application.Interfaces;

namespace SkillNet.Application.Services;

public class RequiredSkillCoverageFallbackProvider(ICandidateJobMatchingStrategy matchingStrategy) : IMatchAnalysisProvider
{
    public Task<MatchAnalysisResult> AnalyzeAsync(MatchAnalysisRequestData request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = matchingStrategy.Match(new MatchingInput
        {
            CandidateSkills = request.CandidateSkills.Select(name => new SkillInfo { SkillName = name }).ToList(),
            JobRequiredSkills = request.RequiredSkills.Select(name => new SkillInfo { SkillName = name }).ToList()
        });
        var action = result.MatchScore >= 75 ? "Strong Match" : result.MatchScore >= 50 ? "Consider" : "Skills Development Needed";
        return Task.FromResult(new MatchAnalysisResult
        {
            AiScore = result.MatchScore,
            Strengths = result.MatchedSkills,
            SkillGaps = result.MissingSkills,
            ConciseExplanation = $"Deterministic required-skill coverage: {result.MatchedRequiredSkillCount} of {result.TotalRequiredSkills} required skills matched.",
            RecommendedAction = action,
            Provider = "RequiredSkillCoverage",
            Model = "Deterministic",
            IsFallback = true
        });
    }
}
