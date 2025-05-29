using AG.EnumLocalization.Attributes;

namespace Sonar.Localization
{
    [EnumLocStrings("RelayDetails")]
    public enum RelayDetailsLoc
    {
        Name,

        World,

        Zone,

        Instance,

        Coordinates,

        Status,

        Rank,

        Level,

        Duration, // Fates

        Progress, // Fates

        [EnumLoc(Fallback = "Last Found")]
        LastFound,

        [EnumLoc(Fallback = "Last Seen")]
        LastSeen,

        [EnumLoc(Fallback = "Last Killed")]
        LastKilled,

        [EnumLoc(Fallback = "Last Healthy")]
        LastHealthy,

        Players,

        [EnumLoc(Fallback = "Actor ID")]
        ActorId, // Hunts
    }
}
