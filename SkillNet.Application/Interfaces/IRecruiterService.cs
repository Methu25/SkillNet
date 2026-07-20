using SkillNet.Application.DTOs;

namespace SkillNet.Application.Interfaces
{
    public interface IRecruiterService
    {
        Task<RecruiterProfileDto?> GetProfileAsync(int userId);
        Task<RecruiterProfileDto> UpsertProfileAsync(int userId, RecruiterProfileDto dto);
        Task<int?> GetRecruiterProfileIdAsync(int userId);
        Task<RecruiterDashboardDto> GetDashboardStatsAsync(int userId);
    }
}
