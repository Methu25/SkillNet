namespace SkillNet.Application.DTOs
{
    public class AddCandidateSkillDto
    {
        public int SkillId { get; set; }
    }

    public class CandidateSkillDto
    {
        public int CandidateId { get; set; }
        public int SkillId { get; set; }
        public string SkillName { get; set; } = string.Empty;
    }

    public class SkillDto
    {
        public int SkillId { get; set; }
        public string SkillName { get; set; } = string.Empty;
    }
}
