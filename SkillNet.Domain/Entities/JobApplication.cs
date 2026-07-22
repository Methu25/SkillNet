namespace SkillNet.Domain.Entities
{
    public class JobApplication
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

        public Candidate Candidate { get; set; } = null!;
        public JobPost Job { get; set; } = null!;
        public Resume Resume { get; set; } = null!;
        public ICollection<ApplicationStatusHistory> StatusHistory { get; set; } = new List<ApplicationStatusHistory>();
        public ICollection<RecruiterNote> RecruiterNotes { get; set; } = new List<RecruiterNote>();
        public ICollection<Interview> Interviews { get; set; } = new List<Interview>();
    }
}
