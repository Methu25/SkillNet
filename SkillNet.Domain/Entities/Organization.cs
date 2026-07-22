namespace SkillNet.Domain.Entities
{
    public class Organization
    {
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public string? Industry { get; set; }
        public string? Website { get; set; }
        public string? Logo { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; }

        // Extended profile fields
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

