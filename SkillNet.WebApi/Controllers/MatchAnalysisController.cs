using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillNet.Application.Services;

namespace SkillNet.WebApi.Controllers;

[ApiController]
[Route("api/match-analysis")]
public class MatchAnalysisController(MatchAnalysisService service) : ControllerBase
{
    [HttpPost("jobs/{jobId:int}/candidates/{candidateId:int}")]
    [Authorize(Roles = "Recruiter")]
    public async Task<IActionResult> AnalyzeCandidate(int jobId, int candidateId, CancellationToken cancellationToken) =>
        Ok(await service.AnalyzeForRecruiterAsync(jobId, candidateId, cancellationToken));

    [HttpPost("jobs/{jobId:int}/me")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> AnalyzeMe(int jobId, CancellationToken cancellationToken) =>
        Ok(await service.AnalyzeForCandidateAsync(jobId, cancellationToken));
}
