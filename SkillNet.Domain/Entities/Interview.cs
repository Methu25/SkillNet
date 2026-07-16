using System.ComponentModel.DataAnnotations.Schema;

namespace SkillNet.Domain.Entities
{
    public class Interview
    {
        public int InterviewId { get; set; }

        public int ApplicationId { get; set; }

        public string? InterviewType { get; set; }

        public int InterviewRound { get; set; }

        public DateTime ScheduledDate { get; set; }

        public int Duration { get; set; }

        public string? Location { get; set; }

        public string? MeetingLink { get; set; }

        public string? Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public ICollection<InterviewAssignment> InterviewAssignments { get; set; } = new List<InterviewAssignment>();

        public ICollection<InterviewEvaluation> InterviewEvaluations { get; set; } = new List<InterviewEvaluation>();

        // Display-only fields. These should NOT be database columns.
        [NotMapped]
        public string? CandidateName { get; set; }

        [NotMapped]
        public string? CandidateEmail { get; set; }

        [NotMapped]
        public string? JobTitle { get; set; }

        [NotMapped]
        public string? CandidateSummary { get; set; }

        [NotMapped]
        public string? CandidateSkills { get; set; }

        [NotMapped]
        public int? ExperienceYears { get; set; }

        [NotMapped]
        public string? Role { get; set; }
    }
}