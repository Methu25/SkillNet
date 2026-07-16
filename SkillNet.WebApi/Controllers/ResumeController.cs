using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillNet.Application.DTOs;
using SkillNet.Application.Interfaces;
using SkillNet.Application.Services;

namespace SkillNet.WebApi.Controllers
{
    [ApiController]
    [Route("api/candidate/resumes")]
    [Authorize(Roles = "Candidate")]
    public class ResumeController : ControllerBase
    {
        private readonly IResumeService _resumeService;
        private readonly IUserService _userService;
        private readonly long _maximumFileSize;
        private readonly string _pdfContentType;

        public ResumeController(
            IResumeService resumeService,
            IUserService userService,
            IConfiguration configuration)
        {
            _resumeService = resumeService;
            _userService = userService;
            _maximumFileSize = long.TryParse(
                configuration["ResumeStorage:MaximumFileSizeBytes"],
                out var configuredMaximum)
                    ? configuredMaximum
                    : 10 * 1024 * 1024;
            _pdfContentType = configuration["ResumeStorage:AllowedMimeType"] ??
                "application/pdf";
        }

        [HttpGet]
        public async Task<IActionResult> GetAllResumes()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var resumes = await _resumeService.GetCandidateResumesAsync(userId);
            return Ok(resumes);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveResume()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var resume = await _resumeService.GetActiveResumeAsync(userId);
            if (resume == null)
            {
                return NotFound(new { message = "No active resume was found." });
            }

            return Ok(resume);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadResume([FromForm] IFormFile file)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var validationResult = ValidatePdf(file);
            if (validationResult != null)
            {
                return validationResult;
            }

            await using var content = file.OpenReadStream();
            var resume = await _resumeService.UploadResumeAsync(userId, new UploadResumeDto
            {
                FileName = Path.GetFileName(file.FileName),
                ContentType = file.ContentType,
                FileSize = file.Length,
                Content = content
            });

            return CreatedAtAction(nameof(GetAllResumes), resume);
        }

        [HttpPut("{resumeId:int}/replace")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ReplaceResume(int resumeId, [FromForm] IFormFile file)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var validationResult = ValidatePdf(file);
            if (validationResult != null)
            {
                return validationResult;
            }

            await using var content = file.OpenReadStream();
            var resume = await _resumeService.ReplaceResumeAsync(userId, resumeId, new ReplaceResumeDto
            {
                FileName = Path.GetFileName(file.FileName),
                ContentType = file.ContentType,
                FileSize = file.Length,
                Content = content
            });

            if (resume == null)
            {
                return NotFound(new { message = "Resume not found or access denied." });
            }

            return Ok(resume);
        }

        [HttpPut("{resumeId:int}/set-active")]
        public async Task<IActionResult> SetActiveResume(int resumeId)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var resume = await _resumeService.SetActiveResumeAsync(userId, resumeId);
            if (resume == null)
            {
                return NotFound(new { message = "Resume not found or access denied." });
            }

            return Ok(resume);
        }

        [HttpDelete("{resumeId:int}")]
        public async Task<IActionResult> DeleteResume(int resumeId)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var deleted = await _resumeService.DeleteResumeAsync(userId, resumeId);
            if (!deleted)
            {
                return NotFound(new { message = "Resume not found or access denied." });
            }

            return Ok(new { message = "Resume deleted successfully." });
        }

        [HttpGet("{resumeId:int}/download")]
        public async Task<IActionResult> DownloadResume(int resumeId)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var download = await _resumeService.DownloadResumeAsync(userId, resumeId);
            if (download == null)
            {
                return NotFound(new { message = "Resume file not found or access denied." });
            }

            return File(
                download.Content,
                download.ContentType,
                download.FileName,
                enableRangeProcessing: true);
        }

        private IActionResult? ValidatePdf(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "A non-empty resume file is required." });
            }

            if (file.Length > _maximumFileSize)
            {
                return StatusCode(
                    StatusCodes.Status413PayloadTooLarge,
                    new { message = $"Resume file size cannot exceed {_maximumFileSize} bytes." });
            }

            if (!string.Equals(Path.GetExtension(file.FileName), ".pdf", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(file.ContentType, _pdfContentType, StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(
                    StatusCodes.Status415UnsupportedMediaType,
                    new { message = "Only PDF resume files are supported." });
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
