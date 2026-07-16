using System.ComponentModel.DataAnnotations;

namespace SkillNet.Application.DTOs
{
    public class CreateCandidateDto
    {
        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        [Required, StringLength(30)]
        public string? PhoneNumber { get; set; }
        [Required, StringLength(150)]
        public string? Location { get; set; }
        [StringLength(150)]
        public string? ProfessionalTitle { get; set; }
        [StringLength(2000)]
        public string? ProfessionalSummary { get; set; }
        [StringLength(2000)]
        public string? Education { get; set; }
        [StringLength(150)]
        public string? Degree { get; set; }
        [StringLength(200)]
        public string? University { get; set; }
        [Range(0, 60)]
        public int? ExperienceYears { get; set; }
        public string? ProfileImagePath { get; set; }
    }

    public class UpdateCandidateDto
    {
        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        [Required, StringLength(30)]
        public string? PhoneNumber { get; set; }
        [Required, StringLength(150)]
        public string? Location { get; set; }
        [StringLength(150)]
        public string? ProfessionalTitle { get; set; }
        [StringLength(2000)]
        public string? ProfessionalSummary { get; set; }
        [StringLength(2000)]
        public string? Education { get; set; }
        [StringLength(150)]
        public string? Degree { get; set; }
        [StringLength(200)]
        public string? University { get; set; }
        [Range(0, 60)]
        public int? ExperienceYears { get; set; }
        public string? ProfileImagePath { get; set; }
    }

    public class CandidateProfileDto
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
        public ResumeDto? ActiveResume { get; set; }
        public List<CandidateSkillDto> Skills { get; set; } = new();
        public ProfileCompletionResultDto ProfileCompletion { get; set; } = new();
    }

    public class CandidateProfileSummaryDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? ProfessionalTitle { get; set; }
        public string? ProfessionalSummary { get; set; }
        public string? Education { get; set; }
        public string? Degree { get; set; }
        public string? Location { get; set; }
        public int? ExperienceYears { get; set; }
        public string? ProfileImagePath { get; set; }
        public bool IsProfileCompleted { get; set; }
        public int ProfileCompletionPercentage { get; set; }
        public int ProfileCompletionLevel { get; set; }
    }
}
