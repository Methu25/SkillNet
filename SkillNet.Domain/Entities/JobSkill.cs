namespace SkillNet.Domain.Entities
{
    public class JobSkill
    {
        public int JobId { get; set; }
        public int SkillId { get; set; }
        public string? SkillName { get; set; } // populated on joins

        public JobPost JobPost { get; set; } = null!;
        public Skill Skill { get; set; } = null!;
    }
}
