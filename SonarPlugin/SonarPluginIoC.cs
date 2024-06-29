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
using System.ComponentModel;
using Container = DryIoc.Container;
using Sonar.Trackers;
using Sonar.Models;
using Dalamud.Plugin.Services;

namespace SonarPlugin
{
    public sealed class SonarPluginIoC : IDisposable
    {
        private readonly Container _container = new();
        private FileDialogManager? _fileDialogs;

        public DalamudPluginInterface PluginInterface { get; set; }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Container Container => this._container; // Only stub should access this
        private SonarPluginStub Stub { get; set; }
        
        // NOTE: All setters for Plugin Services need to stay, Injector won't work otherwise.
        [PluginService] private IFramework Framework { get; set; } = default!;
        [PluginService] private ICondition Condition { get; set; } = default!;
        [PluginService] private IClientState ClientState { get; set; } = default!;
        [PluginService] private IObjectTable ObjectTable { get; set; } = default!;
        [PluginService] private IFateTable FateTable { get; set; } = default!;
        [PluginService] private IGameGui GameGui { get; set; } = default!;
        [PluginService] private IChatGui ChatGui { get; set; } = default!;
        [PluginService] private ICommandManager CommandManager { get; set; } = default!;
        [PluginService] private ISigScanner SigScanner { get; set; } = default!;
        [PluginService] private IDataManager Data { get; set; } = default!;
        [PluginService] private IPluginLog Logger { get; set; } = default!;
        [PluginService] private ITextureProvider Textures { get; set; } = default!;

        public SonarPluginIoC(SonarPluginStub stub, DalamudPluginInterface pluginInterface)
        {
            this.Stub = stub;
            this.PluginInterface = pluginInterface;
            pluginInterface.Inject(this);
            this.ConfigureServices();
        }

        private SonarClient GetSonarClient()
        {
            this.Logger.Information("Initializing Sonar");
            var startInfo = new SonarStartInfo()
            {
                WorkingDirectory = Path.Join(this.PluginInterface.GetPluginConfigDirectory(), "Sonar"),
                PluginSecret = ClientSecret.ReadEmbeddedSecret(typeof(SonarPluginIoC).Assembly, "SonarPlugin.Resources.Secret.data"),
            };

            var versionInfo = VersionUtils.GetSonarVersionModel(this.Data);
            var client = new SonarClient(startInfo) { VersionInfo = versionInfo };
            Database.DefaultLanguage = this.Data.Language switch
            {
                ClientLanguage.Japanese => SonarLanguage.Japanese,
                ClientLanguage.English => SonarLanguage.English,
                ClientLanguage.German => SonarLanguage.German,
                ClientLanguage.French => SonarLanguage.French,
                (ClientLanguage/*.ChineseSimplified*/)4 => SonarLanguage.ChineseSimplified, // https://github.com/ottercorp/Dalamud/blob/cn/Dalamud/ClientLanguage.cs#L31
                _ => SonarLanguage.English
            };
            return client;
        }

        private void ConfigureServices()
        {
            var assembly = Assembly.GetExecutingAssembly();

            // SonarPlugin services
            this._container.RegisterInstance(this, setup: Setup.With(preventDisposal: true));
            this._container.RegisterInstance(this.Stub, setup: Setup.With(preventDisposal: true));
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
            this._container.Register(Made.Of(r => ServiceInfo.Of<SonarClient>(), c => c.Trackers), Reuse.Singleton, Setup.With(preventDisposal: true));
            this._container.Register(Made.Of(r => ServiceInfo.Of<RelayTrackers>(), c => c.Hunts), Reuse.Singleton, Setup.With(preventDisposal: true));
            this._container.Register(Made.Of(r => ServiceInfo.Of<RelayTrackers>(), c => c.Fates), Reuse.Singleton, Setup.With(preventDisposal: true));
            this._container.Register(Made.Of(r => ServiceInfo.Of<SonarPlugin>(), p => p.Windows), Reuse.Singleton, Setup.With(preventDisposal: true));

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
            this._container.RegisterInstance(this.Logger, setup: Setup.With(preventDisposal: true));
            this._container.RegisterInstance(this.Textures, setup: Setup.With(preventDisposal: true));

            // Additional Dalamud Services
            this._container.Register(Made.Of(r => ServiceInfo.Of<DalamudPluginInterface>(), pi => pi.UiBuilder), Reuse.Singleton, Setup.With(preventDisposal: true));
            this._container.Register(Made.Of(r => ServiceInfo.Of<IDataManager>(), d => d.GameData), Reuse.Singleton, Setup.With(preventDisposal: true));

#if DEBUG
            this.Logger.Information("Registered services:");
            foreach (var service in this._container.GetServiceRegistrations())
            {
                this.Logger.Information($" - {service}");
            }

            this.Logger.Information("Validating DryIoC");
            var exceptions = this._container.Validate();

            foreach (var (service, exception) in exceptions)
            {
                this.Logger.Error(exception, $"Exception in {service.ServiceType.Name}");
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
                this.Logger.Debug($"Starting {service.GetType().Name}");
                tasks.Add(service.StartAsync(CancellationToken.None));
            }
            Task.WaitAll(tasks.ToArray());
        }

        public void StopServices()
        {
            var tasks = new List<Task>();
            foreach (var service in this._container.ResolveMany<IHostedService>())
            {
                this.Logger.Debug($"Stopping {service.GetType().Name}");
                tasks.Add(service.StopAsync(CancellationToken.None));
            }
            Task.WaitAll(tasks.ToArray());
        }

        public void Dispose()
        {
            this._container.Dispose(); // All singleton disposables are disposed here
            if (this._fileDialogs is not null) this.PluginInterface.UiBuilder.Draw -= this._fileDialogs.Draw;
        }
    }
}
