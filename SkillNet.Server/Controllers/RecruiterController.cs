using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillNet.Server.DTOs;
using SkillNet.Server.Interfaces;
using System.Security.Claims;

namespace SkillNet.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RecruiterController : ControllerBase
    {
        private readonly IRecruiterService _recruiterService;
        private readonly IJobService _jobService;

        public RecruiterController(IRecruiterService recruiterService, IJobService jobService)
        {
            _recruiterService = recruiterService;
            _jobService = jobService;
        }

        // GET /api/recruiter/profile
        [HttpGet("profile")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var profile = await _recruiterService.GetProfileAsync(userId);
            if (profile == null) return NotFound(new { message = "Recruiter profile not yet created." });
            return Ok(profile);
        }

        // POST /api/recruiter/profile — create or update profile
        [HttpPost("profile")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> UpsertProfile([FromBody] RecruiterProfileDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            dto.UserId = userId;
            var profile = await _recruiterService.UpsertProfileAsync(userId, dto);
            return Ok(profile);
        }

        // GET /api/recruiter/jobs — recruiter's own job postings with dashboard stats
        [HttpGet("jobs")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> GetMyJobs()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var jobs = await _jobService.GetRecruiterJobsAsync(userId);
            var stats = await _recruiterService.GetDashboardStatsAsync(userId);

            return Ok(new
            {
                stats,
                jobs
            });
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }
    }
}
