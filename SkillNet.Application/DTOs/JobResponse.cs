namespace SkillNet.Application.DTOs
{
    public class JobResponse
    {
        public int JobId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string EmploymentType { get; set; } = string.Empty;
        public string WorkMode { get; set; } = string.Empty;
        public string? Location { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public string? ExperienceLevel { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ApplicationDeadline { get; set; }
        public List<string> Skills { get; set; } = new();
        public int RecruiterId { get; set; }
        public string RecruiterName { get; set; } = string.Empty;
        public string? OrganizationName { get; set; }
        public DateTime CreatedAt { get; set; }

        // Match Score Info (Optional/Candidate view)
        public int? MatchScore { get; set; }
        public List<string>? MatchedSkills { get; set; }
        public List<string>? MissingSkills { get; set; }
        public string? MatchMethod { get; set; }
    }
}
