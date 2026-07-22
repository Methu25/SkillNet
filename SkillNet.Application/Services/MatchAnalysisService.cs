using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SkillNet.Application.DTOs;
using SkillNet.Application.Interfaces;

namespace SkillNet.Application.Services;

public class MatchAnalysisService(
    ICurrentUserContext currentUser,
    IJobRepository jobs,
    ICandidateRepository candidates,
    IApplicationRepository applications,
    IRecruiterService recruiters,
    IMatchAnalysisProviderFactory providerFactory,
    RequiredSkillCoverageFallbackProvider fallback,
    ILogger<MatchAnalysisService> logger)
{
    public async Task<MatchAnalysisResult> AnalyzeForRecruiterAsync(int jobId, int candidateId, CancellationToken cancellationToken)
    {
        if (!currentUser.IsInRole("Recruiter")) throw new UnauthorizedAccessException("Recruiter access is required.");
        var userId = currentUser.UserId ?? throw new UnauthorizedAccessException("Current user could not be resolved.");
        var recruiterId = await recruiters.GetRecruiterProfileIdAsync(userId) ?? throw new KeyNotFoundException("Recruiter profile not found.");
        var job = await jobs.GetJobByIdAsync(jobId) ?? throw new KeyNotFoundException("Job not found.");
        if (job.RecruiterId != recruiterId) throw new UnauthorizedAccessException("The authenticated recruiter does not own this job.");
        if (!await applications.HasCandidateAppliedAsync(candidateId, jobId)) throw new KeyNotFoundException("Candidate application not found.");
        return await AnalyzeAsync(job, candidateId, cancellationToken);
    }

    public async Task<MatchAnalysisResult> AnalyzeForCandidateAsync(int jobId, CancellationToken cancellationToken)
    {
        if (!currentUser.IsInRole("Candidate")) throw new UnauthorizedAccessException("Candidate access is required.");
        var candidateId = currentUser.UserId ?? throw new UnauthorizedAccessException("Current user could not be resolved.");
        var job = await jobs.GetJobByIdAsync(jobId) ?? throw new KeyNotFoundException("Job not found.");
        if (!string.Equals(job.Status, "Published", StringComparison.OrdinalIgnoreCase)) throw new KeyNotFoundException("Active job not found.");
        return await AnalyzeAsync(job, candidateId, cancellationToken);
    }

    private async Task<MatchAnalysisResult> AnalyzeAsync(SkillNet.Domain.Entities.JobPost job, int candidateId, CancellationToken cancellationToken)
    {
        var candidate = await candidates.GetCandidateByUserIdAsync(candidateId) ?? throw new KeyNotFoundException("Candidate profile not found.");
        var requiredSkills = (await jobs.GetSkillsByJobIdAsync(job.JobId)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var data = new MatchAnalysisRequestData
        {
            JobTitle = Limit(job.Title, 200),
            JobDescription = Limit(job.Description, 4000),
            RequiredSkills = requiredSkills,
            CandidateSkills = candidate.CandidateSkills.Select(item => item.Skill.SkillName).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            CandidateProfessionalTitle = LimitNullable(candidate.ProfessionalTitle, 200),
            CandidateExperienceSummary = LimitNullable(candidate.ProfessionalSummary, 1000)
        };

        var provider = providerFactory.CreateProvider();
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await provider.AnalyzeAsync(data, cancellationToken);
            logger.LogInformation("Match analysis provider {Provider} succeeded for job {JobId} in {DurationMs}ms.", result.Provider, job.JobId, stopwatch.ElapsedMilliseconds);
            return result;
        }
        catch (MatchAnalysisProviderException)
        {
            logger.LogWarning("Match analysis provider failed for job {JobId}; using deterministic fallback after {DurationMs}ms.", job.JobId, stopwatch.ElapsedMilliseconds);
            return await fallback.AnalyzeAsync(data, cancellationToken);
        }
    }

    private static string Limit(string value, int max) => value.Length <= max ? value : value[..max];
    private static string? LimitNullable(string? value, int max) => string.IsNullOrWhiteSpace(value) ? null : Limit(value.Trim(), max);
}
