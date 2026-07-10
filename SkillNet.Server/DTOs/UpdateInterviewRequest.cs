namespace SkillNet.Server.DTOs
{
    public class UpdateInterviewRequest
    {
        public int ApplicationId { get; set; }

        public string InterviewType { get; set; } = string.Empty;

        public int InterviewRound { get; set; }

        public DateTime ScheduledDate { get; set; }

        public int Duration { get; set; }

        public string Location { get; set; } = string.Empty;

        public string? MeetingLink { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}