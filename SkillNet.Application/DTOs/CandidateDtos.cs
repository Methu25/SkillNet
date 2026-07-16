namespace SkillNet.Application.DTOs
{
    public class CreateCandidateDto
    {
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
    }

    public class UpdateCandidateDto
    {
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
