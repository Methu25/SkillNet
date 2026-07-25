using System;
using System.Collections.Generic;
using System.Linq;
using SkillNet.Application.Interfaces;

namespace SkillNet.Application.Services
{
    public class RequiredSkillCoverageStrategy : ICandidateJobMatchingStrategy
    {
        public MatchingResult Match(MatchingInput input)
        {
            var result = new MatchingResult
            {
                MatchMethod = "RequiredSkillCoverage",
                MatchedSkills = new List<string>(),
                MissingSkills = new List<string>()
            };

            // Deduplicate inputs by ID, or by case-insensitive name if ID is not available
            var candSkills = input.CandidateSkills
                .Where(s => s != null)
                .GroupBy(s => s.SkillId > 0 ? (object)s.SkillId : s.SkillName.Trim().ToLowerInvariant())
                .Select(g => g.First())
                .ToList();

            var jobSkills = input.JobRequiredSkills
                .Where(s => s != null)
                .GroupBy(s => s.SkillId > 0 ? (object)s.SkillId : s.SkillName.Trim().ToLowerInvariant())
                .Select(g => g.First())
                .ToList();

            result.TotalRequiredSkills = jobSkills.Count;

            // Rule 10: Job has no required skills
            if (result.TotalRequiredSkills == 0)
            {
                result.MatchScore = 0;
                result.MatchedRequiredSkillCount = 0;
                return result;
            }

            // Rule 9: Candidate has no skills while job has requirements
            if (candSkills.Count == 0)
            {
                result.MatchScore = 0;
                result.MatchedRequiredSkillCount = 0;
                result.MissingSkills = jobSkills.Select(s => s.SkillName).ToList();
                return result;
            }

            int matchedCount = 0;
            foreach (var reqSkill in jobSkills)
            {
                bool isMatched = false;
                if (reqSkill.SkillId > 0)
                {
                    isMatched = candSkills.Any(cs => cs.SkillId == reqSkill.SkillId);
                }
                else
                {
                    var reqName = reqSkill.SkillName.Trim().ToLowerInvariant();
                    isMatched = candSkills.Any(cs => cs.SkillName.Trim().ToLowerInvariant() == reqName);
                }

                if (isMatched)
                {
                    matchedCount++;
                    result.MatchedSkills.Add(reqSkill.SkillName);
                }
                else
                {
                    result.MissingSkills.Add(reqSkill.SkillName);
                }
            }

            result.MatchedRequiredSkillCount = matchedCount;

            // Score = matched required skills / total required skills * 100
            double score = ((double)matchedCount / result.TotalRequiredSkills) * 100.0;

            // Round to nearest whole percentage (e.g. 66.6% -> 67%)
            result.MatchScore = (int)Math.Round(score, MidpointRounding.AwayFromZero);

            // Enforce score remains between 0 and 100
            if (result.MatchScore < 0) result.MatchScore = 0;
            if (result.MatchScore > 100) result.MatchScore = 100;

            return result;
        }
    }
}
