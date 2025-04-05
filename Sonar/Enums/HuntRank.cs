using AG.EnumLocalization.Attributes;

namespace Sonar.Enums
{
    [EnumLocStrings]
    public enum HuntRank : byte
    {
        None = 0,

        B = 1,

        A = 2,

        S = 3,

        [EnumLoc(Fallback = "SS Minion")]
        SSMinion = 4,

        SS = 5,

        [EnumLoc(Fallback = "FATE")]
        Fate = 255
    }
}
