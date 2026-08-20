using AG.EnumLocalization.Attributes;

namespace Sonar.Relays
{
    [EnumLocStrings]
    public enum RelayType
    {
        Unknown,

        Hunt,

        [EnumLoc(Fallback = "돌발")]
        Fate,

        Manual,

        Event,
    }
}
