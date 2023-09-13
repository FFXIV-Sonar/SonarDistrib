using System;
using Sonar.Models;
using System.Linq;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Memory;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.ClientState;
using SonarPlugin.Game;
using Dalamud.Logging;
using Dalamud.Game.ClientState.Objects;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Sonar;

namespace SonarPlugin.Trackers
{
    public sealed class PlayerProvider : IHostedService
    {
        private SonarPlugin Plugin { get; }
        private SonarClient Client { get; }
        private ClientState ClientState { get; }
        private ObjectTable ObjectTable { get; }

        public PlayerProvider(SonarPlugin plugin, SonarClient client, ClientState clientState, ObjectTable objectTable)
        {
            this.Plugin = plugin;
            this.Client = client;
            this.ClientState = clientState;
            this.ObjectTable = objectTable;
            PluginLog.LogInformation("PlayerTracker initialized");
        }

        private unsafe uint GetCurrentInstance()
        {
            // https://github.com/goatcorp/Dalamud/pull/1078#issuecomment-1382729843
            return (uint)UIState.Instance()->AreaInstance.Instance;
        }

        private void FrameworkTick(Framework framework)
        {
            // Don't proceed if the structures aren't ready
            if (!this.Plugin.SafeToReadTables || !this.ClientState.IsLoggedIn) return;

            var player = this.ClientState.LocalPlayer;

            // Player Information
            var info = new PlayerInfo() { Name = player!.Name.TextValue, HomeWorldId = player.HomeWorld.Id };
            if (this.Client.Meta.UpdatePlayerInfo(info)) PluginLog.LogVerbose("Logged in as {player}", info);

            // Player Place
            var place = new PlayerPosition() { WorldId = player.CurrentWorld.Id, ZoneId = this.ClientState.TerritoryType, InstanceId = this.GetCurrentInstance(), Coords = player.Position.SwapYZ() };
            if (this.Client.Meta.UpdatePlayerPosition(place).PlaceUpdated) PluginLog.LogVerbose("Moved to {place}", place);

            // Players nearby count
            this.PlayerCount = this.ObjectTable
                .OfType<PlayerCharacter>()
                .Count();
        }

        #region Player Information
        /// <summary>Player is logged in</summary>
        [Obsolete("Use ClientState.IsLoggedIn instead")]
        public bool IsLoggedIn => this.ClientState.IsLoggedIn;

        /// <summary>Player Location</summary>
        [Obsolete("Use SonarClient.Meta.PlayerPosition instead")]
        public PlayerPosition Place => this.Client.Meta.PlayerPosition ?? new(); // TODO: Fix this

        /// <summary>Surrounding Players Count (including self)</summary>
        public int PlayerCount { get; private set; }
        #endregion

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
