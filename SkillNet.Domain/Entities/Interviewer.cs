namespace SkillNet.Domain.Entities
{
    public class Interviewer
    {
        public int InterviewerId { get; set; }
        public int UserId { get; set; }
        public int DepartmentId { get; set; }
        public string? Position { get; set; }
    }
}
