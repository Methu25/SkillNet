using SkillNet.Application.DTOs;

namespace SkillNet.Application.Interfaces
{
    public interface ICandidateDashboardService
    {
        Task<CandidateDashboardDto> GetDashboardAsync(int candidateId);
    }
}
