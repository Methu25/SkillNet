using SkillNet.Server.Models;

namespace SkillNet.Server.Interfaces
{
    public interface IInterviewService
    {
        Task<IEnumerable<Interview>> GetAllAsync();
        Task<Interview?> GetByIdAsync(int id);
        Task<Interview> CreateAsync(Interview interview);
        Task<Interview?> UpdateAsync(Interview interview);
        Task<bool> DeleteAsync(int id);
    }
}