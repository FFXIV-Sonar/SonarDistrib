using Dalamud.Game;
using Dalamud.Interface;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Plugin.VersionInfo;
using DryIoc;
using DryIoc.MefAttributedModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Sonar;
using Sonar.Data;
using Sonar.Enums;
using Sonar.Trackers;
using SonarPlugin.Config;
using SonarPlugin.Events;
using SonarPlugin.Logging;
using SonarPlugin.Logging.Internal;
using SonarPlugin.Utility;
using SonarUtils;
using SonarUtils.Secrets;
using SonarUtils.Text.Placeholders;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Container = DryIoc.Container;
using IContainer = DryIoc.IContainer;

namespace SonarPlugin
{
    public sealed partial class SonarPluginIoC : IAsyncDalamudPlugin
    {
        private readonly CancellationTokenSource _cts = new();
        private readonly Container _container;

        public IDalamudPluginInterface PluginInterface { get; }
        public IDalamudVersionInfo DalamudVersion { get; }
        private IDataManager Data { get; }
        private IWindowSystem Windows { get; }
        private FileDialogManager FileDialogs { get; }
        private IUiBuilder Ui { get; }
        private IChatGui Chat { get; }
        private AudioPlaybackEngine Audio { get; }
        private ILogger Logger { get; }

        private SonarClient Client { get; }

