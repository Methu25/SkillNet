namespace SkillNet.Server.models
{
    public class Department
    {
        public int DepartmentId { get; set; }
        public int OrganizationId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}