using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillNet.Application.Interfaces;
using SkillNet.Application.Services;

namespace SkillNet.WebApi.Controllers
{
    [ApiController]
    [Route("api/candidate/dashboard")]
    [Authorize(Roles = "Candidate")]
    public class CandidateDashboardController : ControllerBase
    {
        private readonly ICandidateDashboardService _dashboardService;
        private readonly IUserService _userService;

        public CandidateDashboardController(
            ICandidateDashboardService dashboardService,
            IUserService userService)
        {
            _dashboardService = dashboardService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var dashboard = await _dashboardService.GetDashboardAsync(userId);
            return Ok(dashboard);
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
