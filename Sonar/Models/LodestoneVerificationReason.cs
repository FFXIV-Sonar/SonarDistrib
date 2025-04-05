using AG.EnumLocalization.Attributes;

namespace Sonar.Models
{
    [EnumLocStrings("Lodestone.Reason")]
    public enum LodestoneVerificationReason
    {
        /// <summary>Unknown, should not happen.</summary>
        [EnumLoc(Fallback = "Unknown, should not happen.")]
        Unknown,

        /// <summary>This character is not verified. You need to make your lodestone profile visible and searchable for verification to work. You can make your lodestone profile private again after verification.</summary>
        [EnumLoc(Fallback = "This character is not verified. You need to make your lodestone profile visible and searchable for verification to work. You can make your lodestone profile private again after verification.")]
        NotVerified,

        /// <summary>Your character profile could not be found. You need to make your lodestone profile visible and searchable for verification to work. You can make your lodestone profile private again after verification.</summary>
        [EnumLoc(Fallback = "Your character profile could not be found. You need to make your lodestone profile visible and searchable for verification to work. You can make your lodestone profile private again after verification.")]
        NotFound,

        /// <summary>Your character profile is found but its currently private. You need to make your lodestone profile visible and searchable for verification to work. You can make your lodestone profile private again after verification.</summary>
        [EnumLoc(Fallback = "Your character profile is found but its currently private. You need to make your lodestone profile visible and searchable for verification to work. You can make your lodestone profile private again after verification.")]
        PrivateProfile,

        /// <summary>Request is the result of a rename. You need to make your lodestone profile visible. You can make your lodestone profile private again after verification.</summary>
        [EnumLoc(Fallback = "Request is the result of a rename. You need to make your lodestone profile visible. You can make your lodestone profile private again after verification.")]
        Renamed, // Hash is used to detect and follow renames.

        /// <summary>Character hash mismatch and your lodestone profile is private. You need to make your lodestone profile visible. You can make your lodestone profile private again after verification.</summary>
        [EnumLoc(Fallback = "Character hash mismatch and your lodestone profile is private. You need to make your lodestone profile visible. You can make your lodestone profile private again after verification.")]
        HashMismatch,

        /// <summary>Character information is stale. You need to make your lodestone profile visible. You can make your lodestone profile private again after verification.</summary>
        [EnumLoc(Fallback = "Character information is stale. You need to make your lodestone profile visible. You can make your lodestone profile private again after verification.")]
        Stale, // Minimum planned: 1 year
    }
}
