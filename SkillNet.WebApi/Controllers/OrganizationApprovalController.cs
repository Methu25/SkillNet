using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillNet.Application.DTOs;
using SkillNet.Application.Interfaces;

namespace SkillNet.WebApi.Controllers
{
    [ApiController]
    [Route("api/organization-approval")]
    [Authorize(Roles = "Admin")]
    public class OrganizationApprovalController(IRecruiterService recruiterService) : ControllerBase
    {
        private readonly IRecruiterService _recruiterService = recruiterService;

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            return Ok(await _recruiterService.GetPendingOrganizationsAsync());
        }

        [HttpPatch("{organizationId:int}/approve")]
        public async Task<IActionResult> Approve(int organizationId)
        {
            var organization = await _recruiterService.ApproveOrganizationAsync(organizationId);
            return organization == null
                ? NotFound(new { message = "Pending organization not found." })
                : Ok(organization);
        }

        [HttpPatch("{organizationId:int}/reject")]
        public async Task<IActionResult> Reject(
            int organizationId,
            [FromBody] RejectOrganizationRequest request)
        {
            var organization = await _recruiterService.RejectOrganizationAsync(
                organizationId,
                request.Reason);
            return organization == null
                ? NotFound(new { message = "Pending organization not found." })
                : Ok(organization);
        }
    }
}
