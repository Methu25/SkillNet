namespace SkillNet.Domain.Entities
{
    public class AuditLog
    {
        public int AuditLogId { get; set; }
        public int? UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Entity { get; set; }
        public int? EntityId { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public DateTime Timestamp { get; set; }
        public string? IPAddress { get; set; }
    }
}
