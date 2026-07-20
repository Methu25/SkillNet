namespace SkillNet.Application.DTOs
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

    public class UpsertRecruiterOrganizationRequest
    {
        public string OrganizationName { get; set; } = string.Empty;
        public string? Industry { get; set; }
        public string? Website { get; set; }
        public string? Logo { get; set; }
        public string? Address { get; set; }
    }

    public class RecruiterOrganizationDto
    {
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public string? Industry { get; set; }
        public string? Website { get; set; }
        public string? Logo { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ApprovalStatus { get; set; } = "Draft";
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? RejectionReason { get; set; }
    }

    public class RejectOrganizationRequest
    {
        public string Reason { get; set; } = string.Empty;
    }
}
