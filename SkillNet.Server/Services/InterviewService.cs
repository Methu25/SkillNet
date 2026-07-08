using SkillNet.Server.Interfaces;
using SkillNet.Server.Models;

namespace SkillNet.Server.Services
{
    public class InterviewService : IInterviewService
    {
        private readonly IInterviewRepository _repository;

        public InterviewService(IInterviewRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<Interview>> GetAllAsync()
        {
            return _repository.GetAllAsync();
        }

        public Task<Interview?> GetByIdAsync(int id)
        {
            return _repository.GetByIdAsync(id);
        }

        public Task<Interview> CreateAsync(Interview interview)
        {
            interview.Status = "Scheduled";
            interview.CreatedAt = DateTime.UtcNow;
            return _repository.CreateAsync(interview);
        }

        public Task<Interview?> UpdateAsync(Interview interview)
        {
            return _repository.UpdateAsync(interview);
        }

        public Task<bool> DeleteAsync(int id)
        {
            return _repository.DeleteAsync(id);
        }
    }
}