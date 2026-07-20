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
    }
}
