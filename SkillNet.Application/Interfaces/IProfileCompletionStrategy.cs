using SkillNet.Application.DTOs;

namespace SkillNet.Application.Interfaces
{
    public interface IProfileCompletionStrategy
    {
        Task<ProfileCompletionResultDto> CalculateAsync(CandidateProfileDto profile);
    }
}
