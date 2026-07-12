namespace SkillNet.Server.DTOs
{
    public class JobSearchRequest
    {
        public string? Keyword { get; set; }
        public int? CategoryId { get; set; }
        public string? WorkMode { get; set; }
        public string? Location { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public string? ExperienceLevel { get; set; }
        public string? EmploymentType { get; set; }
        public string SortBy { get; set; } = "newest"; // newest, salary-asc, salary-desc
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
