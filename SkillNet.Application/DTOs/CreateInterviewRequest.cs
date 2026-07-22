namespace SkillNet.Application.DTOs
{
    public class CreateInterviewRequest
    {
        public int ApplicationId { get; set; }
        public string? InterviewType { get; set; }
        public int InterviewRound { get; set; } = 1;
        public DateTime ScheduledDate { get; set; }
        public int Duration { get; set; } = 60;
        public string? Location { get; set; }
        public string? MeetingLink { get; set; }
        public string? Notes { get; set; }
        public List<int> InterviewerIds { get; set; } = [];
    }
}
