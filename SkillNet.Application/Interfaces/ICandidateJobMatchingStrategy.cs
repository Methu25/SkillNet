using System.Collections.Generic;

namespace SkillNet.Application.Interfaces
{
    public class SkillInfo
    {
        public int SkillId { get; set; }
        public string SkillName { get; set; } = string.Empty;
    }

    public class MatchingInput
    {
        public List<SkillInfo> CandidateSkills { get; set; } = new();
        public List<SkillInfo> JobRequiredSkills { get; set; } = new();
    }

    public class MatchingResult
    {
        public int MatchScore { get; set; }
        public List<string> MatchedSkills { get; set; } = new();
        public List<string> MissingSkills { get; set; } = new();
        public int TotalRequiredSkills { get; set; }
        public int MatchedRequiredSkillCount { get; set; }
        public string MatchMethod { get; set; } = "RequiredSkillCoverage";
    }

    public interface ICandidateJobMatchingStrategy
    {
        MatchingResult Match(MatchingInput input);
    }
}
