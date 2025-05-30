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
using System.Reflection.Metadata;

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
        private IChatGui Chat { get; }
        private IPluginLog Logger { get; }

        public SonarCommands(SonarPlugin plugin, SonarClient client, WindowSystem windows, SonarMainOverlay mainWindow, IChatGui chat, ICommandManager commands, SonarConfigWindow configWindow, SonarTrackerWindow trackerWindow, IPluginLog logger)
        {
            this.Plugin = plugin;
            this.Client = client;
            this.Windows = windows;
            this.MainWindow = mainWindow;
            this.Commands = commands;
            this.Chat = chat;
            this.ConfigWindow = configWindow;
            this.TrackerWindow = trackerWindow;
            this.Logger = logger;
            this.Logger.Information("Sonar Commands Initialized");
        }

        [Command("/sonar")]
        [HelpMessage("Sonar 메인 창")]
        [ShowInHelp]
        private void ToggleMainWindowCommand(string command, string args)
        {
            this.MainWindow.IsVisible = !this.MainWindow.IsVisible;
        }

        [Command("/sonarconfig")]
        [Aliases("/sonarcfg")]
        [HelpMessage("Sonar 설정")]
        [ShowInHelp]
        private void ToggleConfigWindowCommand(string command, string args)
        {
            this.ConfigWindow.Toggle();
        }

        [Command("/sonartracker")]
        [HelpMessage("Sonar 트래커")]
        [DoNotShowInHelp]
        private void ToggleTrackerWindowCommand(string command, string args)
        {
            this.TrackerWindow.Toggle();
        }

        [Command("/sonarerror")]
        [HelpMessage("Sonar 오류 창")]
        [DoNotShowInHelp]
        private void ToggleErrorWindowCommand(string command, string args)
        {
            /* Reserved */
        }

        [Command("/sonarsupport")]
        [HelpMessage("Sonar Support 폼 작성")]
        [ShowInHelp]
        private void SonarSupportCommand(string command, string args)
        {
            SupportWindow.CreateWindow(this.Windows, this.Client);
        }

        [Command("/sonaron")]
        [Aliases("/sonarenable")]
        [HelpMessage("전파 기여 활성화")]
        [ShowInHelp]
        private void SonarOnCommand(string command, string args)
        {
            this.Client.Configuration.Contribute.Global = true;
            this.Plugin.SaveConfiguration(true);
        }

        [Command("/sonaroff")]
        [Aliases("/sonardisable")]
        [HelpMessage("전파 기여 비활성화")]
        [ShowInHelp]
        private void SonarOffCommand(string command, string args)
        {
            this.Client.Configuration.Contribute.Global = false;
            this.Plugin.SaveConfiguration(true);
        }

        [Command("/sonartoggle")]
        [HelpMessage("전파 기여 활성화/비활성화")]
        [ShowInHelp]
        private void SonarToggleCommand(string command, string args)
        {
            this.Client.Configuration.Contribute.Global = !this.Client.Configuration.Contribute.Global;
            this.Plugin.SaveConfiguration(true);
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
