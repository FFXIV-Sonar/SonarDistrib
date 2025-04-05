using AG.EnumLocalization.Attributes;

namespace Sonar.Enums
{
    [EnumLocStrings]
    public enum FateStatus : byte
    {
        Unknown, // 0

        Preparation, // 1

        Running, // 2

        Complete, // 3

        Failed, // 4
    }
}
