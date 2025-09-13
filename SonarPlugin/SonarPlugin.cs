using Sonar.Extensions;
using CheapLoc;
using SonarPlugin.Config;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin;
using Sonar;
using System;
using System.Threading;
using SonarPlugin.Utility;
using Sonar.Logging;
using Dalamud.Interface.Windowing;
using System.Diagnostics.CodeAnalysis;
using Dalamud.Plugin.Services;

namespace SonarPlugin
{
    [SingletonService]
    public sealed class SonarPlugin : IDisposable
    {
        private IDalamudPluginInterface PluginInterface { get; }
        private SonarClient Client { get; }
        private IFramework Framework { get; }
        private IChatGui Chat { get; }
        private ICondition Condition { get; }
        private AudioPlaybackEngine Audio { get; }
        private IPluginLog Logger { get; }

        public bool IsDuty { get; private set; }

        public SonarPlugin(IDalamudPluginInterface pluginInterface, SonarClient client, IFramework framework, IChatGui chat, ICondition condition, AudioPlaybackEngine audio, IPluginLog logger)
        {
            this.PluginInterface = pluginInterface;
            this.Client = client;
            this.Framework = framework;
            this.Chat = chat;
            this.Condition = condition;
            this.Audio = audio;
            this.Logger = logger;

            this.Initialize();
        }

        public WindowSystem Windows { get; } = new(nameof(SonarPlugin));
        public SonarConfiguration Configuration { get; private set; } = default!;

        public void Initialize()
        {
            this.LoadConfiguration();

            this.Logger.Info("Setting up localization");
            EnumLocUtils.Setup(this.Configuration.Localization.DebugFallbacks);
            CheapLoc.Loc.SetupWithFallbacks();

            this.Logger.Info("SonarPlugin Resources:");
            foreach (var resourceName in typeof(SonarPlugin).Assembly.GetManifestResourceNames())
            {
                this.Logger.Info($" - {resourceName}");
            }

            this.Logger.Info("Sonar Resources:");
            foreach (var resourceName in typeof(SonarClient).Assembly.GetManifestResourceNames())
            {
                this.Logger.Info($" - {resourceName}");
            }

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
            this.Chat.Print(new()
            {
                Type = this.Configuration.HuntOutputChannel,
                Name = "Sonar",
                Message = message
            });
            this.Logger.Information("Sonar Message Received: {message}");
        }

        #region OnBuildUi and Framework event and tick manager
        public bool SafeToReadTables { get; private set; }

        // FrameworkEvent
        public event Action<IFramework>? OnFrameworkEvent;

        private void Framework_OnUpdateEvent(IFramework framework)
        {
            this.SafeToReadTables = !this.Condition[ConditionFlag.BetweenAreas51]; // TODO: Move the condition checks to their respective places
            this.IsDuty = this.Condition[ConditionFlag.BoundByDuty56];
            this.DispatchFrameworkEvent(nameof(this.OnFrameworkEvent), this.OnFrameworkEvent, framework);
        }

        private void DispatchFrameworkEvent(string name, Action<IFramework>? ev, IFramework framework)
        {
            if (ev is null) return;
            ev.SafeInvoke(framework, out var exceptions);
            foreach (var ex in exceptions) this.FrameworkErrorHandler($"{name} Exception", ex);
        }

        private void FrameworkErrorHandler(string name, Exception ex)
        {
            if (ex is AggregateException aex) ex = aex;
            this.Logger.Error(ex, $"{name} Exception");
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

                this.Configuration.Sanitize();
                this.Client.Configuration.ReadFrom(this.Configuration.SonarConfig);
                this.Configuration.SonarConfig = this.Client.Configuration;

                if (this.Configuration.PerformVersionUpdate(this.Logger))
                {
                    this.SaveConfiguration(true);
                }
            }
            catch (Exception ex)
            {
                this.Logger.Error($"Failed to load configuration: {ex}");
                if (!isReset) this.ResetConfiguration();
            }
        }

        public void SaveConfiguration(bool updateServer = false)
        {
            try
            {
                //if (updateServer) this.Client.Configuration = this.Client.Configuration; // TODO: Better interface than assigning (using the setter)...
                this.Configuration.SonarConfig = this.Client.Configuration;
                this.PluginInterface.SavePluginConfig(this.Configuration);
            }
            catch (Exception e)
            {
                this.Logger.Error($"Failed to save configuration: {e}");
            }
        }

        public void ResetConfiguration()
        {
            this.Configuration = new SonarConfiguration();
            this.Client.Configuration.ReadFrom(this.Configuration.SonarConfig);
            this.Configuration.SonarConfig = this.Client.Configuration;
            this.Client.Configuration.Contribute.Reset();
            this.SaveConfiguration(true);
            this.LoadConfiguration(true);
        }

        private void ClientLogHandler(SonarClient source, SonarLogMessage log) => this.LogHandler(log);

        [SuppressMessage("Minor Code Smell", "S3458", Justification = "Clarity")]
        private void LogHandler(SonarLogMessage log)
        {
            var (level, message) = (log.Level, log.Message);
            switch (level)
            {
                case SonarLogLevel.Verbose:
                    this.Logger.Verbose(message);
                    break;
                case SonarLogLevel.Debug:
                    this.Logger.Debug(message);
                    break;
                case SonarLogLevel.Information:
                    this.Logger.Information(message);
                    break;
                case SonarLogLevel.Warning:
                    this.Logger.Warning(message);
                    break;
                case SonarLogLevel.Error:
                    this.Logger.Error(message);
                    break;
                case SonarLogLevel.Fatal:
                default:
                    this.Logger.Fatal(message);
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
                this.Client.ServerMessage -= this.Events_OnSonarMessage;
                this.Client.LogMessage -= this.ClientLogHandler;
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
