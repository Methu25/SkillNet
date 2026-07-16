using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillNet.Application.DTOs;
using SkillNet.Application.Interfaces;
using SkillNet.Application.Services;

namespace SkillNet.WebApi.Controllers
{
    [ApiController]
    [Route("api/candidate/skills")]
    [Authorize(Roles = "Candidate")]
    public class SkillController : ControllerBase
    {
        private readonly ISkillService _skillService;
        private readonly IUserService _userService;

        public SkillController(ISkillService skillService, IUserService userService)
        {
            _skillService = skillService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCandidateSkills()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var skills = await _skillService.GetCandidateSkillsAsync(userId);
            return Ok(skills);
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableSkills()
        {
            if (!TryGetCurrentUserId(out _))
            {
                return Unauthorized();
            }

            var skills = await _skillService.GetAllSkillsAsync();
            return Ok(skills);
        }

        [HttpPost]
        public async Task<IActionResult> AddSkill([FromBody] AddCandidateSkillDto dto)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var skill = await _skillService.AddCandidateSkillAsync(userId, dto);
            return CreatedAtAction(nameof(GetCandidateSkills), skill);
        }

        [HttpDelete("{skillId:int}")]
        public async Task<IActionResult> RemoveSkill(int skillId)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var removed = await _skillService.RemoveCandidateSkillAsync(userId, skillId);
            if (!removed)
            {
                return NotFound(new { message = "Candidate skill relationship not found." });
            }

            return Ok(new { message = "Skill removed successfully." });
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