        public SonarPluginIoC(IDalamudPluginInterface pluginInterface)
        {
            this.PluginInterface = pluginInterface;
            this._container = this.CreateContainer();

            this.Data = this._container.Resolve<IDataManager>();
            this.DalamudVersion = this._container.Resolve<IDalamudVersionInfo>();
            this.Windows = this._container.Resolve<IWindowSystem>();
            this.FileDialogs = this._container.Resolve<FileDialogManager>(); // NOTE: No interface.
            this.Ui = this._container.Resolve<IUiBuilder>();
            this.Chat = this._container.Resolve<IChatGui>();
            this.Audio = this._container.Resolve<AudioPlaybackEngine>();
            this.Logger = this._container.Resolve<ILogger<SonarPluginIoC>>();

            this.Ui.Draw += this.Windows.Draw;
            this.Ui.Draw += this.FileDialogs.Draw;

            this.LogResourceNames();

            this.Client = this._container.Resolve<SonarClient>();
            this.InitializeClient();

            this.LoadConfiguration();
            Debug.Assert(this.Configuration is not null);

            // Set volume of alerts to current config, this also will initialize the Instance of the audio service
            this.Audio.Volume = this.Configuration.SoundVolume;

            this.Logger.LogInformation("Setting up localization");
            EnumLocUtils.Setup(this.Configuration.Localization.DebugFallbacks);
            CheapLoc.Loc.SetupWithFallbacks();

            try
            {
                var flavor = FlavorUtils.DetermineFlavor(pluginInterface, this.Logger);
                if (!string.IsNullOrWhiteSpace(flavor))
                {
                    this.Logger.LogInformation("Detected Flavor: {flavor}", flavor);
                    this.Name = $"{this.Name}-{flavor}";
                    this.PluginName = $"{this.PluginName}-{flavor}";
                    this.Flavor = flavor;
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Exception occured while getting flavor");
            }

            this._configurationTask = this.ConfigurationTask(this._cts.Token);
        }

        /// <summary>Sonar name</summary>
        public string Name { get; } = "Sonar";

        /// <summary>PluginName</summary>
        public string PluginName { get; } = "SonarPlugin";

        /// <summary>Sonar flavor</summary>
        public string? Flavor { get; }

        /// <summary>Plugin Configuration.</summary>
        public SonarConfiguration Configuration { get; private set; }

        /// <inheritdoc/>
        public async Task LoadAsync(CancellationToken cancellationToken)
        {
            await this._container.StartAllServicesAsync(this.Logger, cancellationToken).ConfigureAwait(false);
            this.Client.Start();
        }

        [Conditional("DEBUG")]
        private void LogResourceNames()
        {
            this.Logger.LogInformation("SonarPlugin Resources:");
            foreach (var resourceName in typeof(SonarPluginIoC).Assembly.GetManifestResourceNames())
            {
                this.Logger.LogInformation(" - {resourceName}", resourceName);
            }

            this.Logger.LogInformation("Sonar Resources:");
            foreach (var resourceName in typeof(SonarClient).Assembly.GetManifestResourceNames())
            {
                this.Logger.LogInformation(" - {resourceName}", resourceName);
            }
        }

        private static WindowSystem CreateWindowSystem() => new("SonarPlugin");

        private static FileDialogManager CreateFileDialogs() => new();

        private async Task<IReadOnlyDictionary<string, ImmutableArray<byte>>?> ChallengeHandlerAsync(ImmutableArray<byte> key, CancellationToken cancellationToken)
        {
            var directory = this.PluginInterface.AssemblyLocation.Directory;
            if (directory is null) return null;

            var results = new Dictionary<string, ImmutableArray<byte>>();
            await foreach (var (file, result) in SonarIntegrity.GenerateHashesAsync(directory, key.AsMemory(), cancellationToken).ConfigureAwait(false))
            {
                results.Add(file, result);
            }
            return results;
        }

        private Container CreateContainer()
        {
            var container = new Container();
            container.RegisterInstanceMany(container, setup: Setup.With(preventDisposal: true));

            // Services
            container.RegisterExports(typeof(SonarPluginIoC).Assembly);

            // Logging Services
            container.RegisterMany(Made.Of(() => new LoggerFactory(Arg.Of<IEnumerable<ILoggerProvider>>())), Reuse.Singleton);
            container.Register(typeof(ILogger<>), typeof(PluginLoggerAdapter<>), Reuse.Singleton);
            container.AddPluginLogger();

            // SonarPlugin services
            container.RegisterInstance(this, setup: Setup.With(preventDisposal: true));
            container.RegisterDelegate<IWindowSystem>(CreateWindowSystem, Reuse.Singleton);
            container.RegisterDelegate(CreateFileDialogs, Reuse.Singleton);

            // Sonar Services
            container.RegisterMany(Made.Of(request => ServiceInfo.Of<SonarPluginIoC>(), plugin => plugin.CreateClient()), Reuse.Singleton);
            container.RegisterMany(Made.Of(request => ServiceInfo.Of<SonarClient>(), client => client.Trackers), Reuse.Singleton, Setup.With(preventDisposal: true));
            container.RegisterMany(Made.Of(request => ServiceInfo.Of<SonarClient>(), client => client.Configuration), Reuse.Singleton, Setup.With(preventDisposal: true));
            container.RegisterMany(Made.Of(request => ServiceInfo.Of<SonarClient>(), client => client.Meta), Reuse.Singleton, Setup.With(preventDisposal: true));
            container.RegisterMany(Made.Of(request => ServiceInfo.Of<RelayTrackers>(), trackers => trackers.Hunts), Reuse.Singleton, Setup.With(preventDisposal: true));
            container.RegisterMany(Made.Of(request => ServiceInfo.Of<RelayTrackers>(), trackers => trackers.Fates), Reuse.Singleton, Setup.With(preventDisposal: true));

            // Additional Services
            container.RegisterInstanceMany(PlaceholderFormatter.Default);

            // Dalamud Services
            container.RegisterInstance(this.PluginInterface, setup: Setup.With(preventDisposal: true)); // Dispose is [Obsolete]
            var dalamudServiceTypes = typeof(IDalamudService).Assembly.GetTypes()
                .Where(type => type.IsInterface)
                .Where(type => type.GetInterfaces().Contains(typeof(IDalamudService)));
            foreach (var type in dalamudServiceTypes)
            {
                container.Register(type, Reuse.Singleton, Made.Of(request => ServiceInfo.Of<IDalamudPluginInterface>(), pluginInterface => pluginInterface.GetService(Arg.Index<Type>(0)), req => req.ServiceType), Setup.With(preventDisposal: true));
            }

            // Additional Dalamud Services
            container.Register(Made.Of(request => ServiceInfo.Of<IDalamudPluginInterface>(), pluginInterface => pluginInterface.GetDalamudVersion()));
            container.RegisterMany(Made.Of(request => ServiceInfo.Of<IDalamudPluginInterface>(), pluginInterface => pluginInterface.UiBuilder), Reuse.Singleton, Setup.With(preventDisposal: true));
            container.RegisterMany(Made.Of(request => ServiceInfo.Of<IDataManager>(), data => data.GameData), Reuse.Singleton, Setup.With(preventDisposal: true));

#if DEBUG
            var logger = this.PluginInterface.GetRequiredService<IPluginLog>();
            try
            {
                container.PerformDebugValidation(out _, new PluginLogger("Container", logger));
            }
            catch (ContainerException ce)
            {
                logger.Error("Container Exception Details: {details}", ce.TryGetDetails(container));
                throw;
            }
#endif

            return container;
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            await this._cts.CancelAsync().ConfigureAwait(false);
            this._cts.Dispose();
            this._configurationSemaphore.Dispose();
            await this._configurationTask.ConfigureAwait(false);
            this.SaveConfigurationCore();

            this.DeinitializeClient();

            this.Ui.Draw -= this.Windows.Draw;
            this.Ui.Draw -= this.FileDialogs.Draw;

            await this._container.StopAllServicesAsync(this.Logger).ConfigureAwait(false);
            this._container.Dispose(); // All singleton disposables are disposed here
        }
    }
}
