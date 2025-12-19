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
using Dalamud.Game;
using SonarPlugin.Utility;
using Sonar.Data;
using Sonar.Enums;
using System.IO;
using Dalamud.Interface.ImGuiFileDialog;
using System.ComponentModel;
using Container = DryIoc.Container;
using Sonar.Trackers;
using Sonar.Models;
using Dalamud.Plugin.Services;
using SonarPlugin.GUI;
using SonarUtils.Text.Placeholders;
using SonarUtils.Secrets;
using SonarPlugin.Sounds;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using Dalamud.Plugin.VersionInfo;

namespace SonarPlugin
{
    public sealed class SonarPluginIoC : IDisposable
    {
        private readonly Container _container = new();
        private FileDialogManager? _fileDialogs;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public Container Container => this._container; // Only stub should access this

        private SonarPluginStub Stub { get; }
        public IDalamudPluginInterface PluginInterface { get; }
        public IDalamudVersionInfo DalamudVersion { get; }
        private IDataManager Data { get; }
        private IPluginLog Logger { get; }

        public SonarPluginIoC(SonarPluginStub stub, IDalamudPluginInterface pluginInterface)
        {
            this.Stub = stub;
            this.PluginInterface = pluginInterface;
            this.Data = pluginInterface.GetRequiredService<IDataManager>();
            this.DalamudVersion = pluginInterface.GetDalamudVersion();
            this.Logger = pluginInterface.GetRequiredService<IPluginLog>();
            this.ConfigureServices();
        }

