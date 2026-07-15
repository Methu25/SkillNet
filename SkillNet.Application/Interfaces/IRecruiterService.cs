using SkillNet.Application.DTOs;

namespace SkillNet.Application.Interfaces
{
    public interface IRecruiterService
    {
        Task<RecruiterProfileDto?> GetProfileAsync(int userId);
        Task<RecruiterProfileDto> UpsertProfileAsync(int userId, RecruiterProfileDto dto);
        Task<RecruiterDashboardDto> GetDashboardStatsAsync(int recruiterId);
    }
}
