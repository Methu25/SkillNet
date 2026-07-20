namespace SkillNet.Application.DTOs
{
    public class CandidateDashboardDto
    {
        public bool HasProfile { get; set; }
        public bool IsFirstTimeUser { get; set; }
        public string? WelcomeMessage { get; set; }
        public CandidateProfileSummaryDto Profile { get; set; } = new();
        public ProfileCompletionResultDto ProfileCompletion { get; set; } = new();
        public int TotalResumes { get; set; }
        public bool HasActiveResume { get; set; }
        public ResumeDto? ActiveResume { get; set; }
        public ResumeDto? LatestResume { get; set; }
        public int TotalSkills { get; set; }
        public List<CandidateSkillDto> Skills { get; set; } = new();
        public int TotalApplications { get; set; }
        public int ActiveApplications { get; set; }
        public int AppliedApplications { get; set; }
        public int ShortlistedApplications { get; set; }
        public int InterviewScheduledApplications { get; set; }
        public int AcceptedApplications { get; set; }
        public int RejectedApplications { get; set; }
        public int UpcomingInterviews { get; set; }
        public List<JobResponse> RecommendedJobs { get; set; } = new();
        public List<InterviewResponse> Interviews { get; set; } = new();
    }

    public class ProfileCompletionResultDto
    {
        public int CompletionPercentage { get; set; }
        public int CompletionLevel { get; set; }
        public bool IsComplete { get; set; }
        public List<string> CompletedSections { get; set; } = new();
        public List<string> MissingSections { get; set; } = new();
    }
}
