using SkillNet.Server.Interfaces;
using SkillNet.Server.Models;
using SkillNet.Server.DTOs;

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

        public async Task<bool> ScheduleInterviewAsync(
    int interviewId,
    ScheduleInterviewDto dto)
        {
            var interview = await GetByIdAsync(interviewId);

            if (interview == null)
                return false;

            interview.ScheduledDate = dto.ScheduledDate;
            interview.Duration = dto.Duration;
            interview.Location = dto.Location;
            interview.MeetingLink = dto.MeetingLink;
            interview.Status = "Scheduled";

            await UpdateAsync(interview);

            return true;
        }
    }
}