using System.Collections.Generic;
using System.Linq;
using Sonar.Models;
using Sonar.Enums;
using Dalamud.Game;
using Dalamud.Game.ClientState.Fates;
using SonarPlugin.Game;
using System.Threading;
using System.Threading.Tasks;
using Sonar.Trackers;
using Dalamud.Logging;

namespace SonarPlugin.Trackers
{
    public sealed class SonarFateProvider : IHostedService
    {
        private PlayerProvider Player { get; }
        private SonarPlugin Plugin { get; }
        private FateTracker Tracker { get; }
        private FateTable Fates { get; }

        /// <summary>
        /// Initialize fate tracker
        /// </summary>
        /// <param name="plugin">Sonar Plugin object</param>
        /// <param name="debug">(Optional) Output debug logging</param>
        public SonarFateProvider(PlayerProvider player, SonarPlugin plugin, FateTracker tracker, FateTable fates)
        {
            // Get Sonar and Plugin Interface
            this.Player = player;
            this.Plugin = plugin;
            this.Tracker = tracker;
            this.Fates = fates;

            // Initialization feedback
            PluginLog.LogInformation("FateTracker Initialized");
        }

        private List<string> _lastFateKeys = new();
        private void Framework(Framework framework)
        {
            // Don't proceed if the structures aren't ready
            if (!this.Plugin.SafeToReadTables)
            {
                this._lastFateKeys.Clear();
                return;
            }

            // Iterate throughout all fates in the fates table
            var fates = this.Fates
                .Where(f => f.State != 0)
                .Where(f => f.State == FateState.Preparation || (f.StartTimeEpoch != 0 && f.Duration != 0 && f.TimeRemaining != 0))
                .Where(f => f.Position.X != 0 || f.Position.Y != 0 || f.Position.Z != 0)
                .Select(f => f.ToSonarFateRelay(this.Player.Place))
                .ToList();

            this.Tracker.FeedRelays(fates);

            // Determine and mark disappeared fates as failed
            var currentFateKeys = fates.Select(f => f.RelayKey).ToList();
            var missingFates = this._lastFateKeys.Except(currentFateKeys);
            if (missingFates.Any())
            {
                var fateStates = this.Tracker.Data.States;
                foreach (var fateKey in missingFates)
                {
                    var fateState = fateStates.GetValueOrDefault(fateKey);
                    if (fateState is null) continue;
                    var fate = fateState.Relay.Clone();
                    fate.Status = FateStatus.Failed;
                    this.Tracker.FeedRelay(fate);
                }
            }
            this._lastFateKeys = currentFateKeys;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.Plugin.OnFrameworkEvent += this.Framework;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.Plugin.OnFrameworkEvent -= this.Framework;
            return Task.CompletedTask;
        }
    }
}
