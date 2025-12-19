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
using DryIoc.FastExpressionCompiler.LightExpression;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using System.Text;
using System.Collections.Generic;
using Sonar.Numerics;
using AG.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace SonarPlugin.Trackers
{
    public sealed class PlayerProvider : IHostedService
    {
        private SonarPlugin Plugin { get; }
        private SonarClient Client { get; }
        private IClientState ClientState { get; }
        private IObjectTable ObjectTable { get; }
        private IPlayerState PlayerState { get; }
        private IPluginLog Logger { get; }
        public UnorderedRefList<Vector3> PlayerPositions { get; } = [];

        public PlayerProvider(SonarPlugin plugin, SonarClient client, IClientState clientState, IObjectTable objectTable, IPlayerState playerState, IPluginLog logger)
        {
            this.Plugin = plugin;
            this.Client = client;
            this.ClientState = clientState;
            this.ObjectTable = objectTable;
            this.PlayerState = playerState;
            this.Logger = logger;
            this.Logger.Information("PlayerTracker initialized");
        }

        // https://github.com/goatcorp/Dalamud/pull/1078#issuecomment-1382729843
        private static unsafe uint GetCurrentInstance() => UIState.Instance()->PublicInstance.InstanceId;

        private void FrameworkTick(IFramework framework)
        {
            // Don't proceed if the structures aren't ready
            if (!this.Plugin.SafeToReadTables) return;

            // Player Information
            var player = this.ObjectTable.LocalPlayer;
            var loggedIn = this.ClientState.IsLoggedIn;
            var info = new PlayerInfo() { LoggedIn = loggedIn, Name = player?.Name.TextValue ?? null, HomeWorldId = player?.HomeWorld.RowId ?? 0, Hash1 = this.GetContentHash(), Hash2 = this.GetAccountHash() };

            if (this.Client.Meta.UpdatePlayerInfo(info))
            {
                if (loggedIn) this.Logger.Verbose("Logged in as {player:X16}", AG.SplitHash64.Compute(info.ToString()));
                else this.Logger.Verbose("Logged out");
            }

            // Player Place
            if (loggedIn && player is not null)
            {
                var place = new PlayerPosition() { WorldId = player.CurrentWorld.RowId, ZoneId = this.ClientState.TerritoryType, InstanceId = GetCurrentInstance(), Coords = player.Position.SwapYZ() };
                if (this.Client.Meta.UpdatePlayerPosition(place).PlaceUpdated) this.Logger.Verbose("Moved to {place}", place);
            }

            // Player Positions data
            var positions = this.ObjectTable
                .OfType<IPlayerCharacter>()
                .Select(player => player.Position);

            // Nearby player counts logic
            var count = 0;
            foreach (var position in positions)
            {
                if (this.PlayerPositions.Count <= count) this.PlayerPositions.Add(position);
                else this.PlayerPositions[count] = position;
                count++;
            }
            this.PlayerPositions.Count = count;
        }

        private long GetContentHash()
        {
            if (!this.ClientState.IsLoggedIn) return 0;
            return AG.SplitHash64.Compute(this.PlayerState.ContentId);
        }

        private unsafe long GetAccountHash()
        {
            if (!this.ClientState.IsLoggedIn) return 0;
            var characterManager = CharacterManager.Instance();
            if (characterManager is null) return 0;
            var character = characterManager->BattleCharas[0].Value;
            if (&character is null) return 0;
            return AG.SplitHash64.Compute(character->AccountId);
        }

        public int GetNearbyPlayerCount() => this.PlayerPositions.Count;
        public int GetNearbyPlayerCount(Vector3 from, float distanceSquared = 50 * 50) => this.PlayerPositions.Count(position => Vector3.DistanceSquared(from, position) <= distanceSquared);

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
