namespace SkillNet.Domain.Entities
{
    public class Interview
    {
        public int InterviewId { get; set; }
        public int ApplicationId { get; set; }
        public string? InterviewType { get; set; }
        public int InterviewRound { get; set; }
        public DateTime ScheduledDate { get; set; }
        public int Duration { get; set; }
        public string? Location { get; set; }
        public string? MeetingLink { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
