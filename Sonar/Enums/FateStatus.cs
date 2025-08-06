using AG.EnumLocalization.Attributes;
using Sonar.Relays;

namespace Sonar.Enums
{
    public enum FateStatus : byte
    {
        [EnumLocAlias<RelayStatus>(RelayStatus.Unknown)]
        Unknown, // 0

        [EnumLocAlias<RelayStatus>(RelayStatus.Preparing)]
        Preparation, // 1

        [EnumLocAlias<RelayStatus>(RelayStatus.Running)]
        Running, // 2

        [EnumLocAlias<RelayStatus>(RelayStatus.Complete)]
        Complete, // 3

        [EnumLocAlias<RelayStatus>(RelayStatus.Failed)]
        Failed, // 4
    }
}
