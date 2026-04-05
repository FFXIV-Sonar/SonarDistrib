using AG.Collections.Generic;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Logging;
using Dalamud.Memory;
using Dalamud.Plugin.Services;
using DryIoc.FastExpressionCompiler.LightExpression;
using DryIocAttributes;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Microsoft.Extensions.Hosting;
using Sonar;
using Sonar.Models;
using Sonar.Numerics;
using SonarPlugin.Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSVector3 = FFXIVClientStructs.FFXIV.Common.Math.Vector3;

namespace SonarPlugin.Trackers
{
    [ExportMany]
    [SingletonReuse]
    public sealed class PlayerProvider : IHostedService
    {
        private SonarPlugin Plugin { get; }
        private SonarClient Client { get; }
        private IClientState ClientState { get; }
        private IPluginLog Logger { get; }

        public PlayerProvider(SonarPlugin plugin, SonarClient client, IClientState clientState, IPluginLog logger)
        {
            this.Plugin = plugin;
            this.Client = client;
            this.ClientState = clientState;
            this.Logger = logger;
            this.Logger.Information("PlayerTracker initialized");
        }

        private unsafe void FrameworkTick(IFramework framework)
        {
            // Don't proceed if the structures aren't ready
            if (!this.Plugin.SafeToReadTables) return;

            // Player Information
            var player = GetPlayerCharacter();
            var info = player is not null ? new PlayerInfo() { LoggedIn = true, Name = SeString.Parse(player->Name).TextValue, HomeWorldId = player->HomeWorld, Hash1 = AG.SplitHash64.Compute(player->ContentId), Hash2 = AG.SplitHash64.Compute(player->AccountId) } : new PlayerInfo() { LoggedIn = false, Name = null, HomeWorldId = 0, Hash1 = 0, Hash2 = 0 };
            if (this.Client.Meta.UpdatePlayerInfo(info))
            {
                if (info.LoggedIn is true) this.Logger.Verbose("Logged in as {player:X16}", AG.SplitHash64.Compute(info.ToString()));
                else this.Logger.Verbose("Logged out");
            }

            // Player Place
            if (player is not null)
            {
                var place = new PlayerPosition() { WorldId = player->CurrentWorld, ZoneId = this.ClientState.TerritoryType, InstanceId = this.ClientState.Instance, Coords = Unsafe.As<CSVector3, Vector3>(ref player->Position).SwapYZ() };
                if (this.Client.Meta.UpdatePlayerPosition(place).PlaceUpdated) this.Logger.Verbose("Moved to {place}", place);
            }
        }

        private static unsafe BattleChara* GetPlayerCharacter()
        {
            var manager = CharacterManager.Instance();
            if (manager is null) return null;
            return manager->BattleCharas[0].Value;
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
