namespace SkillNet.Application.Interfaces
{
    /// <summary>
    /// Defines the legal application status transitions that a Recruiter may initiate.
    /// </summary>
    public interface IApplicationStatusTransitionPolicy
    {
        /// <summary>
        /// Returns true when a Recruiter may legally transition an application
        /// from <paramref name="fromStatus"/> to <paramref name="toStatus"/>.
        /// </summary>
        bool CanRecruiterTransition(string fromStatus, string toStatus);

        /// <summary>
        /// Returns true when <paramref name="status"/> is a recognised canonical status.
        /// </summary>
        bool IsKnownStatus(string status);
    }
}
