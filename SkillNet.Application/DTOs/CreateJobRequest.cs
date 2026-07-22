namespace SkillNet.Application.DTOs
{
    public class CreateJobRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string EmploymentType { get; set; } = string.Empty; // Full-time, Part-time, Contract, Internship
        public string WorkMode { get; set; } = string.Empty;       // Remote, Hybrid, Onsite
        public string? Location { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public string? ExperienceLevel { get; set; }               // Junior, Mid, Senior
        public DateTime? ApplicationDeadline { get; set; }
        public List<int> SkillIds { get; set; } = new();
    }
}
