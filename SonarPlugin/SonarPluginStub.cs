using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SonarPlugin
{
    public sealed class SonarPluginStub : IDalamudPlugin
    {
        public string Name => "Sonar";

        private SonarPluginIoC? Plugin;
        public DalamudPluginInterface PluginInterface { get; }

        [PluginService] public CommandManager Commands { get; private set; } = default!;
        [PluginService] public ChatGui Chat { get; private set; } = default!;

        private Task ReinitDelay = Task.CompletedTask;

        private readonly object SonarTaskLock = new();
        private Task? SonarTask;

        public SonarPluginStub(DalamudPluginInterface pluginInterface)
        {
            PluginLog.Debug("Initializing Sonar [Stub]");
            this.PluginInterface = pluginInterface;
            this.PluginInterface.Inject(this);

            this.SonarOnCommand();

            this.Commands.AddHandler("/sonaron", new CommandInfo(this.SonarOnCommand) { HelpMessage = "Turn on / enable Sonar", ShowInHelp = true });
            this.Commands.AddHandler("/sonarenable", new CommandInfo(this.SonarOnCommand) { HelpMessage = "Turn on / enable Sonar", ShowInHelp = true });

            this.Commands.AddHandler("/sonaroff", new CommandInfo(this.SonarOffCommand) { HelpMessage = "Turn off / disable Sonar", ShowInHelp = true });
            this.Commands.AddHandler("/sonardisable", new CommandInfo(this.SonarOffCommand) { HelpMessage = "Turn off / disable Sonar", ShowInHelp = true });

            this.Commands.AddHandler("/sonarreload", new CommandInfo(this.SonarReloadCommand) { HelpMessage = "Reload Sonar", ShowInHelp = true });
        }

        private void SonarOnCommand(string? _ = null, string? __ = null)
        {
            lock (this.SonarTaskLock)
            {
                if (!this.SonarTask?.IsCompleted ?? false) return;
                this.SonarTask = Task.Factory.StartNew(this.InitializeSonar, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach);
            }
        }

        private void SonarOffCommand(string? _ = null, string? __ = null)
        {
            lock (this.SonarTaskLock)
            {
                if (!this.SonarTask?.IsCompleted ?? false) return;
                this.SonarTask = Task.Factory.StartNew(this.DestroySonar, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach);
            }
        }

        private void SonarReloadCommand(string? _ = null, string? __ = null)
        {
            lock (this.SonarTaskLock)
            {
                if (!this.SonarTask?.IsCompleted ?? false) return;
                this.SonarTask = Task.Factory.StartNew(this.ReloadSonar, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach);
            }
        }

        private void InitializeSonar()
        {
            if (this.IsDisposed) throw new ObjectDisposedException("SonarPlugin");
            if (this.Plugin is not null) return;
            try
            {
                PluginLog.Debug("Starting Sonar");
                this.ReinitDelay.Wait();
                this.Plugin = new(this.PluginInterface);
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, string.Empty);
                this.ShowError(ex, "initialized", true);
                /* Swallow Exception */
            }
        }

        private void DestroySonar()
        {
            if (this.Plugin is null) return;
            try
            {
                PluginLog.Debug("Stopping Sonar");
                this.Plugin?.Dispose();
                this.Plugin = null;
                this.ReinitDelay = Task.Delay(5000);
            }
            catch (Exception ex)
            {
                this.ShowError(ex, "disposed", false);
                PluginLog.Error(ex, string.Empty);
                /* Swallow Exception */
            }
        }

        private void ReloadSonar()
        {
            this.DestroySonar();
            this.InitializeSonar();
        }

        public void ShowError(Exception ex, string action = "initialized", bool isAsync = false)
        {
            var header = $"Sonar could not be {action} {(isAsync ? "in async context" : "")}";
            var dalamud = "Check dalamud.log for more information";
            var footer = "Sonar may be in an undefined state, a game restart may be required.";
            var contact = "If this problem persist report it to https://discord.gg/K7y24Rr";

            PluginLog.Error(header);
            this.Chat.PrintError(header);

            try { if (ex is AggregateException aex) ex = aex.Flatten(); } catch { /* Swallow */ }
            PluginLog.Error($"{ex}");
            try
            {
                if (ex is ReflectionTypeLoadException rexs)
                {
                    foreach (var rex in rexs.LoaderExceptions.Where(ex => ex is not null))
                    {
                        var rrex = rex;
                        try { if (rrex is AggregateException aex) rrex = aex.Flatten(); } catch { /* Swallow */ }
                        PluginLog.Error($"{rrex}");
                    }
                }
            }
            catch (Exception ex2) { PluginLog.Error(ex2, string.Empty); }
            this.Chat.PrintError(dalamud);

            PluginLog.Error(footer);
            this.Chat.PrintError(footer);

            PluginLog.Error(contact);
            this.Chat.PrintError(contact);
        }

        private int disposed;
        public bool IsDisposed => this.disposed == 1;

        private void Dispose(bool disposing)
        {
            if (Interlocked.Exchange(ref this.disposed, 1) == 1) return;
            if (!disposing) return;
            this.DestroySonar();

            this.Commands.RemoveHandler("/sonaron");
            this.Commands.RemoveHandler("/sonarenable");

            this.Commands.RemoveHandler("/sonaroff");
            this.Commands.RemoveHandler("/sonardisable");

            this.Commands.RemoveHandler("/sonarreload");
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SonarPluginStub()
        {
            try { this.Dispose(false); } catch (Exception ex) { PluginLog.Error(ex, string.Empty); }
        }
    }
}
