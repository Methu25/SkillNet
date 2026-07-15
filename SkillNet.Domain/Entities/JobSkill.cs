namespace SkillNet.Domain.Entities
{
    public class JobSkill
    {
        public int JobId { get; set; }
        public int SkillId { get; set; }
        public string? SkillName { get; set; } // populated on joins
    }
}
