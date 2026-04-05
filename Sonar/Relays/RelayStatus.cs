using AG.EnumLocalization.Attributes;

namespace Sonar.Relays
{
    [EnumLocStrings]
    public enum RelayStatus
    {
        [RelayStatusMeta(0, 0, 0, 0, 1)] // Should never happen
        Unknown,

        [RelayStatusMeta(1, 1, 0, 0, 0, RelayType.Fate, RelayType.Event)]
        Preparing,

        [RelayStatusMeta(1, 0, 0, 0, 0)] // Generic alive status
        Alive,

        [RelayStatusMeta(1, 1, 0, 0, 0, RelayType.Hunt)]
        Healthy,

        [RelayStatusMeta(1, 1, 1, 0, 0, RelayType.Fate, RelayType.Event)]
        Running,

        [RelayStatusMeta(1, 0, 1, 0, 0, RelayType.Hunt, RelayType.Fate, RelayType.Event)] // NOTE: Pulled may not make sense for all fates.
        Pulled,

        [RelayStatusMeta(0, 0, 0, 1, 0, RelayType.Hunt)]
        Dead,

        [RelayStatusMeta(0, 0, 0, 1, 0, RelayType.Fate, RelayType.Event)]
        Complete,

        [RelayStatusMeta(0, 0, 0, 1, 0, RelayType.Fate, RelayType.Event)]
        Failed,

        [RelayStatusMeta(0, 0, 0, 1, 0, RelayType.Fate, RelayType.Event)]
        Expired,

        [RelayStatusMeta(1, 1, 0, 0, 0, RelayType.Manual)]
        Manual,

        [RelayStatusMeta(0, 0, 0, 0, 1, RelayType.Hunt, RelayType.Fate, RelayType.Event, RelayType.Manual)]
        Stale,

        [EnumLoc(Fallback = "Not Applicable")]
        [RelayStatusMeta(0, 0, 0, 0, 0)]
        NotApplicable = -1,
    }
}
