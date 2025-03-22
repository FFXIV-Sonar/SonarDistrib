using SonarPlugin.Attributes;

namespace SonarPlugin.Config
{
    public enum SuppressVerification
    {
        [EnumCheapLoc("SuppressVerificationNone", "표시")]
        None,

        [EnumCheapLoc("SuppressVerificationNone", "필수가 아닌 경우 숨기기")]
        UnlessRequired,

        [EnumCheapLoc("SuppressVerificationNone", "숨기기")]
        Always
    }
}
