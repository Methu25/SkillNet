namespace SkillNet.Server.DTOs
{
    public class InterviewResponse
    {
        public int InterviewId { get; set; }

        public int ApplicationId { get; set; }

        public string? InterviewType { get; set; }

        public int InterviewRound { get; set; }

        public DateTime ScheduledDate { get; set; }

        public string? Status { get; set; }
    }
}