        private SonarClient GetSonarClient()
        {
            this.Logger.Information("Initializing Sonar");
            var startInfo = new SonarStartInfo()
            {
                WorkingDirectory = Path.Join(this.PluginInterface.GetPluginConfigDirectory(), "Sonar"),
                PluginSecretMeta = SecretUtils.GetSecretMetaBytes(typeof(SonarPlugin).Assembly)
            };

            SonarLanguage DetermineLanguage(int num)
            {
                var name = Enum.GetName((ClientLanguage)num);
                if (name is "Korean") return SonarLanguage.Korean;
                if (name is "ChineseSimplified") return SonarLanguage.ChineseSimplified;
                if (name is "ChineseTraditional") return SonarLanguage.ChineseSimplified; // TODO: Change to .ChineseTraditional once done

                this.Logger.Warning($"Unable to determine ClientLanguage {num}");
                return
                    num is 4 ? SonarLanguage.ChineseSimplified :
                    num is 5 ? SonarLanguage.ChineseSimplified : // TODO: Change to .ChineseTraditional once done
                    SonarLanguage.English;
            }

            var versionInfo = VersionUtils.GetSonarVersionModel(this.Data, this.DalamudVersion);
            var client = new SonarClient(startInfo) { VersionInfo = versionInfo };
            Database.DefaultLanguage = this.Data.Language switch
            {
                ClientLanguage.Japanese => SonarLanguage.Japanese,
                ClientLanguage.English => SonarLanguage.English,
                ClientLanguage.German => SonarLanguage.German,
                ClientLanguage.French => SonarLanguage.French,
                _ => DetermineLanguage((int)this.Data.Language), // https://github.com/ottercorp/Dalamud/blob/cn/Dalamud/ClientLanguage.cs#L31 https://github.com/yanmucorp/Dalamud/blob/master/Dalamud/Game/ClientLanguage.cs#L36
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
                this._container.RegisterMany([type, typeof(IHostedService)], type, Reuse.Singleton);
            }

            // Sonar Services
            this._container.RegisterDelegate(this.GetSonarClient, Reuse.Singleton);
            this._container.Register(Made.Of(r => ServiceInfo.Of<SonarClient>(), c => c.Trackers), Reuse.Singleton, Setup.With(preventDisposal: true));
            this._container.Register(Made.Of(r => ServiceInfo.Of<SonarClient>(), c => c.Configuration), Reuse.Singleton, Setup.With(preventDisposal: true));
            this._container.Register(Made.Of(r => ServiceInfo.Of<RelayTrackers>(), c => c.Hunts), Reuse.Singleton, Setup.With(preventDisposal: true));
            this._container.Register(Made.Of(r => ServiceInfo.Of<RelayTrackers>(), c => c.Fates), Reuse.Singleton, Setup.With(preventDisposal: true));
            this._container.Register(Made.Of(r => ServiceInfo.Of<SonarPlugin>(), p => p.Windows), Reuse.Singleton, Setup.With(preventDisposal: true));

            // Additional Services
            this._container.RegisterDelegate(this.GetOrCreateFileDialogManager, Reuse.Singleton);
            this._container.RegisterInstance(PlaceholderFormatter.Default);

            // Dalamud Services
            this._container.RegisterInstance(this.PluginInterface, setup: Setup.With(preventDisposal: true)); // Dispose is [Obsolete]
            this._container.RegisterInstance(this.Logger, setup: Setup.With(preventDisposal: true));
            this._container.RegisterDelegate(this.PluginInterface.GetRequiredService<IFramework>, Reuse.Singleton, setup: Setup.With(preventDisposal: true));
            this._container.RegisterDelegate(this.PluginInterface.GetRequiredService<ICondition>, Reuse.Singleton, setup: Setup.With(preventDisposal: true));
            this._container.RegisterDelegate(this.PluginInterface.GetRequiredService<IClientState>, Reuse.Singleton, setup: Setup.With(preventDisposal: true));
            this._container.RegisterDelegate(this.PluginInterface.GetRequiredService<IPlayerState>, Reuse.Singleton, setup: Setup.With(preventDisposal: true));
            this._container.RegisterDelegate(this.PluginInterface.GetRequiredService<IGameGui>, Reuse.Singleton, setup: Setup.With(preventDisposal: true));
            this._container.RegisterDelegate(this.PluginInterface.GetRequiredService<IChatGui>, Reuse.Singleton, setup: Setup.With(preventDisposal: true));
            this._container.RegisterDelegate(this.PluginInterface.GetRequiredService<ICommandManager>, Reuse.Singleton, setup: Setup.With(preventDisposal: true));
            this._container.RegisterDelegate(this.PluginInterface.GetRequiredService<IFateTable>, Reuse.Singleton, setup: Setup.With(preventDisposal: true));
            this._container.RegisterDelegate(this.PluginInterface.GetRequiredService<IObjectTable>, Reuse.Singleton, setup: Setup.With(preventDisposal: true));
            this._container.RegisterDelegate(this.PluginInterface.GetRequiredService<ISigScanner>, Reuse.Singleton, setup: Setup.With(preventDisposal: true));
            this._container.RegisterDelegate(this.PluginInterface.GetRequiredService<IDataManager>, Reuse.Singleton, setup: Setup.With(preventDisposal: true));
            this._container.RegisterDelegate(this.PluginInterface.GetRequiredService<ITextureProvider>, Reuse.Singleton, setup: Setup.With(preventDisposal: true));
            this._container.RegisterDelegate(this.PluginInterface.GetDalamudVersion, Reuse.Singleton, setup: Setup.With(preventDisposal: true));

            // Additional Dalamud Services
            this._container.Register(Made.Of(r => ServiceInfo.Of<IDalamudPluginInterface>(), pi => pi.UiBuilder), Reuse.Singleton, Setup.With(preventDisposal: true));
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

        private FileDialogManager GetOrCreateFileDialogManager()
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
            var services = this._container.ResolveMany<IHostedService>();
            var tasks = new ConcurrentQueue<Task>();
            Parallel.ForEach(services, service =>
            {
                this.Logger.Debug($"Starting {service.GetType().Name}");
                try
                {
                    var task = service.StartAsync(CancellationToken.None);
                    tasks.Enqueue(task);
                }
                catch (Exception ex)
                {
                    this.Logger.Error(ex, $"Exception starting {service.GetType().Name}");
                }
            });
            while (tasks.TryDequeue(out var task)) task.Wait();
        }

        public void StopServices()
        {
            var services = this._container.ResolveMany<IHostedService>();
            var tasks = new ConcurrentQueue<Task>();
            Parallel.ForEach(services, service =>
            {
                this.Logger.Debug($"Stopping {service.GetType().Name}");
                try
                {
                    var task = service.StopAsync(CancellationToken.None);
                    tasks.Enqueue(task);
                }
                catch (Exception ex)
                {
                    this.Logger.Error(ex, $"Exception stopping {service.GetType().Name}");
                }
            });
            while (tasks.TryDequeue(out var task)) task.Wait();
        }

        public void Dispose()
        {
            if (this._fileDialogs is not null) this.PluginInterface.UiBuilder.Draw -= this._fileDialogs.Draw;
            this._container.Dispose(); // All singleton disposables are disposed here
        }
    }
}
