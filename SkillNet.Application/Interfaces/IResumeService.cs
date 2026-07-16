using SkillNet.Application.DTOs;

namespace SkillNet.Application.Interfaces
{
    public interface IResumeService
    {
        Task<IEnumerable<ResumeDto>> GetCandidateResumesAsync(int candidateId);
        Task<ResumeDto?> GetActiveResumeAsync(int candidateId);
        Task<ResumeDto> UploadResumeAsync(int candidateId, UploadResumeDto dto);
        Task<ResumeDto?> ReplaceResumeAsync(int candidateId, int resumeId, ReplaceResumeDto dto);
        Task<ResumeDto?> SetActiveResumeAsync(int candidateId, int resumeId);
        Task<bool> DeleteResumeAsync(int candidateId, int resumeId);
    }
}
