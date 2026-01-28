using AG.EnumLocalization.Attributes;
using Sonar.Relays;

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

        [EnumLocAlias<RelayType>(RelayType.Fate)]
        Fate = 128,

        [EnumLocAlias<RelayType>(RelayType.Event)]
        Event = 129,
    }
}
