using SkillNet.Application.DTOs;

namespace SkillNet.Application.Interfaces
{
    public interface IRecruiterService
    {
        Task<RecruiterProfileDto?> GetProfileAsync(int userId);
        Task<RecruiterProfileDto> UpsertProfileAsync(int userId, RecruiterProfileDto dto);
        Task<int?> GetRecruiterProfileIdAsync(int userId);
        Task<RecruiterDashboardDto> GetDashboardStatsAsync(int userId);
        Task<RecruiterOrganizationDto?> GetOrganizationAsync(int userId);
        Task<RecruiterOrganizationDto> UpsertOrganizationAsync(
            int userId,
            UpsertRecruiterOrganizationRequest request);
        Task<RecruiterOrganizationDto> SubmitOrganizationAsync(int userId);
        Task<bool> IsOrganizationApprovedAsync(int userId);
        Task<IEnumerable<RecruiterOrganizationDto>> GetPendingOrganizationsAsync();
        Task<RecruiterOrganizationDto?> ApproveOrganizationAsync(int organizationId);
        Task<RecruiterOrganizationDto?> RejectOrganizationAsync(
            int organizationId,
            string reason);
    }
}
