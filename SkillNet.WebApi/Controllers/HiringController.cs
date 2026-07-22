using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillNet.Application.Interfaces;

namespace SkillNet.WebApi.Controllers
{
    [Route("api/hiring")]
    [ApiController]
    [Authorize(Roles = "HiringManager")]
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
            var interviews = await _service.GetAssignedInterviewsAsync();
            return Ok(interviews);
        }

        [HttpGet("interviews/{id:int}")]
        public async Task<IActionResult> GetInterview(int id)
        {
            var interview = await _service.GetAssignedInterviewAsync(id);
            return interview == null ? NotFound() : Ok(interview);
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
            return Ok(await _service.GetAssignedInterviewsAsync());
        }
    }
}
