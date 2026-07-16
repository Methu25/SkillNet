using SkillNet.Application.DTOs;

namespace SkillNet.Application.Interfaces
{
    public interface ICandidateService
    {
        Task<CandidateProfileDto> CreateCandidateAsync(int userId, CreateCandidateDto dto);
        Task<CandidateProfileDto?> GetCandidateProfileAsync(int userId);
        Task<CandidateProfileSummaryDto?> GetCandidateProfileSummaryAsync(int userId);
        Task<CandidateProfileDto?> UpdateCandidateAsync(int userId, UpdateCandidateDto dto);
        Task<bool> DeleteCandidateAsync(int userId);
    }
}
