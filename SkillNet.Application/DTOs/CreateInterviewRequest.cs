namespace SkillNet.Application.DTOs
{
    public class CreateInterviewRequest
    {
        public int ApplicationId { get; set; }
        public string? InterviewType { get; set; }
        public int InterviewRound { get; set; }
        public DateTime ScheduledDate { get; set; }
        public int Duration { get; set; }
        public string? Location { get; set; }
        public string? MeetingLink { get; set; }
    }
}
