namespace SkillNet.Domain.Entities
{
    public class Candidate
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Location { get; set; }
        public string? ProfessionalTitle { get; set; }
        public string? ProfessionalSummary { get; set; }
        public string? Education { get; set; }
        public string? Degree { get; set; }
        public string? University { get; set; }
        public int? ExperienceYears { get; set; }
        public string? ProfileImagePath { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsProfileCompleted { get; set; }

        public User User { get; set; } = null!;
        public ICollection<Resume> Resumes { get; set; } = new List<Resume>();
        public ICollection<CandidateSkill> CandidateSkills { get; set; } = new List<CandidateSkill>();
        public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
    }
}
