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
using Sonar;

namespace SonarPlugin.Trackers
{
    public sealed class PlayerProvider : IHostedService
    {
        private SonarPlugin Plugin { get; }
        private SonarClient Client { get; }
        private ClientState ClientState { get; }
        private ObjectTable ObjectTable { get; }
        private SonarAddressResolver Address { get; } 

        public PlayerProvider(SonarPlugin plugin, SonarClient client, ClientState clientState, ObjectTable objectTable, SonarAddressResolver address)
        {
            this.Plugin = plugin;
            this.Client = client;
            this.ClientState = clientState;
            this.ObjectTable = objectTable;
            this.Address = address;
            PluginLog.LogInformation("PlayerTracker initialized");
        }

        private byte GetCurrentInstance()
        {
            var addr = this.Address.Instance;
            return addr != IntPtr.Zero ? MemoryHelper.Read<byte>(addr) : (byte)0;
        }

        private unsafe int GetCurrentInstance2()
        {
            // TODO once its available and CN catches up
            // https://github.com/goatcorp/Dalamud/pull/1078#issuecomment-1382729843
            // using FFXIVClientStructs.FFXIV.Client.Game.UI; <-- move this out to the top of course
            // UIState.Instance()->AreaInstance.Instance;
            return this.GetCurrentInstance(); // Need to return something
        }

        private void FrameworkTick(Framework framework)
        {
            // Don't proceed if the structures aren't ready
            if (!this.Plugin.SafeToReadTables) return;

            var player = this.ClientState.LocalPlayer;
            this.IsLoggedIn = player is not null;
            if (this.IsLoggedIn)
            {
                // Player Information
                var info = new PlayerInfo() { Name = player!.Name.TextValue, HomeWorldId = player.HomeWorld.Id };
                if (this.Client.UpdatePlayerInfo(info)) PluginLog.LogVerbose("Logged in as {player}", info);

                // Player Place
                var worldId = player.CurrentWorld.Id;
                var zoneId = this.ClientState.TerritoryType;
                var instanceId = this.GetCurrentInstance();
                if (this.Place.WorldId != worldId || this.Place.ZoneId != zoneId || this.Place.InstanceId != instanceId)
                {
                    this.Place = new PlayerPlace() { WorldId = worldId, ZoneId = zoneId, InstanceId = instanceId };
                    this.Client.PlayerPlace = this.Place;
                    PluginLog.LogVerbose("Moved to {place}", this.Place);
                }

                // Players nearby count
                this.PlayerCount = this.ObjectTable
                    .OfType<PlayerCharacter>()
                    .Count();
            }
        }

        #region Event Handlers
        public delegate void VoidDelegate(PlayerProvider source);
        public event VoidDelegate? OnPlayerChanged;
        public event VoidDelegate? OnPlaceChanged;
        #endregion

        #region Debug Functions
        public void DebugResetPlace() => this.Place = new PlayerPlace();
        #endregion

        #region Player Information
        /// <summary>
        /// Player is logged in
        /// </summary>
        public bool IsLoggedIn { get; private set; }
        /// <summary>
        /// Player Location
        /// </summary>
        public PlayerPlace Place { get; private set; } = new PlayerPlace();
        /// <summary>
        /// Surrounding Players Count (including self)
        /// </summary>
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
