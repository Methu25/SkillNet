using SkillNet.Application.Interfaces;

namespace SkillNet.Application.Policies
{
    /// <summary>
    /// Implements the legal recruiter-initiated application status transitions.
    ///
    /// Allowed recruiter transitions:
    ///   Applied             → Shortlisted | Rejected
    ///   Shortlisted         → Interviewing | Rejected
    ///   Interviewing        → EvaluationSubmitted | Rejected
    ///   EvaluationSubmitted → Hired | Rejected
    ///
    /// Recruiter may NOT set Withdrawn (candidate-only).
    /// Terminal statuses (Hired, Rejected, Withdrawn) accept no further changes.
    /// </summary>
    public class ApplicationStatusTransitionPolicy : IApplicationStatusTransitionPolicy
    {
        private static readonly HashSet<string> KnownStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            ApplicationStatusConstants.Applied,
            ApplicationStatusConstants.Shortlisted,
            ApplicationStatusConstants.Interviewing,
            ApplicationStatusConstants.EvaluationSubmitted,
            ApplicationStatusConstants.Hired,
            ApplicationStatusConstants.Rejected,
            ApplicationStatusConstants.Withdrawn
        };

        /// <summary>
        /// Adjacency list of legal recruiter transitions.
        /// Key = current status, Value = permitted target statuses.
        /// </summary>
        private static readonly Dictionary<string, HashSet<string>> AllowedTransitions =
            new(StringComparer.OrdinalIgnoreCase)
            {
                [ApplicationStatusConstants.Applied] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ApplicationStatusConstants.Shortlisted,
                    ApplicationStatusConstants.Rejected
                },
                [ApplicationStatusConstants.Shortlisted] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ApplicationStatusConstants.Interviewing,
                    ApplicationStatusConstants.Rejected
                },
                [ApplicationStatusConstants.Interviewing] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ApplicationStatusConstants.EvaluationSubmitted,
                    ApplicationStatusConstants.Rejected
                },
                [ApplicationStatusConstants.EvaluationSubmitted] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ApplicationStatusConstants.Hired,
                    ApplicationStatusConstants.Rejected
                }
            };

        /// <inheritdoc/>
        public bool IsKnownStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status)) return false;
            return KnownStatuses.Contains(status.Trim());
        }

        /// <inheritdoc/>
        public bool CanRecruiterTransition(string fromStatus, string toStatus)
        {
            if (string.IsNullOrWhiteSpace(fromStatus) || string.IsNullOrWhiteSpace(toStatus))
                return false;

            return AllowedTransitions.TryGetValue(fromStatus.Trim(), out var allowed)
                   && allowed.Contains(toStatus.Trim());
        }
    }
}
