using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Logging;
using Dalamud.Plugin.Services;
using DryIocAttributes;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Microsoft.Extensions.Hosting;
using Sonar;
using Sonar.Data;
using Sonar.Models;
using Sonar.Relays;
using Sonar.Trackers;
using Sonar.Utilities;
using SonarPlugin.Game;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CSVector3 = FFXIVClientStructs.FFXIV.Common.Math.Vector3;

namespace SonarPlugin.Trackers
{
    [ExportMany]
    [SingletonReuse]
    public sealed class SonarHuntProvider : IHostedService
    {
        private PlayerCounterService Players { get; }
        private IRelayTracker<HuntRelay> Tracker { get; }
        private SonarPlugin Plugin { get; }
        private SonarMeta Meta { get; }
        private IClientState ClientState { get; }
        private IPluginLog Logger { get; }

        /// <summary>
        /// Initialize monster tracker
        /// </summary>
        public SonarHuntProvider(PlayerCounterService players, IRelayTracker<HuntRelay> tracker, SonarPlugin plugin, SonarMeta meta, IClientState clientState, IPluginLog logger)
        {
            // Get Sonar and Plugin Interface
            this.Players = players;
            this.Tracker = tracker;
            this.Plugin = plugin;
            this.Meta = meta;
            this.ClientState = clientState;
            this.Logger = logger;

            // Initialization feedback
            this.Logger.Information("MobTracker Initialized");
        }

        private unsafe void FrameworkTick(IFramework framework)
        {
            // Don't proceed if the structures aren't ready
            if (!this.Plugin.SafeToReadTables) return;

            // Character Manager
            var manager = CharacterManager.Instance();
            if (manager is null) return;

            // Get player position information
            var playerPosition = this.Meta.PlayerPosition;
            if (playerPosition is null) return;

            // Place and check timestamp
            var worldId = playerPosition.WorldId;
            var zoneId = playerPosition.ZoneId;
            var instanceId = playerPosition.InstanceId;
            double? timestamp = null;

            var characters = manager->BattleCharas;
            foreach (var characterPtr in characters)
            {
                var character = characterPtr.Value;
                if (character is null || character->ObjectKind is not ObjectKind.BattleNpc) continue;

                var id = character->NameId;
                if (!Database.Hunts.ContainsKey(id)) continue;

                var position = Unsafe.As<CSVector3, Vector3>(ref character->Position);

                this.Tracker.FeedRelay(new HuntRelay()
                {
                    Id = id,
                    ActorId = character->EntityId,
                    WorldId = worldId,
                    ZoneId = zoneId,
                    InstanceId = instanceId,
                    Coords = position.SwapYZ(),
                    CurrentHp = character->Health,
                    MaxHp = character->MaxHealth,
                    Players = this.Players.GetCount(position),
                    CheckTimestamp = timestamp ??= UnixTimeHelper.SyncedUnixNow,
                });
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.Plugin.FrameworkUpdate += this.FrameworkTick;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.Plugin.FrameworkUpdate -= this.FrameworkTick;
            return Task.CompletedTask;
        }
    }
}
