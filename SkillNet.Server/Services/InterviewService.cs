using SkillNet.Server.Interfaces;
using SkillNet.Server.Models;
using SkillNet.Server.DTOs;

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

        public async Task<bool> ScheduleInterviewAsync(int interviewId, ScheduleInterviewDto dto)
        {
            var interview = await _repository.GetByIdAsync(interviewId);

            if (interview == null)
                return false;

            interview.ScheduledDate = dto.ScheduledDate;
            interview.Duration = dto.Duration;
            interview.Location = dto.Location;
            interview.MeetingLink = dto.MeetingLink;
            interview.Status = "Scheduled";

            await _repository.UpdateAsync(interview);

            return true;
        }
    }
}