namespace SkillNet.Server.Models
{
    public class RecruiterProfile
    {
        public int RecruiterProfileId { get; set; }
        public int UserId { get; set; }
        public string? Headline { get; set; }
        public string? Bio { get; set; }
        public string? LinkedInUrl { get; set; }
        public int? ExperienceYears { get; set; }
        public int? OrganizationId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
