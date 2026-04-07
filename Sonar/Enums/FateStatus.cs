using AG.EnumLocalization.Attributes;
using Sonar.Relays;

namespace Sonar.Enums
{
    public enum FateStatus : byte
    {
        [EnumLocAlias<RelayStatus>(RelayStatus.Unknown)]
        알수없음, // 0

        [EnumLocAlias<RelayStatus>(RelayStatus.Preparing)]
        준비중, // 1

        [EnumLocAlias<RelayStatus>(RelayStatus.Running)]
        진행중, // 2

        [EnumLocAlias<RelayStatus>(RelayStatus.Complete)]
        완료, // 3

        [EnumLocAlias<RelayStatus>(RelayStatus.Failed)]
        실패, // 4
    }
}
