namespace SkillNet.Domain.Entities
{
    public class InterviewAssignment
    {
        public int InterviewId { get; set; }

        public int InterviewerId { get; set; }

        public string? Role { get; set; }

        public Interview? Interview { get; set; }

        public Interviewer? Interviewer { get; set; }
    }
}