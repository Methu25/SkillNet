using Microsoft.AspNetCore.Mvc;
using SkillNet.Server.Interfaces;

namespace SkillNet.Server.Controllers
{
    [Route("api/hiring")]
    [ApiController]
    public class HiringController : ControllerBase
    {
        private readonly IInterviewService _service;

        public HiringController(IInterviewService service)
        {
            _service = service;
        }

        [HttpGet("interviews")]
        public async Task<IActionResult> GetAllInterviews()
        {
            var interviews = await _service.GetAllInterviewsAsync();
            return Ok(interviews);
        }

        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingInterviews()
        {
            var interviews = await _service.GetUpcomingInterviewsAsync();
            return Ok(interviews);
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetHiringDashboard()
        {
            var dashboard = await _service.GetHiringDashboardAsync();
            return Ok(dashboard);
        }
    }
}