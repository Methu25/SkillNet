namespace SkillNet.Domain.Entities
{
    public class RecruiterNote
    {
        public int NoteId { get; set; }
        public int ApplicationId { get; set; }
        public int RecruiterId { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public JobApplication Application { get; set; } = null!;
        public RecruiterProfile Recruiter { get; set; } = null!;
    }
}
