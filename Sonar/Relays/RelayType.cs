using AG.EnumLocalization.Attributes;

namespace Sonar.Relays
{
    [EnumLocStrings]
    public enum RelayType
    {
        Unknown,

        Hunt,

        [EnumLoc(Fallback = "FATE")]
        Fate,

        Manual
    }
}
