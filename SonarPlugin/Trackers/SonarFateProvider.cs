using Dalamud.Plugin.Services;
using DryIocAttributes;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using Microsoft.Extensions.Hosting;
using Sonar;
using Sonar.Data;
using Sonar.Enums;
using Sonar.Indexes;
using Sonar.Relays;
using Sonar.Trackers;
using Sonar.Utilities;
using SonarPlugin.Game;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Sonar.SonarConstants;

namespace SonarPlugin.Trackers
{
    [ExportMany]
    [SingletonReuse]
    public sealed class SonarFateProvider : IHostedService
    {
        private PlayerCounterService Players { get; }
        private SonarPlugin Plugin { get; }
        private SonarMeta Meta { get; }
        private IRelayTracker<FateRelay> Tracker { get; }
        private IPluginLog Logger { get; }

        /// <summary>
        /// Initialize fate tracker
        /// </summary>
        /// <param name="plugin">Sonar Plugin object</param>
        /// <param name="debug">(Optional) Output debug logging</param>
        public SonarFateProvider(PlayerCounterService players, SonarPlugin plugin, SonarMeta meta, IRelayTracker<FateRelay> tracker, IPluginLog logger)
        {
            // Get Sonar and Plugin Interface
            this.Players = players;
            this.Plugin = plugin;
            this.Meta = meta;
            this.Tracker = tracker;
            this.Logger = logger;

            // Initialization feedback
            this.Logger.Information("FateTracker Initialized");
        }

        private List<uint> _nextFateIds = [];
        private List<uint> _lastFateIds = [];
        private unsafe void Framework(IFramework framework)
        {
            // Don't proceed if the structures aren't ready
            if (!this.Plugin.SafeToReadTables) goto Fail;

            // Fate Manager
            var manager = FateManager.Instance();
            if (manager is null) goto Fail;

            // Get player position information
            var playerPosition = this.Meta.PlayerPosition;
            if (playerPosition is null) goto Fail;

            // Place and check timestamp
            var worldId = playerPosition.WorldId;
            var zoneId = playerPosition.ZoneId;
            var instanceId = playerPosition.InstanceId;
            double? timestamp = null;

            var fates = manager->Fates.AsSpan();
            var currentFateIds = this._nextFateIds;
            foreach (var fatePtr in fates)
            {
                var fate = fatePtr.Value;
                if (fate is null) continue; // && fate->State is not 0 && (fate->State is FateState.Preparing || (fate->StartTimeEpoch is not 0 && fate->Duration is not 0)))

                var state = fate->State;
                if (state is 0) continue;

                var id = fate->FateId;
                if (!Database.Fates.ContainsKey(id)) continue;

                var startTimeEpoch = fate->StartTimeEpoch;
                var duration = fate->Duration;
                var position = fate->Location;

                currentFateIds.Add(id);
                this.Tracker.FeedRelay(new FateRelay()
                {
                    Id = id,
                    WorldId = worldId,
                    ZoneId = zoneId,
                    InstanceId = instanceId,
                    Coords = position.SwapYZ(),
                    StartTime = startTimeEpoch * EarthSecond,
                    Duration = duration * EarthSecond,
                    Progress = fate->Progress,
                    Status = state.ToSonarFateStatus(),
                    Players = this.Players.GetCount(position),
                    CheckTimestamp = timestamp ??= UnixTimeHelper.SyncedUnixNow,
                    Bonus = fate->IsBonus,
                });
            }


            // Determine and mark disappeared fates as failed
            var lastFateIds = this._lastFateIds;
            var missingFates = lastFateIds.Except(currentFateIds);
            if (missingFates.Any())
            {
                var fateStates = this.Tracker.Data.States;
                foreach (var fateId in missingFates)
                {
                    var fateKey = IndexUtils.GetWorldZoneInstanceIndexKey(worldId, fateId, instanceId); // NOTE: Key formats are the same
                    var fateState = fateStates.GetValueOrDefault(fateKey);
                    if (fateState is null) continue;
                    var fate = fateState.Relay.Clone();
                    fate.Status = FateStatus.Failed;
                    this.Tracker.FeedRelay(fate);
                }
            }
            this._lastFateIds = currentFateIds;

            // Clear last fate keys and place the list into pooled list for next iteration
            lastFateIds.Clear();
            this._nextFateIds = lastFateIds;
            return;

        Fail:
            this._lastFateIds.Clear();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.Plugin.FrameworkUpdate += this.Framework;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.Plugin.FrameworkUpdate -= this.Framework;
            return Task.CompletedTask;
        }
    }
}
