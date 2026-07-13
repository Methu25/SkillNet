namespace SkillNet.Domain.Entities
{
    public class Resume
    {
        public int ResumeId { get; set; }
        public int CandidateId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedDate { get; set; }
        public bool IsActive { get; set; }

        public Candidate Candidate { get; set; } = null!;
    }
}
