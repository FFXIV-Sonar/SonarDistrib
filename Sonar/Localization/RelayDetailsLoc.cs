using AG.EnumLocalization.Attributes;

namespace Sonar.Localization
{
    [EnumLocStrings("RelayDetails")]
    public enum RelayDetailsLoc
    {
        [EnumLoc(Fallback = "이름")]
        Name,

        [EnumLoc(Fallback = "서버")]
        World,

        [EnumLoc(Fallback = "지역")]
        Zone,

        [EnumLoc(Fallback = "인스턴스")]
        Instance,

        [EnumLoc(Fallback = "좌표")]
        Coordinates,

        [EnumLoc(Fallback = "상태")]
        Status,

        [EnumLoc(Fallback = "등급")]
        Rank,

        [EnumLoc(Fallback = "레벨")]
        Level,

        [EnumLoc(Fallback = "남은 시간")]
        Duration, // Fates

        [EnumLoc(Fallback = "진행도")]
        Progress, // Fates

        [EnumLoc(Fallback = "마지막 발견")]
        LastFound,

        [EnumLoc(Fallback = "마지막 목격")]
        LastSeen,

        [EnumLoc(Fallback = "마지막 토벌")]
        LastKilled,

        [EnumLoc(Fallback = "마지막 미토벌")]
        LastHealthy,

        [EnumLoc(Fallback = "주변 플레이어 수")]
        Players,

        [EnumLoc(Fallback = "액터 ID")]
        ActorId, // Hunts
    }
}
