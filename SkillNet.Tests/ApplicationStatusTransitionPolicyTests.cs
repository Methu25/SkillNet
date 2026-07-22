using SkillNet.Application.Policies;
using Xunit;

namespace SkillNet.Tests
{
    public class ApplicationStatusTransitionPolicyTests
    {
        private readonly ApplicationStatusTransitionPolicy _policy = new();

        // --- IsKnownStatus ---

        [Theory]
        [InlineData("Applied")]
        [InlineData("Shortlisted")]
        [InlineData("Interviewing")]
        [InlineData("EvaluationSubmitted")]
        [InlineData("Hired")]
        [InlineData("Rejected")]
        [InlineData("Withdrawn")]
        [InlineData("applied")] // case-insensitive
        [InlineData("SHORTLISTED")]
        public void IsKnownStatus_KnownStatuses_ReturnsTrue(string status)
        {
            Assert.True(_policy.IsKnownStatus(status));
        }

        [Theory]
        [InlineData("AdministratorApproved")]
        [InlineData("Pending")]
        [InlineData("")]
        [InlineData("   ")]
        public void IsKnownStatus_UnknownOrEmpty_ReturnsFalse(string status)
        {
            Assert.False(_policy.IsKnownStatus(status));
        }

        // --- CanRecruiterTransition: legal transitions ---

        [Theory]
        [InlineData("Applied", "Shortlisted")]
        [InlineData("Applied", "Rejected")]
        [InlineData("Shortlisted", "Interviewing")]
        [InlineData("Shortlisted", "Rejected")]
        [InlineData("Interviewing", "EvaluationSubmitted")]
        [InlineData("Interviewing", "Rejected")]
        [InlineData("EvaluationSubmitted", "Hired")]
        [InlineData("EvaluationSubmitted", "Rejected")]
        [InlineData("applied", "shortlisted")] // case-insensitive
        public void CanRecruiterTransition_LegalTransitions_ReturnsTrue(string from, string to)
        {
            Assert.True(_policy.CanRecruiterTransition(from, to));
        }

        // --- CanRecruiterTransition: illegal transitions ---

        [Theory]
        [InlineData("Applied", "Hired")]           // skip
        [InlineData("Applied", "EvaluationSubmitted")] // skip
        [InlineData("Applied", "Interviewing")]    // skip
        [InlineData("Applied", "Withdrawn")]       // candidate-only
        [InlineData("Applied", "Applied")]         // same (not a valid forward move)
        [InlineData("Shortlisted", "Applied")]     // backwards
        [InlineData("Hired", "Shortlisted")]       // terminal
        [InlineData("Rejected", "Shortlisted")]    // terminal
        [InlineData("Withdrawn", "Applied")]       // terminal
        [InlineData("", "Shortlisted")]            // empty from
        [InlineData("Applied", "")]               // empty to
        public void CanRecruiterTransition_IllegalTransitions_ReturnsFalse(string from, string to)
        {
            Assert.False(_policy.CanRecruiterTransition(from, to));
        }
    }
}
