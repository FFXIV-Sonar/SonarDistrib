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

namespace SonarPlugin.Notifiers
{
    public sealed class HuntNotifier : IHostedService
    {
        private const double SSMinionNotificationThreshold = EarthHour;
        private readonly Dictionary<string, double> _lastSSMinionSignals = new();

        private SonarPlugin Plugin { get; }
        private HuntTracker Tracker { get; }
        private PlayerProvider Player { get; }
        private ChatGui Chat { get; }
        private AudioPlaybackEngine Audio { get; }

        public HuntNotifier(SonarPlugin plugin, HuntTracker tracker, PlayerProvider player, ChatGui chat, AudioPlaybackEngine audio)
        {
            this.Plugin = plugin;
            this.Tracker = tracker;
            this.Player = player;
            this.Chat = chat;
            this.Audio = audio;

            PluginLog.LogInformation("Hunt Notifier Initialized");
        }

        private void HuntFound(RelayState<HuntRelay> state)
        {
            if (!this.Player.IsLoggedIn) return;
            if (this.Plugin.Configuration.SSMinionReportingMode == NotifyMode.Single && state.GetRank() == HuntRank.SSMinion && !this.CheckSSMinionSpawn(state)) return;
            var allowChat = !(this.Plugin.IsDuty && this.Plugin.Configuration.DisableChatInDuty);
            var allowSound = !(this.Plugin.IsDuty && this.Plugin.Configuration.DisableSoundInDuty);

            if (allowChat && this.Plugin.Configuration.EnableGameChatReports)
            {
                this.SendToChat(state);
            }

            if (allowSound && this.Plugin.Configuration.PlaySoundARanks && state.Relay.GetRank() == HuntRank.A)
            {
                try { this.Audio.PlaySound(this.Plugin.Configuration.SoundFileARanks ?? string.Empty); } catch (Exception ex) { PluginLog.LogError(ex, "Exception playing sound"); }
            }

            if (allowSound && this.Plugin.Configuration.PlaySoundSRanks && (state.Relay.GetRank() == HuntRank.S || state.Relay.GetRank() == HuntRank.SS || state.Relay.GetRank() == HuntRank.SSMinion))
            {
                try { this.Audio.PlaySound(this.Plugin.Configuration.SoundFileSRanks ?? string.Empty); } catch (Exception ex) { PluginLog.LogError(ex, "Exception playing sound"); }
            }
        }

        private void HuntDead(RelayState<HuntRelay> state)
        {
            if (!this.Player.IsLoggedIn) return;
            var allowChat = !(this.Plugin.IsDuty && this.Plugin.Configuration.DisableChatInDuty);
            if (allowChat && this.Plugin.Configuration.EnableGameChatReports && this.Plugin.Configuration.EnableGameChatReportsDeaths)
            {
                this.SendToChat(state);
            }
        }

        public void SendToChat(RelayState<HuntRelay> state, XivChatType type = XivChatType.None) => this.SendToChat(state.Relay, type);

        public void SendToChat(HuntRelay relay, XivChatType type = XivChatType.None)
        {
            if (type == XivChatType.None) type = this.Plugin.Configuration.HuntOutputChannel;
            if (type == XivChatType.None) type = XivChatType.Echo;
            var cwIcon = this.Plugin.Configuration.EnableGameChatCrossworldIcon && this.Player.Place.WorldId != relay.WorldId;

            var builder = new SeStringBuilder();
            if (this.Plugin.Configuration.EnableGameChatItalicFont) builder.AddItalicsOn();
            builder.AddSeString(relay.GetMapLinkSeString(cwIcon));
            if (relay.IsDead()) builder.AddText(" was just killed");
            if (this.Plugin.Configuration.EnableGameChatItalicFont) builder.AddItalicsOff();

            this.Chat.PrintChat(new()
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
