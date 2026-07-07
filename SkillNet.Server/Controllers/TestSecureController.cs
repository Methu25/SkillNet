using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SkillNet.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestSecureController : ControllerBase
    {
        // Anyone logged in with a valid token can access this
        [Authorize]
        [HttpGet("all-users")]
        public IActionResult GetAllUsersData()
        {
            return Ok(new { message = "Success: You are authenticated!" });
        }

        // ONLY users registered as 'Admin' can access this
        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult GetAdminData()
        {
            return Ok(new { message = "Success: Welcome, Admin!" });
        }

        // ONLY users registered as 'Candidate' can access this
        [Authorize(Roles = "Candidate")]
        [HttpGet("candidate-only")]
        public IActionResult GetCandidateData()
        {
            return Ok(new { message = "Success: Welcome, Candidate!" });
        }
    }
}