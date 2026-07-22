using System;
using System.Collections.Generic;
using System.Linq;
using SkillNet.Application.Interfaces;
using SkillNet.Application.Services;
using Xunit;

namespace SkillNet.Tests
{
    public class RequiredSkillCoverageStrategyTests
    {
        private readonly RequiredSkillCoverageStrategy _strategy;

        public RequiredSkillCoverageStrategyTests()
        {
            _strategy = new RequiredSkillCoverageStrategy();
        }

        [Fact]
        public void Match_ExactMatch_Returns100()
        {
            var input = new MatchingInput
            {
                CandidateSkills = new List<SkillInfo>
                {
                    new() { SkillId = 1, SkillName = "C#" },
                    new() { SkillId = 2, SkillName = "React" }
                },
                JobRequiredSkills = new List<SkillInfo>
                {
                    new() { SkillId = 1, SkillName = "C#" },
                    new() { SkillId = 2, SkillName = "React" }
                }
            };

            var result = _strategy.Match(input);

            Assert.Equal(100, result.MatchScore);
            Assert.Equal(2, result.MatchedRequiredSkillCount);
            Assert.Empty(result.MissingSkills);
        }

        [Fact]
        public void Match_PartialMatch_ReturnsCorrectRoundedScore()
        {
            // Candidate has 2 out of 3 required skills (2/3 = 66.666...% -> should round to 67%)
            var input = new MatchingInput
            {
                CandidateSkills = new List<SkillInfo>
                {
                    new() { SkillId = 1, SkillName = "C#" },
                    new() { SkillId = 2, SkillName = "React" },
                    new() { SkillId = 4, SkillName = "Docker" } // extra
                },
                JobRequiredSkills = new List<SkillInfo>
                {
                    new() { SkillId = 1, SkillName = "C#" },
                    new() { SkillId = 2, SkillName = "React" },
                    new() { SkillId = 3, SkillName = "Kubernetes" } // missing
                }
            };

            var result = _strategy.Match(input);

            Assert.Equal(67, result.MatchScore);
            Assert.Equal(2, result.MatchedRequiredSkillCount);
            Assert.Single(result.MissingSkills);
            Assert.Equal("Kubernetes", result.MissingSkills.First());
        }

        [Fact]
        public void Match_NoMatch_Returns0()
        {
            var input = new MatchingInput
            {
                CandidateSkills = new List<SkillInfo>
                {
                    new() { SkillId = 4, SkillName = "Python" }
                },
                JobRequiredSkills = new List<SkillInfo>
                {
                    new() { SkillId = 1, SkillName = "C#" },
                    new() { SkillId = 2, SkillName = "React" }
                }
            };

            var result = _strategy.Match(input);

            Assert.Equal(0, result.MatchScore);
            Assert.Equal(0, result.MatchedRequiredSkillCount);
            Assert.Equal(2, result.MissingSkills.Count);
        }

        [Fact]
        public void Match_CandidateHasNoSkills_Returns0()
        {
            var input = new MatchingInput
            {
                CandidateSkills = new List<SkillInfo>(),
                JobRequiredSkills = new List<SkillInfo>
                {
                    new() { SkillId = 1, SkillName = "C#" }
                }
            };

            var result = _strategy.Match(input);

            Assert.Equal(0, result.MatchScore);
            Assert.Equal(0, result.MatchedRequiredSkillCount);
            Assert.Single(result.MissingSkills);
        }

        [Fact]
        public void Match_JobHasNoRequiredSkills_Returns0()
        {
            var input = new MatchingInput
            {
                CandidateSkills = new List<SkillInfo>
                {
                    new() { SkillId = 1, SkillName = "C#" }
                },
                JobRequiredSkills = new List<SkillInfo>()
            };

            var result = _strategy.Match(input);

            Assert.Equal(0, result.MatchScore);
            Assert.Equal(0, result.MatchedRequiredSkillCount);
            Assert.Empty(result.MatchedSkills);
        }

