using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using DryIoc;
using DryIoc.MefAttributedModel;
using SonarDiagnostics.Dns;
using SonarDiagnostics.GUI;
using SonarUtils;
using System;
using System.Linq;

namespace SonarDiagnostics
{
    public sealed partial class Plugin : IDalamudPlugin
    {
        private readonly WindowSystem _windows;
        private readonly Container _container;

        private IDalamudPluginInterface PluginInterface { get; }
        private ICommandManager Commands { get; }
        private IChatGui Chat { get; }
        private DiagnosticLogger Logger { get; }
        public string? LogPath => this.Logger.LogPath;

        public Plugin(IDalamudPluginInterface pluginInterface, ICommandManager commands, IChatGui chat, IPluginLog logger)
        {
            this.PluginInterface = pluginInterface;
            this.Commands = commands;
            this.Chat = chat;
            this.Logger = new DiagnosticLogger(logger, pluginInterface);

            this._windows = new WindowSystem();
            this.PluginInterface.UiBuilder.Draw += this._windows.Draw;
            this.PluginInterface.UiBuilder.OpenMainUi += this.UiBuilder_OpenMainUi;

            var commandInfo = new CommandInfo(this.CommandHandler) { HelpMessage = "Open/Close Sonar Diagnostics. Add \"help\" for subcommands." };
            this.Commands.AddHandler("/sonardiagnostics", commandInfo);
            this.Commands.AddHandler("/sonardiag", commandInfo);

            this._container = this.CreateContainer();

            if (pluginInterface.Reason is PluginLoadReason.Installer or PluginLoadReason.Reload) this.UiBuilder_OpenMainUi();
        }

        private void UiBuilder_OpenMainUi()
        {
            this._container.Resolve<MainWindow>().Toggle();
        }

        private void CommandHandler(string command, string args)
        {
            switch (args.ToLower().Trim())
            {
                case "dns":
                    this._container.Resolve<DnsWindow>().Toggle();
                    break;
                case "help":
                    this.Chat.Print($"{command}: Open / Close Sonar Diagnostics window");
                    this.Chat.Print($"{command} dns: Open / Close DNS Tests window");
                    this.Chat.Print($"{command} help: Show this help message");
                    break;
                default:
                    this._container.Resolve<MainWindow>().Toggle();
                    break;
            }
        }

        private Container CreateContainer()
        {
            var container = new Container();

            // Plugin Services
            container.RegisterExports(typeof(Plugin).Assembly);
            container.RegisterInstance(this, setup: Setup.With(preventDisposal: true));
            container.RegisterInstance(this._windows);
            container.RegisterInstance(container, setup: Setup.With(preventDisposal: true));

            // Dalamud Services
            container.RegisterInstance(this.PluginInterface, setup: Setup.With(preventDisposal: true));
            container.RegisterInstance(this.PluginInterface.UiBuilder, setup: Setup.With(preventDisposal: true));
            container.RegisterInstance(this.Commands, setup: Setup.With(preventDisposal: true));
            container.RegisterInstanceMany(this.Logger, setup: Setup.With(preventDisposal: true));

            // Additional Dalamud Services
            var services = new PluginServices();
            this.PluginInterface.Inject(services);
            container.RegisterInstance(services.GameGui);

            this.Logger.Information("Registered services:");
            foreach (var service in container.GetServiceRegistrations())
            {
                this.Logger.Information($" - {service}");
            }

            this.Logger.Information("Validating DryIoC");
            var exceptions = container.Validate();
            foreach (var (service, exception) in exceptions)
            {
                this.Logger.Error(exception, $"Exception in {service.ServiceType.Name}");
            }
            return container;
        }

        public void Dispose()
        {
            this.PluginInterface.UiBuilder.Draw -= this._windows.Draw;
            this.PluginInterface.UiBuilder.OpenMainUi -= this.UiBuilder_OpenMainUi;
            this.Commands.RemoveHandler("/sonardiagnostics");
            this.Commands.RemoveHandler("/sonardiag");
            this._container.Dispose();
        }
    }
}
