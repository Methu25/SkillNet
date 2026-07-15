namespace SkillNet.Application.DTOs
{
    public class AssignInterviewerRequest
    {
        public int InterviewerId { get; set; }
        public string? Role { get; set; } // LeadInterviewer, PanelMember, Observer
    }
}
