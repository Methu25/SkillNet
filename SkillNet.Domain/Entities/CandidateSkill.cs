namespace SkillNet.Domain.Entities
{
    public class CandidateSkill
    {
        public int CandidateId { get; set; }
        public int SkillId { get; set; }

        public Candidate Candidate { get; set; } = null!;
        public Skill Skill { get; set; } = null!;
    }
}
