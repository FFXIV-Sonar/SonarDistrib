using SonarPlugin.Attributes;

namespace SonarPlugin.Config
{
    public enum SuppressVerification
    {
        [EnumCheapLoc("SuppressVerificationNone", "Never")]
        None,

        [EnumCheapLoc("SuppressVerificationNone", "Yes unless its required")]
        UnlessRequired,

        [EnumCheapLoc("SuppressVerificationNone", "Always")]
        Always
    }
}
