namespace SkillNet.Server.DTOs
{
    public class RecruiterProfileDto
    {
        public int UserId { get; set; }
        public string? Headline { get; set; }
        public string? Bio { get; set; }
        public string? LinkedInUrl { get; set; }
        public int? ExperienceYears { get; set; }
        public int? OrganizationId { get; set; }
        public string? OrganizationName { get; set; }
    }

    public class RecruiterDashboardDto
    {
        public int TotalJobs { get; set; }
        public int PublishedJobs { get; set; }
        public int DraftJobs { get; set; }
        public int ClosedJobs { get; set; }
        public int TotalApplicationsReceived { get; set; }
    }
}
