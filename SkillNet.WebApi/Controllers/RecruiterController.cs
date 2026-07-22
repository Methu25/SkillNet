using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SkillNet.Application.DTOs;
using SkillNet.Application.Interfaces;
using SkillNet.Application.Services;
using SkillNet.WebApi.Models;
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

        [HttpGet("organization")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> GetOrganization()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var organization = await _recruiterService.GetOrganizationAsync(userId);
            if (organization == null)
                return NotFound(new { message = "Recruiter organization not yet created." });

            return Ok(organization);
        }

        [HttpPost("organization")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> UpsertOrganization(
            [FromBody] UpsertRecruiterOrganizationRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            try
            {
                return Ok(await _recruiterService.UpsertOrganizationAsync(userId, request));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(new { message = exception.Message });
            }
            catch (InvalidOperationException exception)
            {
                return Conflict(new { message = exception.Message });
            }
            catch (SqlException exception) when (exception.Number is 2601 or 2627)
            {
                return Conflict(new
                {
                    message = "A recruiter profile or organization link already exists. Refresh and try again."
                });
            }
            catch (SqlException exception) when (exception.Number == 2628)
            {
                return BadRequest(new
                {
                    message = "One or more organization fields exceed the supported length."
                });
            }
        }

        [HttpPost("organization/logo")]
        [Authorize(Roles = "Recruiter")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
        public async Task<IActionResult> UploadOrganizationLogo(
            [FromForm] FileUploadRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var logo = request.File;
            try
            {
                await using var content = logo.OpenReadStream();
                var organization = await _recruiterService.UploadOrganizationLogoAsync(
                    userId,
                    content,
                    Path.GetFileName(logo.FileName),
                    logo.ContentType,
                    logo.Length);
                return Ok(organization);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                return StatusCode(
                    StatusCodes.Status413PayloadTooLarge,
                    new { message = exception.Message });
            }
            catch (InvalidDataException exception)
            {
                return StatusCode(
                    StatusCodes.Status415UnsupportedMediaType,
                    new { message = exception.Message });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(new { message = exception.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpDelete("organization/logo")]
        [Authorize(Roles = "Recruiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteOrganizationLogo()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            try
            {
                return Ok(await _recruiterService.DeleteOrganizationLogoAsync(userId));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(new { message = exception.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
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
