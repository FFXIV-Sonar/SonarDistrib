namespace Sonar.Models
{
    public enum LodestoneVerificationReason
    {
        /// <summary>Unknown, should not happen.</summary>
        Unknown,

        /// <summary>This character is not verified. You need to make your lodestone profile visible and searchable for verification to work. You can make your lodestone profile private again after verification.</summary>
        NotVerified,

        /// <summary>Your character profile could not be found. You need to make your lodestone profile visible and searchable for verification to work. You can make your lodestone profile private again after verification.</summary>
        NotFound,

        /// <summary>Your character profile is found but its currently private. You need to make your lodestone profile visible and searchable for verification to work. You can make your lodestone profile private again after verification.</summary>
        PrivateProfile,

        /// <summary>Request is the result of a rename. You need to make your lodestone profile visible. You can make your lodestone profile private again after verification.</summary>
        Renamed, // Hash is used to detect and follow renames.

        /// <summary>Character hash mismatch and your lodestone profile is private. You need to make your lodestone profile visible. You can make your lodestone profile private again after verification.</summary>
        HashMismatch,

        /// <summary>Character information is stale. You need to make your lodestone profile visible. You can make your lodestone profile private again after verification.</summary>
        Stale, // Minimum planned: 1 year
    }
}
