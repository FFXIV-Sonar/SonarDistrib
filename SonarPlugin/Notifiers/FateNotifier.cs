using Dalamud.Game.Gui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Game.Text.SeStringHandling;
using Sonar.Trackers;
using SonarPlugin.Config;
using SonarPlugin.Trackers;
using SonarPlugin.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SonarPlugin.Game;
using Sonar.Data.Extensions;
using Dalamud.Logging;
using Sonar.Relays;
using Dalamud.Plugin.Services;

namespace SonarPlugin.Notifiers
{
    public sealed class FateNotifier : IHostedService
    {
        private SonarPlugin Plugin { get; }
        private IRelayTracker<FateRelay> Tracker { get; }
        private PlayerProvider Player { get; }
        private IChatGui Chat { get; }
        private AudioPlaybackEngine Audio { get; }
        private IPluginLog Logger { get; }
        
        public FateNotifier(SonarPlugin plugin, IRelayTracker<FateRelay> tracker, PlayerProvider player, IChatGui chat, AudioPlaybackEngine audio, IPluginLog logger)
        {
            this.Plugin = plugin;
            this.Tracker = tracker;
            this.Player = player;
            this.Chat = chat;
            this.Audio = audio;
            this.Logger = logger;

            this.Logger.Information("Fate Notifier Initialized");
        }

        private void FateFound(RelayState<FateRelay> state)
        {
            if (!this.Player.IsLoggedIn) return;
            var allowChat = !(this.Plugin.IsDuty && this.Plugin.Configuration.DisableChatInDuty);
            var allowSound = !(this.Plugin.IsDuty && this.Plugin.Configuration.DisableSoundInDuty);

            if (allowChat && this.Plugin.Configuration.EnableFateChatReports && this.Plugin.Configuration.SendFateToChat.Contains(state.Relay.Id))
            {
                this.SendToChat(state);
            }

            if (allowSound && this.Plugin.Configuration.PlaySoundFates && this.Plugin.Configuration.SendFateToSound.Contains(state.Relay.Id))
            {
                try { this.Audio.PlaySound(this.Plugin.Configuration.SoundFileFates ?? string.Empty); } catch (Exception ex) { this.Logger.Error(ex, "Exception playing sound"); }
            }
        }

        public void SendToChat(RelayState<FateRelay> state, XivChatType type = XivChatType.None) => this.SendToChat(state.Relay, type);
        public void SendToChat(FateRelay relay, XivChatType type = XivChatType.None)
        {
            if (type == XivChatType.None) type = this.Plugin.Configuration.FateOutputChannel;
            if (type == XivChatType.None) type = XivChatType.Echo;
            var cwIcon = this.Plugin.Configuration.EnableGameChatCrossworldIcon && this.Player.Place.WorldId != relay.WorldId;

            var builder = new SeStringBuilder();
            if (this.Plugin.Configuration.EnableFateChatItalicFont) builder.AddItalicsOn();
            builder.AddSeString(relay.GetMapLinkSeString(cwIcon));
            if (relay.IsDead()) builder.AddText(" was just killed");
            if (this.Plugin.Configuration.EnableFateChatItalicFont) builder.AddItalicsOff();

            this.Chat.Print(new()
            {
                Type = type,
                Name = "Sonar",
                Message = builder.Build()
            });
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.Tracker.Found += this.FateFound;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.Tracker.Found -= this.FateFound;
            return Task.CompletedTask;
        }
    }
}
