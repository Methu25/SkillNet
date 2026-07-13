namespace SkillNet.Domain.Entities
{
    public class InterviewFeedbackHistory
    {
        public int HistoryId { get; set; }
        public int EvaluationId { get; set; }
        public int UpdatedBy { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
