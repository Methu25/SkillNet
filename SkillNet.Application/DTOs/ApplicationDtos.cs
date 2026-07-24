using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SkillNet.Application.DTOs
{
    public class CreateJobApplicationDto
    {
        [Range(1, int.MaxValue)]
        public int JobId { get; set; }

        [Range(1, int.MaxValue)]
        public int ResumeId { get; set; }

        [StringLength(2000)]
        public string? CoverLetter { get; set; }
    }

    public class WithdrawApplicationDto
    {
        [StringLength(2000)]
        public string? Reason { get; set; }
    }

    public class UpdateApplicationStatusDto
    {
        [Required, StringLength(50)]
        public string Status { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Comment { get; set; }
    }

    public class AddRecruiterNoteDto
    {
        [Required, StringLength(2000)]
        public string Comment { get; set; } = string.Empty;
    }

    public class ApplicationSearchRequest
    {
        [Range(1, int.MaxValue)]
        public int? JobId { get; set; }

        [Range(1, int.MaxValue)]
        public int? CandidateId { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        [StringLength(200)]
        public string? SearchTerm { get; set; }

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;
    }

    public class JobApplicationDto
    {
        public int ApplicationId { get; set; }
        public int CandidateId { get; set; }
        public int JobId { get; set; }
        public int ResumeId { get; set; }
        public DateTime AppliedDate { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public string? CoverLetter { get; set; }
        public string? Source { get; set; }
        public DateTime LastUpdated { get; set; }

        public string CandidateName { get; set; } = string.Empty;
        public string CandidateEmail { get; set; } = string.Empty;
        public string? CandidateProfessionalTitle { get; set; }

        public string JobTitle { get; set; } = string.Empty;
        public string JobStatus { get; set; } = string.Empty;

        public string ResumeFileName { get; set; } = string.Empty;
        public string ResumeFileType { get; set; } = string.Empty;
        public long ResumeFileSize { get; set; }
        public DateTime ResumeUploadedDate { get; set; }

        public List<ApplicationStatusHistoryDto> StatusHistory { get; set; } = new();
        public List<string> ValidNextStatuses { get; set; } = new();
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<RecruiterNoteDto>? RecruiterNotes { get; set; }

        // Scheduled Interview Info
        public ScheduledInterviewDto? ScheduledInterview { get; set; }

        // Match Score Info
        public int? MatchScore { get; set; }
        public List<string>? MatchedSkills { get; set; }
        public List<string>? MissingSkills { get; set; }
        public string? MatchMethod { get; set; }
        public int? MatchedRequiredSkillCount { get; set; }
        public int? TotalRequiredSkills { get; set; }
    }

    public class ScheduledInterviewDto
    {
        public int InterviewId { get; set; }
        public string? InterviewType { get; set; }
        public int InterviewRound { get; set; }
        public DateTime ScheduledDate { get; set; }
        public int Duration { get; set; }
        public string? Location { get; set; }
        public string? MeetingLink { get; set; }
        public string? Status { get; set; }
    }

    public class JobApplicationSummaryDto
    {
        public int ApplicationId { get; set; }
        public int CandidateId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public int JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public int ResumeId { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public DateTime AppliedDate { get; set; }
        public DateTime LastUpdated { get; set; }

        // Match Score Info
        public int? MatchScore { get; set; }
        public List<string>? MatchedSkills { get; set; }
        public List<string>? MissingSkills { get; set; }
        public string? MatchMethod { get; set; }
        public int? MatchedRequiredSkillCount { get; set; }
        public int? TotalRequiredSkills { get; set; }
    }

    public class ApplicationStatusHistoryDto
    {
        public int StatusHistoryId { get; set; }
        public int ApplicationId { get; set; }
        public string? OldStatus { get; set; }
        public string NewStatus { get; set; } = string.Empty;
        public int ChangedBy { get; set; }
        public string ChangedByName { get; set; } = string.Empty;
        public string ChangedByEmail { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        public string? Comment { get; set; }
    }

    public class RecruiterNoteDto
    {
        public int NoteId { get; set; }
        public int ApplicationId { get; set; }
        public int RecruiterId { get; set; }
        public string RecruiterName { get; set; } = string.Empty;
        public string RecruiterEmail { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ApplicationStatisticsDto
    {
        public int Total { get; set; }
        public Dictionary<string, int> ByStatus { get; set; } = new();
    }
}
