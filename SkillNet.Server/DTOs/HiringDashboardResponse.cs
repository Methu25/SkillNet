namespace SkillNet.Server.DTOs
{
    public class HiringDashboardResponse
    {
        public int TotalInterviews { get; set; }

        public int TodaysInterviews { get; set; }

        public int UpcomingInterviews { get; set; }

        public int CompletedInterviews { get; set; }

        public int PendingEvaluations { get; set; }

        public int CancelledInterviews { get; set; }
    }
}