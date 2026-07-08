using SkillNet.Server.Interfaces;
using SkillNet.Server.Models;

namespace SkillNet.Server.Repositories
{
    public class InterviewRepository : IInterviewRepository
    {
        public Task<IEnumerable<Interview>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Interview?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Interview> CreateAsync(Interview interview)
        {
            throw new NotImplementedException();
        }

        public Task<Interview?> UpdateAsync(Interview interview)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}