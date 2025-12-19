using System.Linq;
using SonarPlugin.Game;
using Sonar.Models;
using Sonar.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.Types;
using System.Threading;
using System.Threading.Tasks;
using Sonar.Trackers;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Logging;
using Dalamud.Plugin.Services;
using Sonar.Relays;
using Sonar;

namespace SonarPlugin.Trackers
{
    public sealed class SonarHuntProvider : IHostedService
    {
        private PlayerProvider Player { get; }
        private IRelayTracker<HuntRelay> Tracker { get; }
        private IObjectTable Table { get; }
        private SonarPlugin Plugin { get; }
        private SonarClient Client { get; }
        private IClientState ClientState { get; }
        private IPluginLog Logger { get; }

        /// <summary>
        /// Initialize monster tracker
        /// </summary>
        public SonarHuntProvider(PlayerProvider player, IRelayTracker<HuntRelay> tracker, IObjectTable table, SonarPlugin plugin, SonarClient client, IClientState clientState, IPluginLog logger)
        {
            // Get Sonar and Plugin Interface
            this.Player = player;
            this.Tracker = tracker;
            this.Table = table;
            this.Plugin = plugin;
            this.Client = client;
            this.ClientState = clientState;
            this.Logger = logger;

            // Initialization feedback
            this.Logger.Information("MobTracker Initialized");
        }

        private void FrameworkTick(IFramework framework)
        {
            // Don't proceed if the structures aren't ready
            if (!this.Plugin.SafeToReadTables || !this.ClientState.IsLoggedIn) return;

            // Get player position information
            var playerPosition = this.Client.Meta.PlayerPosition;
            if (playerPosition is null) return;

            // Iterate throughout all hunts in the actor table
            var hunts = this.Table
                .OfType<IBattleNpc>()
                .Where(a => Database.Hunts.ContainsKey(a.NameId))
                .Select(h => h.ToSonarHuntRelay(playerPosition, this.Player.GetNearbyPlayerCount(h.Position)));

            this.Tracker.FeedRelays(hunts);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.Plugin.OnFrameworkEvent += this.FrameworkTick;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.Plugin.OnFrameworkEvent -= this.FrameworkTick;
            return Task.CompletedTask;
        }
    }
}
