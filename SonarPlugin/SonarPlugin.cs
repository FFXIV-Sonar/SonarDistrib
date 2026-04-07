using CheapLoc;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using DryIocAttributes;
using Sonar;
using Sonar.Extensions;
using Sonar.Logging;
using SonarPlugin.Config;
using SonarPlugin.Utility;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SonarPlugin
{
    [ExportMany]
    [SingletonReuse]
    public sealed class SonarPlugin : IDisposable
    {
        private ImmutableArray<Action<IFramework>> _frameworkUpdateHandlers = [];
        private ImmutableArray<Action<IFramework>> _frameworkTickHandlers = [];
        private bool _tick;
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

            this.Client.Tick += this.Client_Tick;
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
            this.Framework.Update += this.Framework_Update;

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
        
        /// <summary>Triggers every framework update after checks.</summary>
        public event Action<IFramework>? FrameworkUpdate
        {
            add
            {
                if (value is not null) ImmutableInterlocked.Update(ref this._frameworkUpdateHandlers, (handlers, handler) => handlers.Add(handler), value);
            }
            remove
            {
                if (value is not null) ImmutableInterlocked.Update(ref this._frameworkUpdateHandlers, (handlers, handler) => handlers.Remove(handler), value);
            }
        }

        /// <summary>Triggers every framework update after checks, but only once per Sonar tick (400ms).</summary>
        public event Action<IFramework>? FrameworkTick
        {
            add
            {
                if (value is not null) ImmutableInterlocked.Update(ref this._frameworkTickHandlers, (handlers, handler) => handlers.Add(handler), value);
            }
            remove
            {
                if (value is not null) ImmutableInterlocked.Update(ref this._frameworkTickHandlers, (handlers, handler) => handlers.Remove(handler), value);
            }
        }

        private void Framework_Update(IFramework framework)
        {
            this.SafeToReadTables = !this.Condition[ConditionFlag.BetweenAreas51]; // TODO: Move the condition checks to their respective places
            this.IsDuty = this.Condition[ConditionFlag.BoundByDuty56];

            this.Framework_UpdateCore(this._frameworkUpdateHandlers.AsSpan(), framework);
            if (!Interlocked.CompareExchange(ref this._tick, false, true)) return;
            this.Framework_UpdateCore(this._frameworkTickHandlers.AsSpan(), framework);
        }

        private void Framework_UpdateCore(ReadOnlySpan<Action<IFramework>> handlers, IFramework framework)
        {
            foreach (var handler in handlers)
            {
                try
                {
                    handler(framework);
                }
                catch (Exception ex)
                {
                    this.Logger.Error(ex, "Framework handler exception");
                }
            }
        }

        private void Client_Tick(SonarClient source) => Volatile.Write(ref this._tick, true);
        #endregion

        [SuppressMessage("Major Code Smell", "S112", Justification = "No suitable exception")]
        public void LoadConfiguration(bool isReset = false)
        {
            try
            {
                var configuration = (SonarConfiguration?)this.PluginInterface.GetPluginConfig();
                if (configuration is null)
                {
                    if (isReset) throw new Exception($"Failed resetting configuration");
                    this.ResetConfiguration();
                    return;
                }
                this.Configuration = configuration;

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
            this.Client.Tick -= this.Client_Tick;

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
                this.Framework.Update -= this.Framework_Update;
            }
        }
        #endregion
    }
}
