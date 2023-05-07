using Sonar.Extensions;
using Dalamud.Logging;
using CheapLoc;
using SonarPlugin.Config;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Network;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.IoC;
using Dalamud.Plugin;
using ImGuiNET;
using SonarPlugin.Managers;
using Sonar;
using Sonar.Data.Extensions;
using Sonar.Enums;
using Sonar.Models;
using Sonar.Services;
using Sonar.Sockets;
using Sonar.Utilities;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using SonarPlugin.Trackers;
using SonarPlugin.Utility;
using static Sonar.SonarConstants;
using SonarPlugin.Game;
using Sonar.Data;
using Sonar.Logging;
using Dalamud.Interface.Windowing;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace SonarPlugin
{
    [SingletonService]
    public sealed class SonarPlugin : IDisposable
    {
        private DalamudPluginInterface PluginInterface { get; }
        private SonarClient Client { get; }
        private Framework Framework { get; }
        private ChatGui Chat { get; }
        private Condition Condition { get; }
        private Localization Localization { get; }
        private AudioPlaybackEngine Audio { get; }

        public bool IsDuty { get; private set; }

        public SonarPlugin(DalamudPluginInterface pluginInterface, SonarClient client, Framework framework, ChatGui chat, Condition condition, Localization localization, AudioPlaybackEngine audio)
        {
            this.PluginInterface = pluginInterface;
            this.Client = client;
            this.Framework = framework;
            this.Chat = chat;
            this.Condition = condition;
            this.Localization = localization;
            this.Audio = audio;

            this.Initialize();
        }

        public WindowSystem Windows { get; } = new(nameof(SonarPlugin));
        public SonarConfiguration Configuration { get; private set; } = default!;

        public void Initialize()
        {
            this.LoadConfiguration();
            this.Localization.SetupLocalization(this.Configuration.Language);

            this.PluginInterface.UiBuilder.Draw += this.Windows.Draw;

            // Set volume of alerts to current config, this also will initialize the Instance of the audio service
            this.Audio.Volume = this.Configuration.SoundVolume;

            // Framework OnUpdateEvent handlers
            this.Framework.Update += this.Framework_OnUpdateEvent;

            // Start Sonar.NET client
            this.Client.ServerMessage += this.Events_OnSonarMessage;
            this.Client.LogMessage += this.ClientLogHandler;
            this.Client.Start();
        }

        private void Events_OnSonarMessage(SonarClient source, string? message)
        {
            if (message is null) return;
            this.Chat.PrintChat(new()
            {
                Type = this.Configuration.HuntOutputChannel,
                Name = "Sonar",
                Message = message
            });
            PluginLog.LogInformation("Sonar Message Received: {message}");
        }

        #region OnBuildUi and Framework event and tick manager
        public bool SafeToReadTables { get; private set; }

        // FrameworkEvent
        public event Action<Framework>? OnFrameworkEvent;

        private void Framework_OnUpdateEvent(Framework framework)
        {
            this.SafeToReadTables = !this.Condition[ConditionFlag.BetweenAreas51]; // TODO: Move the condition checks to their respective places
            this.IsDuty = this.Condition[ConditionFlag.BoundByDuty56];
            DispatchFrameworkEvent(nameof(this.OnFrameworkEvent), this.OnFrameworkEvent, framework);
        }

        private static void DispatchFrameworkEvent(string name, Action<Framework>? ev, Framework framework)
        {
            if (ev is null) return;
            ev.SafeInvoke(framework, out var exceptions);
            foreach (var ex in exceptions) FrameworkErrorHandler($"{name} Exception", ex);
        }

        private static void FrameworkErrorHandler(string name, Exception ex)
        {
            if (ex is AggregateException aex) ex = aex;
            PluginLog.LogError(ex, $"{name} Exception");
        }
        #endregion

        [SuppressMessage("Major Code Smell", "S112", Justification = "No suitable exception")]
        public void LoadConfiguration(bool isReset = false)
        {
            try
            {
                this.Configuration = (SonarConfiguration)this.PluginInterface.GetPluginConfig()!;
                if (this.Configuration is null)
                {
                    if (isReset) throw new Exception($"Failed resetting configuration");
                    this.ResetConfiguration();
                    return;
                }

                if (this.Configuration.Version < SonarConfiguration.SonarConfigurationVersion)
                {
                    this.Configuration.PerformVersionUpdate();
                    this.SaveConfiguration(true);
                }
                else if (this.Configuration.Version > SonarConfiguration.SonarConfigurationVersion)
                {
                    PluginLog.LogWarning($"Your Sonar configuration v{this.Configuration.Version} is from the future! Please turn off your time machine and go back to v{SonarConfiguration.SonarConfigurationVersion}.");
                }
                this.Configuration.Sanitize();
                this.Client.Configuration = this.Configuration.SonarConfig;
            }
            catch (Exception e)
            {
                PluginLog.LogError($"Failed to load configuration: {e}");
                this.ResetConfiguration(); // TODO: Potential infinite recursion
            }
        }

        public void SaveConfiguration(bool updateServer = false)
        {
            try
            {
                if (updateServer) this.Client.Configuration = this.Client.Configuration;
                this.Configuration.SonarConfig = this.Client.Configuration;
                this.PluginInterface.SavePluginConfig(this.Configuration);
            }
            catch (Exception e)
            {
                PluginLog.LogError($"Failed to save configuration: {e}");
            }
        }

        public void ResetConfiguration()
        {
            this.Configuration = new SonarConfiguration();
            this.Client.Configuration = new Sonar.Config.SonarConfig();
            this.Client.Configuration.HuntConfig.Contribute = true;
            this.Client.Configuration.FateConfig.Contribute = true;
            this.SaveConfiguration(true);
            this.LoadConfiguration(true);
        }

        private void ClientLogHandler(SonarClient source, SonarLogMessage log) => this.LogHandler(log);

        private void LogHandler(SonarLogMessage log)
        {
            var (level, message) = (log.Level, log.Message);
            switch (level)
            {
                case SonarLogLevel.Verbose:
                    PluginLog.LogVerbose(message);
                    break;
                case SonarLogLevel.Debug:
                    PluginLog.LogDebug(message);
                    break;
                case SonarLogLevel.Information:
                    PluginLog.LogInformation(message);
                    break;
                case SonarLogLevel.Warning:
                    PluginLog.LogWarning(message);
                    break;
                case SonarLogLevel.Error:
                    PluginLog.LogError(message);
                    break;
                case SonarLogLevel.Fatal:
                    PluginLog.LogFatal(message);
                    break;
                default:
                    PluginLog.Log(message);
                    break;
            }
        }

        #region IDisposable Support
        private int _disposed; // Interlocked
        public bool IsDisposed => this._disposed != 0;

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref this._disposed, 1, 0) != 0) return;
            this.SaveConfiguration();

            // Hunt and Fate Trackers
            if (this.Client is not null)
            {
                this.Client.ServerMessage -= Events_OnSonarMessage;
                this.Client.LogMessage -= ClientLogHandler;
            }

            if (this.PluginInterface is not null)
            {
                // Logged in / out handlers
                this.PluginInterface.UiBuilder.Draw -= this.Windows.Draw;
                this.Framework.Update -= this.Framework_OnUpdateEvent;
            }
        }
        #endregion
    }
}
