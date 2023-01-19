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
using static Sonar.Constants;
using SonarPlugin.Game;
using Sonar.Data;
using Sonar.Logging;
using SonarGUI;
using Collections.Pooled;
using Dalamud.Interface.Windowing;
using System.Diagnostics;

namespace SonarPlugin
{
    [SingletonService]
    public sealed class SonarPlugin : IDisposable
    {
        private DalamudPluginInterface PluginInterface { get; }
        private SonarClient Client { get; }
        private Framework Framework { get; }
        private Condition Condition { get; }
        private Localization Localization { get; }
        private AudioPlaybackEngine Audio { get; }

        public bool IsDuty { get; private set; }

        public SonarPlugin(DalamudPluginInterface pluginInterface, SonarClient client, SonarAddressResolver address, Framework framework, Condition condition, Localization localization, AudioPlaybackEngine audio)
        {
            this.PluginInterface = pluginInterface;
            this.Client = client;
            this.Framework = framework;
            this.Condition = condition;
            this.Localization = localization;
            this.Audio = audio;

            this.Initialize();
        }

        public WindowSystem Windows { get; } = new(nameof(SonarPlugin));
        public SonarConfiguration Configuration { get; private set; } = default!;
        public SonarGUIService SonarGUI { get; private set; } = default!; // TODO: Remove this (after porting SupportWindow)

        public void Initialize()
        {
            this.LoadConfiguration();
            this.Localization.SetupLocalization(this.Configuration.Language);

            this.SonarGUI = new(this.Client);
            this.SonarGUI.LogMessage += this.GUILogHandler;
            this.PluginInterface.UiBuilder.Draw += this.SonarGUI.Draw;
            this.PluginInterface.UiBuilder.Draw += this.Windows.Draw;

            // Set volume of alerts to current config, this also will initialize the Instance of the audio service
            this.Audio.Volume = this.Configuration.SoundVolume;

            // Framework OnUpdateEvent handlers
            this.Framework.Update += this.Framework_OnUpdateEvent;
            this.PluginInterface.UiBuilder.Draw += this.UiBuilder_OnBuildUi;

            // Start Sonar.NET client
            this.Client.ServerMessage += this.Events_OnSonarMessage;
            this.Client.LogMessage += this.ClientLogHandler;
            this.Client.ClientConnected += this.Client_OnClientConnected;
            this.Client.Ready += this.Client_OnReady;
            this.Client.ClientDisconnected += this.Client_OnClientDisconnected;
            this.Client.Start();

            // FrameworkTick
            this.Client.Tick += this.Ticker_OnTick;
        }

        private void Events_OnSonarMessage(SonarClient source, string? message)
        {
            /* TODO */
            PluginLog.LogWarning("Events_OnSonarMessage not implemented, received: {message}");
        }

