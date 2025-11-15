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
using Sonar;
using SonarPlugin.Sounds;

namespace SonarPlugin.Notifiers
{
    public sealed class FateNotifier : IHostedService
    {
        private SonarPlugin Plugin { get; }
        private IClientState ClientState { get; }
        private SonarClient Client { get; }
        private IRelayTracker<FateRelay> Tracker { get; }
        private PlayerProvider Player { get; }
        private IChatGui Chat { get; }
        private SoundEngine Sounds { get; }
        private IPluginLog Logger { get; }
        
        public FateNotifier(SonarPlugin plugin, IClientState clientState, SonarClient client, IRelayTracker<FateRelay> tracker, PlayerProvider player, IChatGui chat, SoundEngine sounds, IPluginLog logger)
        {
            this.Plugin = plugin;
            this.ClientState = clientState;
            this.Client = client;
            this.Tracker = tracker;
            this.Player = player;
            this.Chat = chat;
            this.Sounds = sounds;
            this.Logger = logger;

            this.Logger.Information("Fate Notifier Initialized");
        }

        private void FateFound(RelayState<FateRelay> state)
        {
            if (!this.ClientState.IsLoggedIn) return;
            var allowChat = !(this.Plugin.IsDuty && this.Plugin.Configuration.DisableChatInDuty);
            var allowSound = !(this.Plugin.IsDuty && this.Plugin.Configuration.DisableSoundInDuty);

            if (allowChat && this.Plugin.Configuration.EnableFateChatReports && this.Plugin.Configuration.SendFateToChat.Contains(state.Relay.Id)) this.SendToChat(state);
            if (allowSound && this.Plugin.Configuration.SendFateToSound.Contains(state.Relay.Id)) this.Sounds.PlaySound(this.Plugin.Configuration.SoundFileFatesConfig);
        }

        public void SendToChat(RelayState<FateRelay> state, XivChatType type = XivChatType.None) => this.SendToChat(state.Relay, type);
        public void SendToChat(FateRelay relay, XivChatType type = XivChatType.None)
        {
            if (type == XivChatType.None) type = this.Plugin.Configuration.FateOutputChannel;
            if (type == XivChatType.None) type = XivChatType.Echo;
            var cwIcon = this.Plugin.Configuration.EnableGameChatCrossworldIcon && this.Client.Meta.PlayerPosition?.WorldId != relay.WorldId;

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
