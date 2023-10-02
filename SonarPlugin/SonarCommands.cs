using SonarPlugin.Attributes;
using SonarPlugin.Managers;
using System;
using SonarPlugin.GUI;
using Dalamud.Game.Command;
using Dalamud.Logging;
using System.Threading.Tasks;
using System.Threading;
using Dalamud.Interface.Windowing;
using Sonar;
using Dalamud.Plugin.Services;

namespace SonarPlugin
{
    public sealed class SonarCommands : IHostedService
    {
        private PluginCommandManager<SonarCommands>? _commandManager;

        private SonarPlugin Plugin { get; }
        private SonarClient Client { get; }
        private WindowSystem Windows { get; }
        private SonarMainOverlay MainWindow { get; }
        private SonarConfigWindow ConfigWindow { get; }
        private SonarTrackerWindow TrackerWindow { get; }
        private ICommandManager Commands { get; }
        private IPluginLog Logger { get; }

        public SonarCommands(SonarPlugin plugin, SonarClient client, WindowSystem windows, SonarMainOverlay mainWindow, ICommandManager commands, SonarConfigWindow configWindow, SonarTrackerWindow trackerWindow, IPluginLog logger)
        {
            this.Plugin = plugin;
            this.Client = client;
            this.Windows = windows;
            this.MainWindow = mainWindow;
            this.Commands = commands;
            this.ConfigWindow = configWindow;
            this.TrackerWindow = trackerWindow;
            this.Logger = logger;
            this.Logger.Information("Sonar Commands Initialized");
        }

        [Command("/sonar")]
        [HelpMessage("Open/close Sonar's main window")]
        [ShowInHelp]
        private void ToggleMainWindowCommand(string command, string args)
        {
            this.MainWindow.IsVisible = !this.MainWindow.IsVisible;
        }

        [Command("/sonarconfig")]
        [Aliases("/sonarcfg")]
        [HelpMessage("Open/close Sonar's configuration")]
        [ShowInHelp]
        private void ToggleConfigWindowCommand(string command, string args)
        {
            this.ConfigWindow.Toggle();
        }

        [Command("/sonartracker")]
        [HelpMessage("Open/close Sonar's tracker")]
        [DoNotShowInHelp]
        private void ToggleTrackerWindowCommand(string command, string args)
        {
            this.TrackerWindow.Toggle();
        }

        [Command("/sonarerror")]
        [HelpMessage("Open/close Sonar errors window")]
        [DoNotShowInHelp]
        private void ToggleErrorWindowCommand(string command, string args)
        {
            /* Reserved */
        }

        [Command("/sonarsupport")]
        [HelpMessage("Contact Sonar Support")]
        [ShowInHelp]
        private void SonarSupportCommand(string command, string args)
        {
            SupportWindow.CreateWindow(this.Windows, this.Client);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this._commandManager = new PluginCommandManager<SonarCommands>(this, this.Commands, this.Logger);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this._commandManager?.Dispose();
            return Task.CompletedTask;
        }
    }
}