        #region OnBuildUi and Framework event and tick manager
        public bool SafeToReadTables { get; private set; }
        private int ProcessFramework; // Interlocked

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3264:Events should be invoked", Justification = "Invoked via GetInvocationList")]
        public event ImGuiScene.RawDX11Scene.BuildUIDelegate? OnBuildUi;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S3217:\"Explicit\" conversions of \"foreach\" loops should not be used", Justification = "Event Handler")]
        private void UiBuilder_OnBuildUi()
        {
            this.SafeToReadTables = !this.Condition[ConditionFlag.BetweenAreas51];
            this.IsDuty = this.Condition[ConditionFlag.BoundByDuty56];
            if (this.OnBuildUi != null)
            {
                foreach (ImGuiScene.RawDX11Scene.BuildUIDelegate del in this.OnBuildUi.GetInvocationList())
                {
                    string id = $"{del.Method.DeclaringType!.FullName}.{del.Method.Name}";
                    try
                    {
                        ImGui.PushID(id);
                        del.Invoke();
                    }
                    catch (Exception ex)
                    {
                        if (ex is AggregateException aex) ex = aex.Flatten();
                        var exMessage = $"{ex}";
                        PluginLog.LogError($"Error occurred during OnBuildUi event: {exMessage}");
                        throw;
                    }
                    finally
                    {
                        ImGui.PopID();
                    }
                }
            }
            this.ProcessFramework = 1; // Not Interlocked here
        }

        // FrameworkOnce
        private readonly object FrameworkOnceLock = new();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3264:Events should be invoked", Justification = "Invoked via GetInvocationList")]
        private event Framework.OnUpdateDelegate FrameworkOnceQueue;
        public event Framework.OnUpdateDelegate OnFrameworkOnce
        {
            add
            {
                lock (this.FrameworkOnceLock)
                {
                    this.FrameworkOnceQueue += value;
                }
            }
            remove
            {
                lock (this.FrameworkOnceLock)
                {
                    // The reason this is swallowed is because it may not be here anymore
                    try { this.FrameworkOnceQueue -= value; } catch { /* Swallow */ }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S3217:\"Explicit\" conversions of \"foreach\" loops should not be used", Justification = "Event Handler")]
        private void FrameworkOnceHandler(Framework framework)
        {
            Delegate[] dels = null;
            lock (this.FrameworkOnceLock)
            {
                if (this.FrameworkOnceQueue != null)
                {
                    dels = this.FrameworkOnceQueue.GetInvocationList();
                    foreach (Framework.OnUpdateDelegate del in dels)
                    {
                        this.FrameworkOnceQueue -= del;
                    }
                }
            }

            if (dels != null)
            {
                foreach (Framework.OnUpdateDelegate del in dels)
                {
                    try
                    {
                        this.FrameworkOnceQueue -= del;
                        del.Invoke(framework); // Nice proxy
                    }
                    catch (Exception ex)
                    {
                        if (ex is AggregateException aex) ex = aex.Flatten();
                        PluginLog.LogError($"Error occurred during Framework once event: {ex}");
                    }
                }
            }
        }

        // FrameworkEvent
        public event Framework.OnUpdateDelegate OnFrameworkEvent;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S3217:\"Explicit\" conversions of \"foreach\" loops should not be used", Justification = "Event Handler")]
        private void FrameworkEventHandler(Framework framework)
        {
            if (this.OnFrameworkEvent != null)
            {
                Delegate[] dels = this.OnFrameworkEvent.GetInvocationList();
                foreach (Framework.OnUpdateDelegate del in dels)
                {
                    try
                    {
                        del.Invoke(framework); // Nice proxy
                    }
                    catch (Exception ex)
                    {
                        if (ex is AggregateException aex) ex = aex.Flatten();
                        PluginLog.LogError($"Error occurred during Framework event: {ex}");
                    }
                }
            }
        }

        // FrameworkTick
        private int DoFrameworkTick; // Interlocked
        public event Framework.OnUpdateDelegate OnFrameworkTick;
        private void FrameworkTickHandler(Framework framework)
        {
            if (this.OnFrameworkTick != null && Interlocked.Exchange(ref this.DoFrameworkTick, 0) != 0)
            {
                Delegate[] dels = this.OnFrameworkTick.GetInvocationList();
                foreach (Framework.OnUpdateDelegate del in dels)
                {
                    try
                    {
                        del.Invoke(framework);
                    }
                    catch (Exception ex)
                    {
                        if (ex is AggregateException aex) ex = aex.Flatten();
                        PluginLog.LogError($"Error occurred during Framework tick event: {ex}");
                    }
                }
            }
        }

        private void Framework_OnUpdateEvent(Framework framework)
        {
            if (Interlocked.Exchange(ref this.ProcessFramework, 1) == 0) return; // TODO: No effect for now
            try { this.FrameworkOnceHandler(framework); } catch (Exception ex) { this.FrameworkErrorHandler(nameof(this.OnFrameworkOnce), ex); }
            try { this.FrameworkEventHandler(framework); } catch (Exception ex) { this.FrameworkErrorHandler(nameof(this.OnFrameworkEvent), ex); }
            try { this.FrameworkTickHandler(framework); } catch (Exception ex) { this.FrameworkErrorHandler(nameof(this.OnFrameworkTick), ex); }
        }

        private void FrameworkErrorHandler(string name, Exception ex)
        {
            if (ex is AggregateException aex) ex = aex;
            PluginLog.LogError($"Error occurred while invoking {name}: {ex}");
        }

        private void Ticker_OnTick(SonarClient source)
        {
            this.DoFrameworkTick = 1; // No need for Interlocked here
        }
        #endregion

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
                this.ResetConfiguration();
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

        private void Client_OnClientConnected(SonarClient sonar) => PluginLog.LogInformation($"Connected to Sonar");
        private void Client_OnReady(SonarClient sonar) => PluginLog.LogInformation($"Sonar is ready");

        private void Client_OnClientDisconnected(SonarClient source)
        {
            PluginLog.LogError($"Disconnected from Sonar");
        }

        private void ClientLogHandler(SonarClient source, LogLine log) => this.LogHandler(log);
        private void GUILogHandler(SonarGUIService source, LogLine log) => this.LogHandler(new(log.Level, $"[GUI] {log.Message}"));

        private void LogHandler(LogLine log)
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

        ~SonarPlugin()
        {
            try { this.Dispose(false); } catch (Exception ex) { PluginLog.Error(ex, string.Empty); }
        }

        private void Dispose(bool disposing)
        {
            if (Interlocked.CompareExchange(ref this._disposed, 1, 0) != 0) return;
            this.SaveConfiguration();
            if (this.Client is not null)
            {
                this.Client.Tick -= Ticker_OnTick;
            }
            this.SonarGUI.LogMessage -= GUILogHandler;

            // Hunt and Fate Trackers
            if (this.Client is not null)
            {
                this.Client.ServerMessage -= Events_OnSonarMessage;
                this.Client.LogMessage -= ClientLogHandler;
                this.Client.ClientConnected -= Client_OnClientConnected;
                this.Client.Ready -= Client_OnReady;
                this.Client.ClientDisconnected -= Client_OnClientDisconnected;
            }

            if (this.PluginInterface is not null)
            {
                // Logged in / out handlers
                this.PluginInterface.UiBuilder.Draw -= this.SonarGUI.Draw;
                this.PluginInterface.UiBuilder.Draw -= UiBuilder_OnBuildUi;
                this.PluginInterface.UiBuilder.Draw -= this.Windows.Draw;
                this.Framework.Update -= Framework_OnUpdateEvent;
            }

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
