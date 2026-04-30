using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using Sonar.Enums;
using Sonar.Models;
using Sonar.Relays;
using Sonar.Utilities;
using static Sonar.SonarConstants;

namespace SonarPlugin.Game
{
    public static class ClientStateExtensions
    {
        public static FateStatus ToSonarFateStatus(this FateState state)
        {
            // Based on: https://github.com/aers/FFXIVClientStructs/blob/main/FFXIVClientStructs/FFXIV/Client/Game/Fate/FateContext.cs
            return state switch
            {
                FateState.Preparing => FateStatus.준비중,
                FateState.Running => FateStatus.진행중,
                FateState.Ending => FateStatus.진행중,
                FateState.Ended => FateStatus.완료,
                FateState.Failed => FateStatus.실패,

                (FateState)0 => FateStatus.준비중,     /* Just spawned / Uninitialized */
                (FateState)9 => FateStatus.실패,          /* Expired? */
                (FateState)6 => FateStatus.실패,          /* Expired? */

                _ => FateStatus.알수없음,
            };

            // https://github.com/SapphireServer/Sapphire/blob/d9777084c55ac686f8027eaa09c7ad93f8c94f88/src/common/Common.h#L769
            // https://github.com/SapphireServer/Sapphire/blob/master/src/common/Common.h#L781

            // Some fates are expiring with 0x09

            // Fates that just spawned (and still don't have any info) have a Start Time, Duration and State of 0
            // as those values are not initialized yet.
        }
    }
}
