using Dalamud.Game.ClientState.Fates;
using Dalamud.Game.ClientState.Objects.Types;
using Sonar.Enums;
using Sonar.Models;
using Sonar.Relays;
using Sonar.Utilities;
using static Sonar.SonarConstants;

namespace SonarPlugin.Game
{
    public static class ClientStateExtensions
    {
        public static HuntRelay ToSonarHuntRelay(this IBattleNpc mob, GamePlace place, int playerCount)
        {
            return new HuntRelay()
            {
                Id = mob.NameId,
                ActorId = mob.EntityId, // NOTE / TODO: .GameObjectId seem to be something else
                WorldId = place.WorldId,
                ZoneId = place.ZoneId,
                InstanceId = place.InstanceId,
                Coords = mob.Position.SwapYZ(),
                CurrentHp = mob.CurrentHp,
                MaxHp = mob.MaxHp,
                Players = playerCount,
                CheckTimestamp = UnixTimeHelper.SyncedUnixNow,
            };
        }

        public static FateStatus ToSonarFateStatus(this FateState state)
        {
            // Based on: https://github.com/aers/FFXIVClientStructs/blob/main/FFXIVClientStructs/FFXIV/Client/Game/Fate/FateContext.cs
            return state switch
            {
                (FateState)3 => FateStatus.준비중,
                (FateState)4 => FateStatus.진행중,
                (FateState)5 => FateStatus.진행중,
                (FateState)7 => FateStatus.완료,
                (FateState)8 => FateStatus.실패,

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


        public static FateRelay ToSonarFateRelay(this IFate fate, GamePlace place, int playerCount)
        {
            return new FateRelay()
            {
                Id = fate.FateId,
                WorldId = place.WorldId,
                ZoneId = place.ZoneId,
                InstanceId = place.InstanceId,
                Coords = fate.Position.SwapYZ(),
                StartTime = fate.StartTimeEpoch * EarthSecond,
                Duration = fate.Duration * EarthSecond,
                Progress = fate.Progress,
                Status = fate.State.ToSonarFateStatus(),
                Players = playerCount,
                CheckTimestamp = UnixTimeHelper.SyncedUnixNow,
                Bonus = fate.HasBonus,
            };
        }
    }
}
