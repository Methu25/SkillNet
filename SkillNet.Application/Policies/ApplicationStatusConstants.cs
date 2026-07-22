namespace SkillNet.Application.Policies
{
    /// <summary>
    /// Canonical application status string constants.
    /// All status comparisons must use these values with OrdinalIgnoreCase.
    /// </summary>
    public static class ApplicationStatusConstants
    {
        public const string Applied = "Applied";
        public const string Shortlisted = "Shortlisted";
        public const string Interviewing = "Interviewing";
        public const string EvaluationSubmitted = "EvaluationSubmitted";
        public const string Hired = "Hired";
        public const string Rejected = "Rejected";
        public const string Withdrawn = "Withdrawn";
    }
}
