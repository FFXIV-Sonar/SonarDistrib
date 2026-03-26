using AG.EnumLocalization.Attributes;

namespace Sonar.Relays
{
    [EnumLocStrings]
    public enum RelayStatus
    {
        [EnumLoc(Fallback = "알 수 없음")]
        [RelayStatusMeta(0, 0, 0, 0, 1)] // Should never happen
        Unknown,

        [EnumLoc(Fallback = "준비중")]
        [RelayStatusMeta(1, 1, 0, 0, 0, RelayType.Fate)]
        Preparing,

        [EnumLoc(Fallback = "생존")]
        [RelayStatusMeta(1, 0, 0, 0, 0)] // Generic alive status
        Alive,

        [EnumLoc(Fallback = "생존")]
        [RelayStatusMeta(1, 1, 0, 0, 0, RelayType.Hunt)]
        Healthy,

        [EnumLoc(Fallback = "진행중")]
        [RelayStatusMeta(1, 1, 1, 0, 0, RelayType.Fate)]
        Running,

        [EnumLoc(Fallback = "토벌 중")]
        [RelayStatusMeta(1, 0, 1, 0, 0, RelayType.Hunt, RelayType.Fate)] // NOTE: Pulled may not make sense for all fates.
        Pulled,

        [EnumLoc(Fallback = "토벌 완료")]
        [RelayStatusMeta(0, 0, 0, 1, 0, RelayType.Hunt)]
        Dead,

        [EnumLoc(Fallback = "완료")]
        [RelayStatusMeta(0, 0, 0, 1, 0, RelayType.Fate)]
        Complete,

        [EnumLoc(Fallback = "실패")]
        [RelayStatusMeta(0, 0, 0, 1, 0, RelayType.Fate)]
        Failed,

        [EnumLoc(Fallback = "실패")]
        [RelayStatusMeta(0, 0, 0, 1, 0, RelayType.Fate)]
        Expired,

        [RelayStatusMeta(1, 1, 0, 0, 0, RelayType.Manual)]
        Manual,

        [RelayStatusMeta(0, 0, 0, 0, 1, RelayType.Hunt, RelayType.Fate, RelayType.Manual)]
        Stale,

        [EnumLoc(Fallback = "Not Applicable")]
        [RelayStatusMeta(0, 0, 0, 0, 0)]
        NotApplicable = -1,
    }
}
