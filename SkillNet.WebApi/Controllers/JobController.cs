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
    public class JobController : ControllerBase
    {
        private readonly IJobService _jobService;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public JobController(IJobService jobService, IUserService userService, IConfiguration configuration)
        {
            _jobService = jobService;
            _userService = userService;
            _configuration = configuration;
        }

        // GET /api/job?keyword=...&categoryId=...&workMode=...&page=1
        [HttpGet]
        public async Task<IActionResult> SearchJobs([FromQuery] JobSearchRequest request)
        {
            var results = await _jobService.SearchJobsAsync(request);
            return Ok(results);
        }

        // GET /api/job/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetJob(int id)
        {
            var job = await _jobService.GetJobByIdAsync(id);
            if (job == null) return NotFound(new { message = "Job not found." });
            return Ok(job);
        }

        // GET /api/job/categories — served from Singleton cache
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var connStr = _configuration.GetConnectionString("DefaultConnection")!;
                var categories = await JobCategoryService.GetInstance().GetCategoriesAsync(connStr);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // GET /api/job/skills — shared Skills catalog for Recruiter Job forms
        [HttpGet("skills")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> GetSkills()
        {
            return Ok(await _jobService.GetSkillsAsync());
        }

        // POST /api/job — create new job (Recruiter only, uses Builder Pattern)
        [HttpPost]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> CreateJob([FromBody] CreateJobRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var job = await _jobService.CreateJobAsync(userId, request);
            return CreatedAtAction(nameof(GetJob), new { id = job.JobId }, job);
        }

        // PUT /api/job/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> UpdateJob(int id, [FromBody] UpdateJobRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var job = await _jobService.UpdateJobAsync(id, userId, request);
            if (job == null) return NotFound(new { message = "Job not found or access denied." });
            return Ok(job);
        }

        // DELETE /api/job/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var deleted = await _jobService.DeleteJobAsync(id, userId);
            if (!deleted) return NotFound(new { message = "Job not found or access denied." });
            return Ok(new { message = "Job deleted successfully." });
        }

        // PATCH /api/job/{id}/publish
        [HttpPatch("{id}/publish")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> PublishJob(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var job = await _jobService.PublishJobAsync(id, userId);
            if (job == null) return NotFound(new { message = "Job not found or access denied." });
            return Ok(job);
        }

        // PATCH /api/job/{id}/close
        [HttpPatch("{id}/close")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> CloseJob(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var job = await _jobService.CloseJobAsync(id, userId);
            if (job == null) return NotFound(new { message = "Job not found or access denied." });
            return Ok(job);
        }

        // POST /api/job/{id}/duplicate — Prototype Pattern
        [HttpPost("{id}/duplicate")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> DuplicateJob(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            try
            {
                var cloned = await _jobService.DuplicateJobAsync(id, userId);
                return CreatedAtAction(nameof(GetJob), new { id = cloned.JobId }, cloned);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        private int GetCurrentUserId()
        {
            var email = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrWhiteSpace(email)) return 0;

            return _userService.GetUserByEmail(email)?.UserID ?? 0;
        }
    }
}
