namespace SkillNet.Domain.Entities
{
    public class Skill
    {
        public int SkillId { get; set; }
        public string SkillName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }

        public ICollection<CandidateSkill> CandidateSkills { get; set; } = new List<CandidateSkill>();
        public ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();
    }
}

