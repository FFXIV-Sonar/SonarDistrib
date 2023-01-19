using Dalamud.Game.ClientState.Fates;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Sonar;
using System;
using System.Linq;
using System.Reflection;
using DryIoc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Dalamud.Plugin;
using Dalamud.Logging;
using Dalamud.Data;
using Dalamud.Game;
using SonarPlugin.Utility;
using Sonar.Data;
using Sonar.Enums;
using SonarPlugin.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState;
using System.IO;
using Dalamud.Interface.ImGuiFileDialog;
using System.Runtime.CompilerServices;

namespace SonarPlugin
{
    public sealed class SonarPluginIoC : IDisposable
    {
        private readonly Container _container = new();
        private FileDialogManager? _fileDialogs;

        public DalamudPluginInterface PluginInterface { get; set; }
        [PluginService] private Framework Framework { get; set; } = default!;
        [PluginService] private Condition Condition { get; set; } = default!;
        [PluginService] private ClientState ClientState { get; set; } = default!;
        [PluginService] private ObjectTable ObjectTable { get; set; } = default!;
        [PluginService] private FateTable FateTable { get; set; } = default!;
        [PluginService] private GameGui GameGui { get; set; } = default!;
        [PluginService] private ChatGui ChatGui { get; set; } = default!;
        [PluginService] private CommandManager CommandManager { get; set; } = default!;
        [PluginService] private SigScanner SigScanner { get; set; } = default!;
        [PluginService] private DataManager Data { get; set; } = default!;

        public SonarPluginIoC(DalamudPluginInterface pluginInterface)
        {
            this.PluginInterface = pluginInterface;
            pluginInterface.Inject(this);
            this.ConfigureServices();
            this.VersionCheck();
            this.StartServices();
        }

        private void VersionCheck()
        {
            // Game Version Check
            if (this._container.Resolve<SonarAddressResolver>().Instance == IntPtr.Zero)
            {
                throw new NotSupportedException("FFXIV Version incompatible (Unable to read instance)");
            }
        }

        private SonarClient GetSonarClient()
        {
            PluginLog.LogInformation("Initializing Sonar");
            var startInfo = new SonarStartInfo() { WorkingDirectory = Path.Join(this.PluginInterface.GetPluginConfigDirectory(), "Sonar") };
            var versionInfo = VersionUtils.GetSonarVersionModel(this.Data);
            var client = new SonarClient(startInfo) { VersionInfo = versionInfo };
            Database.DefaultLanguage = this.Data.Language switch
            {
                Dalamud.ClientLanguage.Japanese => SonarLanguage.Japanese,
                Dalamud.ClientLanguage.English => SonarLanguage.English,
                Dalamud.ClientLanguage.German => SonarLanguage.German,
                Dalamud.ClientLanguage.French => SonarLanguage.French,
                (Dalamud.ClientLanguage/*.ChineseSimplified*/)4 => SonarLanguage.ChineseSimplified, // https://github.com/ottercorp/Dalamud/blob/cn/Dalamud/ClientLanguage.cs#L31
                _ => SonarLanguage.English
            };
            return client;
        }

