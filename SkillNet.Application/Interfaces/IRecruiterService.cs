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
        Task<RecruiterOrganizationDto> UploadOrganizationLogoAsync(
            int userId,
            Stream content,
            string fileName,
            string contentType,
            long fileSize);
        Task<RecruiterOrganizationDto> DeleteOrganizationLogoAsync(int userId);
    }
}
