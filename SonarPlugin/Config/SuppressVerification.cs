using SonarPlugin.Attributes;

namespace SonarPlugin.Config
{
    public enum SuppressVerification
    {
        [EnumCheapLoc("SuppressVerificationNone", "ǥ��")]
        None,

        [EnumCheapLoc("SuppressVerificationNone", "�ʼ��� �ƴ� ��� �����")]
        UnlessRequired,

        [EnumCheapLoc("SuppressVerificationNone", "�����")]
        Always
    }
}
