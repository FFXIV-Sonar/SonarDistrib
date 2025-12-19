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
using Dalamud.Plugin.Services;
using Sonar.Relays;
using Sonar;

namespace SonarPlugin.Trackers
{
    public sealed class SonarFateProvider : IHostedService
    {
        private PlayerProvider Player { get; }
        private SonarPlugin Plugin { get; }
        private SonarClient Client { get; }
        private IRelayTracker<FateRelay> Tracker { get; }
        private IFateTable Fates { get; }
        private IClientState ClientState { get; }
        private IPluginLog Logger { get; }

        /// <summary>
        /// Initialize fate tracker
        /// </summary>
        /// <param name="plugin">Sonar Plugin object</param>
        /// <param name="debug">(Optional) Output debug logging</param>
        public SonarFateProvider(PlayerProvider player, SonarPlugin plugin, SonarClient client, IRelayTracker<FateRelay> tracker, IFateTable fates, IClientState clientState, IPluginLog logger)
        {
            // Get Sonar and Plugin Interface
            this.Player = player;
            this.Plugin = plugin;
            this.Client = client;
            this.Tracker = tracker;
            this.Fates = fates;
            this.ClientState = clientState;
            this.Logger = logger;

            // Initialization feedback
            this.Logger.Information("FateTracker Initialized");
        }

        private List<string> _lastFateKeys = new();
        private void Framework(IFramework framework)
        {
            // Don't proceed if the structures aren't ready
            if (!this.Plugin.SafeToReadTables || !this.ClientState.IsLoggedIn)
            {
                this._lastFateKeys.Clear();
                return;
            }

            // Get player position information
            var playerPosition = this.Client.Meta.PlayerPosition;
            if (playerPosition is null) return;

            // Iterate throughout all fates in the fates table
            var fates = this.Fates
                .Where(f => f.State != 0)
                .Where(f => f.State == FateState.Preparation || (f.StartTimeEpoch is not 0 && f.Duration is not 0 && f.TimeRemaining is not 0))
                .Where(f => f.Position.X is not 0 || f.Position.Y is not 0 || f.Position.Z is not 0)
                .Select(f => f.ToSonarFateRelay(playerPosition, this.Player.GetNearbyPlayerCount(f.Position)))
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
