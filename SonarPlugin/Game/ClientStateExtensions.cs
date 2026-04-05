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
                FateState.Preparing => FateStatus.Preparation,
                FateState.Running => FateStatus.Running,
                FateState.Ending => FateStatus.Running,
                FateState.Ended => FateStatus.Complete,
                FateState.Failed => FateStatus.Failed,

                (FateState)0 => FateStatus.Preparation,     /* Just spawned / Uninitialized */
                (FateState)9 => FateStatus.Failed,          /* Expired? */
                (FateState)6 => FateStatus.Failed,          /* Expired? */

                _ => FateStatus.Unknown,
            };

            // https://github.com/SapphireServer/Sapphire/blob/d9777084c55ac686f8027eaa09c7ad93f8c94f88/src/common/Common.h#L769
            // https://github.com/SapphireServer/Sapphire/blob/master/src/common/Common.h#L781

            // Some fates are expiring with 0x09

            // Fates that just spawned (and still don't have any info) have a Start Time, Duration and State of 0
            // as those values are not initialized yet.
        }
    }
}
