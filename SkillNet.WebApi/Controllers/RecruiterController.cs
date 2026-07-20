using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillNet.Application.DTOs;
using SkillNet.Application.Interfaces;
using SkillNet.Application.Services;
using System.Security.Claims;

namespace SkillNet.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RecruiterController : ControllerBase
    {
        private readonly IRecruiterService _recruiterService;
        private readonly IJobService _jobService;
        private readonly IUserService _userService;

        public RecruiterController(IRecruiterService recruiterService, IJobService jobService, IUserService userService)
        {
            _recruiterService = recruiterService;
            _jobService = jobService;
            _userService = userService;
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

            return Ok(new { stats, jobs });
        }

        private int GetCurrentUserId()
        {
            var email = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrWhiteSpace(email)) return 0;

            return _userService.GetUserByEmail(email)?.UserID ?? 0;
        }
    }
}
