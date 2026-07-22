using SkillNet.Domain.Entities;

namespace SkillNet.Application.Utilities
{
    /// <summary>
    /// Builder Pattern — Solves the problem of constructing a complex JobPost object
    /// that has many optional parameters. Instead of one large constructor with 12+
    /// parameters (which is unreadable and error-prone), the builder lets callers
    /// set only the fields they need in a fluent, readable chain.
    ///
    /// Usage:
    ///   var job = new JobPostBuilder()
    ///       .SetTitle("Senior Developer")
    ///       .SetCategory(1)
    ///       .SetWorkMode("Remote")
    ///       .SetSalaryRange(80000, 120000)
    ///       .Build();
    /// </summary>
    public class JobPostBuilder
    {
        private readonly JobPost _job = new();

        public JobPostBuilder SetTitle(string title)
        {
            _job.Title = title;
            return this;
        }

        public JobPostBuilder SetDescription(string description)
        {
            _job.Description = description;
            return this;
        }

        public JobPostBuilder SetCategory(int categoryId)
        {
            _job.CategoryId = categoryId;
            return this;
        }

        public JobPostBuilder SetEmploymentType(string type)
        {
            _job.EmploymentType = type;
            return this;
        }

        public JobPostBuilder SetWorkMode(string mode)
        {
            _job.WorkMode = mode;
            return this;
        }

        public JobPostBuilder SetLocation(string location)
        {
            _job.Location = location;
            return this;
        }

        public JobPostBuilder SetSalaryRange(decimal? min, decimal? max)
        {
            _job.SalaryMin = min;
            _job.SalaryMax = max;
            return this;
        }

        public JobPostBuilder SetExperienceLevel(string level)
        {
            _job.ExperienceLevel = level;
            return this;
        }

        public JobPostBuilder SetApplicationDeadline(DateTime? deadline)
        {
            _job.ApplicationDeadline = deadline;
            return this;
        }

        public JobPostBuilder SetRecruiter(int recruiterId, int? organizationId)
        {
            _job.RecruiterId = recruiterId;
            _job.OrganizationId = organizationId;
            return this;
        }

        /// <summary>
        /// Finalises and returns the assembled JobPost object.
        /// </summary>
        public JobPost Build()
        {
            _job.Status = "Draft";
            _job.CreatedAt = DateTime.Now;
            _job.UpdatedAt = DateTime.Now;
            return _job;
        }
    }
}
