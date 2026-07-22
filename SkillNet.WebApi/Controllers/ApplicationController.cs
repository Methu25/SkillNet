using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillNet.Application.DTOs;
using SkillNet.Application.Interfaces;
using SkillNet.Application.Services;

namespace SkillNet.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ApplicationController : ControllerBase
    {
        private readonly IApplicationService _applicationService;
        private readonly IUserService _userService;
        private readonly IRecruiterService _recruiterService;
        private readonly IJobService _jobService;

        public ApplicationController(
            IApplicationService applicationService,
            IUserService userService,
            IRecruiterService recruiterService,
            IJobService jobService)
        {
            _applicationService = applicationService;
            _userService = userService;
            _recruiterService = recruiterService;
            _jobService = jobService;
        }

        [HttpPost]
        [Authorize(Roles = "Candidate")]
        public async Task<IActionResult> ApplyForJob([FromBody] CreateJobApplicationDto dto)
        {
            if (!TryGetCurrentUserId(out var candidateId))
            {
                return Unauthorized();
            }

            var application = await _applicationService.ApplyForJobAsync(candidateId, dto);

            return CreatedAtAction(
                nameof(GetMyApplication),
                new { applicationId = application.ApplicationId },
                application);
        }

        [HttpGet("my")]
        [Authorize(Roles = "Candidate")]
        public async Task<IActionResult> GetMyApplications()
        {
            if (!TryGetCurrentUserId(out var candidateId))
            {
                return Unauthorized();
            }

            var applications = await _applicationService.GetCandidateApplicationsAsync(candidateId);
            return Ok(applications);
        }

        [HttpGet("my/{applicationId:int}")]
        [Authorize(Roles = "Candidate")]
        public async Task<IActionResult> GetMyApplication(int applicationId)
        {
            if (!TryGetCurrentUserId(out var candidateId))
            {
                return Unauthorized();
            }

            var application = await _applicationService.GetCandidateApplicationByIdAsync(candidateId, applicationId);
            if (application == null)
            {
                return NotFound(new { message = "Application not found." });
            }

            return Ok(application);
        }

        [HttpPatch("my/{applicationId:int}/withdraw")]
        [Authorize(Roles = "Candidate")]
        public async Task<IActionResult> WithdrawApplication(
            int applicationId,
            [FromBody] WithdrawApplicationDto dto)
        {
            if (!TryGetCurrentUserId(out var candidateId))
            {
                return Unauthorized();
            }

            var withdrawn = await _applicationService.WithdrawApplicationAsync(candidateId, applicationId, dto);
            if (!withdrawn)
            {
                return NotFound(new { message = "Application not found." });
            }

            return Ok(new { message = "Application withdrawn successfully." });
        }

        [HttpGet("job/{jobId:int}")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> GetApplicationsForJob(
            int jobId,
            [FromQuery] ApplicationSearchRequest request)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var recruiterId = await GetRecruiterProfileIdAsync(userId);
            if (!recruiterId.HasValue)
            {
                return NotFound(new { message = "Recruiter profile not found." });
            }

            var applications = await _applicationService.GetApplicationsForJobAsync(
                jobId,
                recruiterId.Value,
                request);

            return Ok(applications);
        }

        [HttpGet("recruiter/jobs")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> GetRecruiterJobs()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var recruiterId = await GetRecruiterProfileIdAsync(userId);
            if (!recruiterId.HasValue)
            {
                return NotFound(new { message = "Recruiter profile not found." });
            }

            var jobs = await _jobService.GetRecruiterJobsAsync(recruiterId.Value);
            return Ok(jobs);
        }

        [HttpGet("recruiter/{applicationId:int}")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> GetRecruiterApplication(int applicationId)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var recruiterId = await GetRecruiterProfileIdAsync(userId);
            if (!recruiterId.HasValue)
            {
                return NotFound(new { message = "Recruiter profile not found." });
            }

            var application = await _applicationService.GetRecruiterApplicationByIdAsync(
                recruiterId.Value,
                applicationId);

            if (application == null)
            {
                return NotFound(new { message = "Application not found." });
            }

            return Ok(application);
        }

        [HttpPatch("recruiter/{applicationId:int}/status")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> UpdateApplicationStatus(
            int applicationId,
            [FromBody] UpdateApplicationStatusDto dto)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var recruiterId = await GetRecruiterProfileIdAsync(userId);
            if (!recruiterId.HasValue)
            {
                return NotFound(new { message = "Recruiter profile not found." });
            }

            JobApplicationDto? application;
            try
            {
                application = await _applicationService.UpdateApplicationStatusAsync(
                    recruiterId.Value,
                    applicationId,
                    dto);
            }
            catch (InvalidOperationException exception)
            {
                return Conflict(new { message = exception.Message });
            }

            if (application == null)
            {
                return NotFound(new { message = "Application not found." });
            }

            return Ok(application);
        }

        [HttpPost("recruiter/{applicationId:int}/notes")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> AddRecruiterNote(
            int applicationId,
            [FromBody] AddRecruiterNoteDto dto)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var recruiterId = await GetRecruiterProfileIdAsync(userId);
            if (!recruiterId.HasValue)
            {
                return NotFound(new { message = "Recruiter profile not found." });
            }

            RecruiterNoteDto? note;
            try
            {
                note = await _applicationService.AddRecruiterNoteAsync(
                    recruiterId.Value,
                    applicationId,
                    dto);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new { message = exception.Message });
            }

            if (note == null)
            {
                return NotFound(new { message = "Application not found." });
            }

            return Ok(note);
        }

        [HttpGet("recruiter/statistics")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> GetApplicationStatistics([FromQuery] int? jobId)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var recruiterId = await GetRecruiterProfileIdAsync(userId);
            if (!recruiterId.HasValue)
            {
                return NotFound(new { message = "Recruiter profile not found." });
            }

            var statistics = await _applicationService.GetApplicationStatisticsAsync(
                recruiterId.Value,
                jobId);

            return Ok(statistics);
        }

        private bool TryGetCurrentUserId(out int userId)
        {
            userId = 0;

            var email = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            var user = _userService.GetUserByEmail(email);
            if (user == null)
            {
                return false;
            }

            userId = user.UserID;
            return userId > 0;
        }

        private async Task<int?> GetRecruiterProfileIdAsync(int userId)
        {
            var recruiterProfile = await _recruiterService.GetProfileAsync(userId);
            if (recruiterProfile == null || recruiterProfile.RecruiterProfileId <= 0)
            {
                return null;
            }

            return recruiterProfile.RecruiterProfileId;
        }
    }
}
