using System.IO;

namespace SkillNet.Application.DTOs
{
    public class UploadResumeDto
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public Stream Content { get; set; } = Stream.Null;
    }

    public class ReplaceResumeDto
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public Stream Content { get; set; } = Stream.Null;
    }

    public class ResumeDto
    {
        public int ResumeId { get; set; }
        public int CandidateId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedDate { get; set; }
        public bool IsActive { get; set; }
    }
}
