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
using Dalamud.Plugin.Services;

namespace SonarPlugin.Trackers
{
    public sealed class PlayerProvider : IHostedService
    {
        private SonarPlugin Plugin { get; }
        private SonarClient Client { get; }
        private IClientState ClientState { get; }
        private IObjectTable ObjectTable { get; }
        private IPluginLog Logger { get; }

        public PlayerProvider(SonarPlugin plugin, SonarClient client, IClientState clientState, IObjectTable objectTable, IPluginLog logger)
        {
            this.Plugin = plugin;
            this.Client = client;
            this.ClientState = clientState;
            this.ObjectTable = objectTable;
            this.Logger = logger;
            this.Logger.Information("PlayerTracker initialized");
        }

        private unsafe uint GetCurrentInstance()
        {
            // https://github.com/goatcorp/Dalamud/pull/1078#issuecomment-1382729843
            return (uint)UIState.Instance()->PublicInstance.InstanceId;
        }

        private void FrameworkTick(IFramework framework)
        {
            // Don't proceed if the structures aren't ready
            if (!this.Plugin.SafeToReadTables) return;

            // Player Information
            var player = this.ClientState.LocalPlayer;
            var loggedIn = this.ClientState.IsLoggedIn;
            if ((player is null && loggedIn) || (player is not null && !loggedIn)) this.Logger.Warning("Inconsistent logged in status detected");
            var info = new PlayerInfo() { LoggedIn = loggedIn, Name = player?.Name.TextValue ?? null, HomeWorldId = player?.HomeWorld.Id ?? 0, Hash = loggedIn ? AG.SplitHash64.Compute(this.ClientState.LocalContentId) : 0 };
            if (this.Client.Meta.UpdatePlayerInfo(info))
            {
                if (loggedIn) this.Logger.Verbose("Logged in as {player:X16}", AG.SplitHash64.Compute(info.ToString()));
                else this.Logger.Verbose("Logged out");
            }

            // Player Place
            if (loggedIn && player is not null)
            {
                var place = new PlayerPosition() { WorldId = player.CurrentWorld.Id, ZoneId = this.ClientState.TerritoryType, InstanceId = this.GetCurrentInstance(), Coords = player.Position.SwapYZ() };
                if (this.Client.Meta.UpdatePlayerPosition(place).PlaceUpdated) this.Logger.Verbose("Moved to {place}", place);
            }

            // Players nearby count
            this.PlayerCount = this.ObjectTable
                .OfType<IPlayerCharacter>()
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
