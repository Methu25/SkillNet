namespace SkillNet.Server.DTOs
{
    public class ScheduleInterviewDto
    {
        public DateTime ScheduledDate { get; set; }

        public int Duration { get; set; }

        public string Location { get; set; } = string.Empty;

        public string? MeetingLink { get; set; }
    }
}