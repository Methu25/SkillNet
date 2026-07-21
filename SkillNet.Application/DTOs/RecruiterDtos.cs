using System.ComponentModel.DataAnnotations;

namespace SkillNet.Application.DTOs
{
    public class RecruiterProfileDto
    {
        public int RecruiterProfileId { get; set; }
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
        [StringLength(200)]
        public string OrganizationName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Industry { get; set; }

        [StringLength(255)]
        public string? Website { get; set; }

        [StringLength(255)]
        public string? Logo { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        public string? Description { get; set; }

        [StringLength(50)]
        public string? CompanySize { get; set; }

        public int? FoundedYear { get; set; }

        [StringLength(254)]
        [EmailAddress]
        public string? ContactEmail { get; set; }

        [StringLength(30)]
        public string? ContactPhone { get; set; }

        [StringLength(255)]
        public string? LinkedInUrl { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }
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

        public string? Description { get; set; }
        public string? CompanySize { get; set; }
        public int? FoundedYear { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
    }
}
