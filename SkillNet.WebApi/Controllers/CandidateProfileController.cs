using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillNet.Application.DTOs;
using SkillNet.Application.Interfaces;
using SkillNet.Application.Services;

namespace SkillNet.WebApi.Controllers
{
    [ApiController]
    [Route("api/candidate/profile")]
    [Authorize(Roles = "Candidate")]
    public class CandidateProfileController : ControllerBase
    {
        private readonly ICandidateService _candidateService;
        private readonly IUserService _userService;

        public CandidateProfileController(
            ICandidateService candidateService,
            IUserService userService)
        {
            _candidateService = candidateService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var profile = await _candidateService.GetCandidateProfileAsync(userId);
            if (profile == null)
            {
                return NotFound(new { message = "Candidate profile not yet created." });
            }

            return Ok(profile);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProfile([FromBody] CreateCandidateDto dto)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var profile = await _candidateService.CreateCandidateAsync(userId, dto);
                return CreatedAtAction(nameof(GetProfile), profile);
            }
            catch (InvalidOperationException ex)
                when (ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateCandidateDto dto)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var profile = await _candidateService.UpdateCandidateAsync(userId, dto);
            if (profile == null)
            {
                return NotFound(new { message = "Candidate profile not yet created." });
            }

            return Ok(profile);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteProfile()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var deleted = await _candidateService.DeleteCandidateAsync(userId);
            if (!deleted)
            {
                return NotFound(new { message = "Candidate profile not yet created." });
            }

            return Ok(new { message = "Candidate profile deleted successfully." });
        }

        [HttpGet("exists")]
        public async Task<IActionResult> ProfileExists()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var profile = await _candidateService.GetCandidateProfileAsync(userId);
            return Ok(new { exists = profile != null });
        }

        [HttpGet("completion")]
        public async Task<IActionResult> GetProfileCompletion()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var profile = await _candidateService.GetCandidateProfileAsync(userId);
            if (profile == null)
            {
                return NotFound(new { message = "Candidate profile not yet created." });
            }

            return Ok(profile.ProfileCompletion);
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
    }
}
