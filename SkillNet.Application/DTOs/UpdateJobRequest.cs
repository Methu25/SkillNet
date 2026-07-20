namespace SkillNet.Application.DTOs
{
    public class UpdateJobRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public string? EmploymentType { get; set; }
        public string? WorkMode { get; set; }
        public string? Location { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public string? ExperienceLevel { get; set; }
        public DateTime? ApplicationDeadline { get; set; }
        public List<int>? SkillIds { get; set; }
    }
}
