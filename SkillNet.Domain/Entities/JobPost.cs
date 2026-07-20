namespace SkillNet.Domain.Entities
{
    // Prototype Pattern: JobPost implements ICloneable so recruiters can duplicate
    // an existing job posting without building one from scratch.
    public class JobPost : ICloneable
    {
        public int JobId { get; set; }
        public int RecruiterId { get; set; }
        public int? OrganizationId { get; set; }
        public int CategoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string EmploymentType { get; set; } = string.Empty; // Full-time, Part-time, Contract, Internship
        public string WorkMode { get; set; } = string.Empty;       // Remote, Hybrid, Onsite
        public string Location { get; set; } = string.Empty;
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public string ExperienceLevel { get; set; } = string.Empty; // Junior, Mid, Senior
        public string Status { get; set; } = "Draft";               // Draft, Published, Closed
        public DateTime? ApplicationDeadline { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public RecruiterProfile RecruiterProfile { get; set; } = null!;
        public Organization? Organization { get; set; }
        public JobCategory JobCategory { get; set; } = null!;
        public ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();

        /// <summary>
        /// Prototype Pattern — Creates a new Draft copy of this job posting.
        /// Resets the ID, status, and deadline. Prefixes title with "[Copy]".
        /// More efficient than constructing a new job from scratch when re-posting
        /// a similar role.
        /// </summary>
        public object Clone()
        {
            return new JobPost
            {
                JobId = 0, // new record — DB will assign a new ID
                RecruiterId = this.RecruiterId,
                OrganizationId = this.OrganizationId,
                CategoryId = this.CategoryId,
                Title = "[Copy] " + this.Title,
                Description = this.Description,
                EmploymentType = this.EmploymentType,
                WorkMode = this.WorkMode,
                Location = this.Location,
                SalaryMin = this.SalaryMin,
                SalaryMax = this.SalaryMax,
                ExperienceLevel = this.ExperienceLevel,
                Status = "Draft",          // always starts as Draft
                ApplicationDeadline = null, // recruiter sets a new deadline
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }
    }
}
