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
        private readonly IProfileImageService _profileImageService;
        private readonly IUserService _userService;

        public CandidateProfileController(
            ICandidateService candidateService,
            IProfileImageService profileImageService,
            IUserService userService)
        {
            _candidateService = candidateService;
            _profileImageService = profileImageService;
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

        [HttpPost("image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadProfileImage([FromForm] IFormFile image)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

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
