using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillNet.Application.DTOs;
using SkillNet.Application.Interfaces;
using SkillNet.Application.Services;
using SkillNet.WebApi.Models;

namespace SkillNet.WebApi.Controllers
{
    [ApiController]
    [Route("api/candidate/profile")]
    [Authorize(Roles = "Candidate")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public class CandidateProfileController : ControllerBase
    {
        private readonly ICandidateService _candidateService;
        private readonly IProfileImageService _profileImageService;
        private readonly IUserService _userService;
        private readonly IAuditLogService _auditLogService;

        public CandidateProfileController(
            ICandidateService candidateService,
            IProfileImageService profileImageService,
            IUserService userService,
            IAuditLogService auditLogService)
        {
            _candidateService = candidateService;
            _profileImageService = profileImageService;
            _userService = userService;
            _auditLogService = auditLogService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(CandidateProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        [ProducesResponseType(typeof(CandidateProfileDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
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
        [ProducesResponseType(typeof(CandidateProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

            await _auditLogService.LogActionAsync("Account Changed (Profile Update)", "Candidates", userId, null, null);

            return Ok(profile);
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ProfileExists()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var exists = await _candidateService.CandidateExistsAsync(userId);
            return Ok(new { exists });
        }

        [HttpGet("completion")]
        [ProducesResponseType(typeof(ProfileCompletionResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        [HttpPost("image")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
        public async Task<IActionResult> UploadProfileImage([FromForm] FileUploadRequest request)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var image = request.File;
            var validationResult = ValidateProfileImage(image);
            if (validationResult != null)
            {
                return validationResult;
            }

            await using var content = image.OpenReadStream();
            var imageUrl = await _profileImageService.UploadAsync(
                userId,
                content,
                Path.GetFileName(image.FileName),
                image.ContentType,
                image.Length);

            return Ok(new { profileImageUrl = imageUrl });
        }

        [HttpDelete("image")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProfileImage()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var deleted = await _profileImageService.DeleteAsync(userId);
            if (!deleted)
            {
                return NotFound(new { message = "Candidate profile image not found." });
            }

            return Ok(new { message = "Candidate profile image deleted successfully." });
        }

        private IActionResult? ValidateProfileImage(IFormFile? image)
        {
            const long maximumFileSize = 5 * 1024 * 1024;
            var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            if (image == null || image.Length == 0)
            {
                return BadRequest(new { message = "A non-empty profile image is required." });
            }

            if (image.Length > maximumFileSize)
            {
                return StatusCode(
                    StatusCodes.Status413PayloadTooLarge,
                    new { message = $"Profile image size cannot exceed {maximumFileSize} bytes." });
            }

            if (!allowedContentTypes.Contains(image.ContentType, StringComparer.OrdinalIgnoreCase) ||
                !allowedExtensions.Contains(
                    Path.GetExtension(image.FileName),
                    StringComparer.OrdinalIgnoreCase))
            {
                return StatusCode(
                    StatusCodes.Status415UnsupportedMediaType,
                    new { message = "Only JPEG, PNG, and WEBP profile images are supported." });
            }

            return null;
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