        [Fact]
        public void Match_DuplicateCandidateSkills_DoNotInflateScore()
        {
            var input = new MatchingInput
            {
                CandidateSkills = new List<SkillInfo>
                {
                    new() { SkillId = 1, SkillName = "C#" },
                    new() { SkillId = 1, SkillName = "C#" } // duplicate
                },
                JobRequiredSkills = new List<SkillInfo>
                {
                    new() { SkillId = 1, SkillName = "C#" },
                    new() { SkillId = 2, SkillName = "React" }
                }
            };

            var result = _strategy.Match(input);

            Assert.Equal(50, result.MatchScore);
            Assert.Equal(1, result.MatchedRequiredSkillCount);
        }

        [Fact]
        public void Match_DuplicateJobSkills_DoNotChangeDenominator()
        {
            var input = new MatchingInput
            {
                CandidateSkills = new List<SkillInfo>
                {
                    new() { SkillId = 1, SkillName = "C#" }
                },
                JobRequiredSkills = new List<SkillInfo>
                {
                    new() { SkillId = 1, SkillName = "C#" },
                    new() { SkillId = 1, SkillName = "C#" } // duplicate
                }
            };

            var result = _strategy.Match(input);

            Assert.Equal(100, result.MatchScore);
            Assert.Equal(1, result.MatchedRequiredSkillCount);
            Assert.Equal(1, result.TotalRequiredSkills);
        }

        [Fact]
        public void Match_ExtraCandidateSkills_DoNotReduceRequiredSkillCoverage()
        {
            var input = new MatchingInput
            {
                CandidateSkills = new List<SkillInfo>
                {
                    new() { SkillId = 1, SkillName = "C#" },
                    new() { SkillId = 2, SkillName = "React" },
                    new() { SkillId = 3, SkillName = "SQL" } // extra
                },
                JobRequiredSkills = new List<SkillInfo>
                {
                    new() { SkillId = 1, SkillName = "C#" },
                    new() { SkillId = 2, SkillName = "React" }
                }
            };

            var result = _strategy.Match(input);

            Assert.Equal(100, result.MatchScore);
        }

        [Fact]
        public void Match_MatchedSkills_IsCorrect()
        {
            var input = new MatchingInput
            {
                CandidateSkills = new List<SkillInfo>
                {
                    new() { SkillId = 1, SkillName = "C#" }
                },
                JobRequiredSkills = new List<SkillInfo>
                {
                    new() { SkillId = 1, SkillName = "C#" },
                    new() { SkillId = 2, SkillName = "React" }
                }
            };

            var result = _strategy.Match(input);

            Assert.Single(result.MatchedSkills);
            Assert.Equal("C#", result.MatchedSkills.First());
        }

        [Fact]
        public void Match_MissingSkills_IsCorrect()
        {
            var input = new MatchingInput
            {
                CandidateSkills = new List<SkillInfo>
                {
                    new() { SkillId = 1, SkillName = "C#" }
                },
                JobRequiredSkills = new List<SkillInfo>
                {
                    new() { SkillId = 1, SkillName = "C#" },
                    new() { SkillId = 2, SkillName = "React" }
                }
            };

            var result = _strategy.Match(input);

            Assert.Single(result.MissingSkills);
            Assert.Equal("React", result.MissingSkills.First());
        }

        [Fact]
        public void Match_Deterministic_SameInputReturnsSameResult()
        {
            var input1 = new MatchingInput
            {
                CandidateSkills = new List<SkillInfo> { new() { SkillId = 1, SkillName = "C#" } },
                JobRequiredSkills = new List<SkillInfo> { new() { SkillId = 1, SkillName = "C#" }, new() { SkillId = 2, SkillName = "React" } }
            };

            var input2 = new MatchingInput
            {
                CandidateSkills = new List<SkillInfo> { new() { SkillId = 1, SkillName = "C#" } },
                JobRequiredSkills = new List<SkillInfo> { new() { SkillId = 1, SkillName = "C#" }, new() { SkillId = 2, SkillName = "React" } }
            };

            var result1 = _strategy.Match(input1);
            var result2 = _strategy.Match(input2);

            Assert.Equal(result1.MatchScore, result2.MatchScore);
            Assert.Equal(result1.MatchedSkills.Count, result2.MatchedSkills.Count);
        }
    }
}
