namespace Sonar.Models
{
    public enum SupportType
    {
        /// <summary>
        /// Unspecified
        /// </summary>
        [SupportTypeMeta]
        Unspecified,

        /// <summary>
        /// Feedback
        /// </summary>
        [SupportTypeMeta]
        Feedback,

        /// <summary>
        /// Suggestion
        /// </summary>
        [SupportTypeMeta]
        Suggestion,

        /// <summary>
        /// Bug Report
        /// </summary>
        [SupportTypeMeta]
        BugReport,

        /// <summary>
        /// Question
        /// </summary>
        [SupportTypeMeta(RequireContact = true)]
        Question,

        /// <summary>
        /// Player Report
        /// </summary>
        [SupportTypeMeta(RequireContact = true, RequirePlayerName = true)]
        PlayerReport,

        /// <summary>
        /// Appeal
        /// </summary>
        [SupportTypeMeta(RequireContact = true)]
        Appeal,
    }
}
