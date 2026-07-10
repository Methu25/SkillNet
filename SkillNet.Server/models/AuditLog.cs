namespace SkillNet.Server.models
{
    public class AuditLog
    {
        public int LogId { get; set; }
        public int? UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string? IpAddress { get; set; }
        public DateTime Timestamp { get; set; }
    }
}