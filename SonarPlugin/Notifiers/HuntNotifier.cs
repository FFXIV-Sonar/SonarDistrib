using Sonar.Trackers;
using System;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.Text;
using SonarPlugin.Config;
using SonarPlugin.Trackers;
using Dalamud.Game.Gui;
using Sonar.Data.Extensions;
using Dalamud.Logging;
using Sonar.Enums;
using CheapLoc;
using SonarPlugin.Utility;
using System.Collections.Generic;
using Dalamud.Game.Text.SeStringHandling;
using SonarPlugin.Game;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using System.Diagnostics;
using static Sonar.SonarConstants;
using Sonar.Relays;
using SonarPlugin.GUI;
using Dalamud.Plugin.Services;
using Sonar;
using SonarPlugin.Sounds;

namespace SonarPlugin.Notifiers
{
    public sealed class HuntNotifier : IHostedService
    {
        private const double SSMinionNotificationThreshold = EarthHour;
        private readonly Dictionary<string, double> _lastSSMinionSignals = new();

        private SonarPlugin Plugin { get; }
        private IClientState ClientState { get; }
        private SonarClient Client { get; }
        private IRelayTracker<HuntRelay> Tracker { get; }
        private IChatGui Chat { get; }
        private SoundEngine Sounds { get; }
        private IPluginLog Logger { get; }

        public HuntNotifier(SonarPlugin plugin, IClientState clientState, SonarClient client, RelayTrackerViews views, IChatGui chat, SoundEngine sounds, IPluginLog logger)
        {
            this.Plugin = plugin;
            this.ClientState = clientState;
            this.Client = client;
            this.Tracker = views.Hunts;
            this.Chat = chat;
            this.Sounds = sounds;
            this.Logger = logger;

            this.Logger.Information("Hunt Notifier Initialized");
        }

        private void HuntFound(RelayState<HuntRelay> state)
        {
            if (!this.ClientState.IsLoggedIn) return;
            if (this.Plugin.Configuration.SSMinionReportingMode == NotifyMode.Single && state.GetRank() == HuntRank.SSMinion && !this.CheckSSMinionSpawn(state)) return;
            var allowChat = !(this.Plugin.IsDuty && this.Plugin.Configuration.DisableChatInDuty);
            var allowSound = !(this.Plugin.IsDuty && this.Plugin.Configuration.DisableSoundInDuty);

            if (allowChat && this.Plugin.Configuration.EnableGameChatReports) this.SendToChat(state);

            if (allowSound)
            {
                if (state.Relay.GetRank() == HuntRank.B) this.Sounds.PlaySound(this.Plugin.Configuration.SoundFileBRanksConfig);
                if (state.Relay.GetRank() == HuntRank.A) this.Sounds.PlaySound(this.Plugin.Configuration.SoundFileARanksConfig);
                if (state.Relay.GetRank() is HuntRank.S or HuntRank.SS or HuntRank.SSMinion) this.Sounds.PlaySound(this.Plugin.Configuration.SoundSRanksConfig);
            }

        }

        private void HuntDead(RelayState<HuntRelay> state)
        {
            if (!this.ClientState.IsLoggedIn) return;
            var allowChat = !(this.Plugin.IsDuty && this.Plugin.Configuration.DisableChatInDuty);
            if (allowChat && this.Plugin.Configuration.EnableGameChatReports && this.Plugin.Configuration.EnableGameChatReportsDeaths) this.SendToChat(state);
        }

        public void SendToChat(RelayState<HuntRelay> state, XivChatType type = XivChatType.None) => this.SendToChat(state.Relay, type);

        public void SendToChat(HuntRelay relay, XivChatType type = XivChatType.None)
        {
            if (type == XivChatType.None) type = this.Plugin.Configuration.HuntOutputChannel;
            if (type == XivChatType.None) type = XivChatType.Echo;
            var cwIcon = this.Plugin.Configuration.EnableGameChatCrossworldIcon && this.Client.Meta.PlayerPosition?.WorldId != relay.WorldId;

            var builder = new SeStringBuilder();
            if (this.Plugin.Configuration.EnableGameChatItalicFont) builder.AddItalicsOn();
            builder.AddSeString(relay.GetMapLinkSeString(cwIcon));
            if (relay.IsDead()) builder.AddText(" was just killed");
            if (this.Plugin.Configuration.EnableGameChatItalicFont) builder.AddItalicsOff();

            this.Chat.Print(new()
            {
                Type = type,
                Name = "Sonar",
                Message = builder.Build()
            });
        }

        public bool CheckSSMinionSpawn(RelayState<HuntRelay> state)
        {
            Debug.Assert(state.GetRank() == HuntRank.SSMinion);
            var relay = state.Relay;
            var placeKey = relay.PlaceKey;

            lock (this._lastSSMinionSignals)
            {
                var spawnTime = state.LastFound;
                var lastSpawn = this._lastSSMinionSignals.GetValueOrDefault(placeKey);
                if (spawnTime > lastSpawn + SSMinionNotificationThreshold)
                {
                    this._lastSSMinionSignals[placeKey] = spawnTime;
                    return true;
                }
                return false;
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.Tracker.Found += this.HuntFound;
            this.Tracker.Dead += this.HuntDead;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.Tracker.Found -= this.HuntFound;
            this.Tracker.Dead -= this.HuntDead;
            return Task.CompletedTask;
        }
    }
}
