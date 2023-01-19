using SonarPlugin.Attributes;
using SonarPlugin.Managers;
using System;
using SonarGUI;
using SonarPlugin.GUI;
using Dalamud.Game.Command;
using Dalamud.Logging;
using System.Threading.Tasks;
using System.Threading;

namespace SonarPlugin
{
    public sealed class SonarCommands : IHostedService
    {
        private PluginCommandManager<SonarCommands> _commandManager;

        private SonarPlugin Plugin { get; }
        private SonarMainOverlay MainWindow { get; }
        private SonarConfigWindow ConfigWindow { get; }
        private CommandManager Commands { get; }

        public SonarCommands(SonarPlugin plugin, SonarMainOverlay mainWindow, CommandManager commands, SonarConfigWindow configWindow)
        {
            this.Plugin = plugin;
            this.MainWindow = mainWindow;
            this.Commands = commands;
            this.ConfigWindow = configWindow;
            PluginLog.LogInformation("Sonar Commands Initialized");
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
            this.Plugin.SonarGUI.OpenSupportWindow();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this._commandManager = new PluginCommandManager<SonarCommands>(this, this.Commands);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this._commandManager?.Dispose();
            return Task.CompletedTask;
        }
    }
}