        private void ConfigureServices()
        {
            var assembly = Assembly.GetExecutingAssembly();

            // SonarPlugin services
            this._container.RegisterInstance(this, setup: Setup.With(preventDisposal: true));
            this._container.RegisterInstance(this._container, setup: Setup.With(preventDisposal: true));

            // Services
            this.RegisterTypesWithAttribute(assembly, typeof(SingletonServiceAttribute), Reuse.Singleton);
            this.RegisterTypesWithAttribute(assembly, typeof(ScopedServiceAttribute), Reuse.Scoped);
            this.RegisterTypesWithAttribute(assembly, typeof(TransientServiceAttribute), Reuse.Transient);

            // Background Services
            foreach (var type in assembly.GetTypes().Where(t => t.GetInterface(nameof(IHostedService)) is not null))
            {
                this._container.RegisterMany(new[] { type, typeof(IHostedService) }, type, Reuse.Singleton);
            }

            // Sonar Services
            this._container.RegisterDelegate(this.GetSonarClient, Reuse.Singleton);
            this._container.Register(Made.Of(r => ServiceInfo.Of<SonarClient>(), c => c.HuntTracker), Reuse.Singleton, Setup.With(preventDisposal: true));
            this._container.Register(Made.Of(r => ServiceInfo.Of<SonarClient>(), c => c.FateTracker), Reuse.Singleton, Setup.With(preventDisposal: true));

            // Additional Services
            this._container.RegisterDelegate(this.CreateFileDialogManager, Reuse.Singleton);

            // Dalamud Services
            this._container.RegisterInstance(this.PluginInterface, setup: Setup.With(preventDisposal: true)); // Dispose is [Obsolete]
            this._container.RegisterInstance(this.Framework, setup: Setup.With(preventDisposal: true));
            this._container.RegisterInstance(this.Condition, setup: Setup.With(preventDisposal: true));
            this._container.RegisterInstance(this.ClientState, setup: Setup.With(preventDisposal: true));
            this._container.RegisterInstance(this.GameGui, setup: Setup.With(preventDisposal: true));
            this._container.RegisterInstance(this.ChatGui, setup: Setup.With(preventDisposal: true));
            this._container.RegisterInstance(this.CommandManager, setup: Setup.With(preventDisposal: true));
            this._container.RegisterInstance(this.FateTable, setup: Setup.With(preventDisposal: true));
            this._container.RegisterInstance(this.ObjectTable, setup: Setup.With(preventDisposal: true));
            this._container.RegisterInstance(this.SigScanner, setup: Setup.With(preventDisposal: true));
            this._container.RegisterInstance(this.Data, setup: Setup.With(preventDisposal: true));

            // Additional Dalamud Services
            this._container.Register(Made.Of(r => ServiceInfo.Of<DalamudPluginInterface>(), pi => pi.UiBuilder), Reuse.Singleton, Setup.With(preventDisposal: true));
            this._container.Register(Made.Of(r => ServiceInfo.Of<DataManager>(), d => d.GameData), Reuse.Singleton, Setup.With(preventDisposal: true));

#if DEBUG
            PluginLog.LogInformation("Validating DryIoC");
            var exceptions = this._container.Validate();
            foreach (var (service, exception) in exceptions)
            {
                PluginLog.LogError(exception, $"Exception in {service.ServiceType.Name}");
            }
#endif
        }

        private FileDialogManager CreateFileDialogManager()
        {
            if (this._fileDialogs is null && Interlocked.CompareExchange(ref this._fileDialogs, new(), null) == null)
            {
                this.PluginInterface.UiBuilder.Draw += this._fileDialogs.Draw;
            }
            return this._fileDialogs;
        }

        private void RegisterTypesWithAttribute(Assembly assembly, Type attribute, IReuse reuse)
        {
            // Singleton Services
            foreach (var type in assembly.GetTypes().Where(t => t.GetCustomAttribute(attribute) is not null))
            {
                this._container.Register(type, reuse);
            }
        }

        public void StartServices()
        {
            var tasks = new List<Task>();
            foreach (var service in this._container.ResolveMany<IHostedService>())
            {
                PluginLog.LogDebug($"Starting {service.GetType().Name}");
                tasks.Add(service.StartAsync(CancellationToken.None));
            }
            Task.WaitAll(tasks.ToArray());
        }

        public void StopServices()
        {
            var tasks = new List<Task>();
            foreach (var service in this._container.ResolveMany<IHostedService>())
            {
                PluginLog.LogDebug($"Stopping {service.GetType().Name}");
                tasks.Add(service.StopAsync(CancellationToken.None));
            }
            Task.WaitAll(tasks.ToArray());
        }

        public void Dispose()
        {
            this.StopServices();
            this._container.Dispose(); // All singleton disposables are disposed here
            if (this._fileDialogs is not null) this.PluginInterface.UiBuilder.Draw -= this._fileDialogs.Draw;
        }
    }
}
