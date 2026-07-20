namespace SkillNet.Domain.Entities
{
    public class ApplicationStatusHistory
    {
        public int StatusHistoryId { get; set; }
        public int ApplicationId { get; set; }
        public string? OldStatus { get; set; }
        public string NewStatus { get; set; } = string.Empty;
        public int ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? Comment { get; set; }

        public JobApplication Application { get; set; } = null!;
        public User ChangedByUser { get; set; } = null!;
    }
}
