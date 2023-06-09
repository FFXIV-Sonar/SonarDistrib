using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using DryIoc;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SonarPlugin
{
    public sealed class SonarPluginStub : IDalamudPlugin
    {
        /// <summary>Sonar name</summary>
        public string Name { get; } = "Sonar";

        /// <summary>Sonar name</summary>
        public string PluginName { get; } = "SonarPlugin";

        /// <summary>Sonar flavor</summary>
        public string? Flavor { get; } = null;


        private SonarPluginIoC? Plugin;
        private DalamudPluginInterface PluginInterface { get; }

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

            try
            {
                var flavor = this.DetermineFlavor();
                if (!string.IsNullOrWhiteSpace(flavor))
                {
                    PluginLog.Information($"Detected Flavor: {flavor}");
                    this.Name = $"{this.Name}-{flavor}";
                    this.PluginName = $"{this.PluginName}-{flavor}";
                    this.Flavor = flavor;
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Exception occured while getting flavor");
            }

            this.SonarOnCommand();

            this.Commands.AddHandler("/sonaron", new CommandInfo(this.SonarOnCommand) { HelpMessage = "Turn on / enable Sonar", ShowInHelp = true });
            this.Commands.AddHandler("/sonarenable", new CommandInfo(this.SonarOnCommand) { HelpMessage = "Turn on / enable Sonar", ShowInHelp = true });

            this.Commands.AddHandler("/sonaroff", new CommandInfo(this.SonarOffCommand) { HelpMessage = "Turn off / disable Sonar", ShowInHelp = true });
            this.Commands.AddHandler("/sonardisable", new CommandInfo(this.SonarOffCommand) { HelpMessage = "Turn off / disable Sonar", ShowInHelp = true });

            this.Commands.AddHandler("/sonarreload", new CommandInfo(this.SonarReloadCommand) { HelpMessage = "Reload Sonar", ShowInHelp = true });
        }

        private string? DetermineFlavor()
        {
            PluginLog.Debug("Determining Flavor");

            // Attempt #1: Flavor resource
            var flavor = GetFlavorResource();
            if (flavor is not null) return flavor;

            // Attempt #2: Testing
            if (this.PluginInterface.IsDev) return "dev";
            else if (this.PluginInterface.IsTesting) return "testing";

            // Attempt #3 and #4: Internal name and Directory name
            flavor = DetermineFlavorCore(this.PluginInterface.InternalName) ?? DetermineFlavorCore(this.PluginInterface.AssemblyLocation.Directory?.Name);
            if (flavor is not null) return flavor;

            // Attempt #5: Give up
            PluginLog.Warning("Unable to determine flavor");
            return null;
        }

        private static string? DetermineFlavorCore(string? input)
        {
            if (input is not null)
            {
                // Attempt #1: SonarPlugin-something
                var match = new Regex(@"^SonarPlugin-(?<flavor>.*)$", RegexOptions.CultureInvariant).Match(input);
                if (match.Success)
                {
                    var flavor = match.Groups["flavor"].Value;
                    if (string.IsNullOrEmpty(flavor)) return "negative";
                    return flavor;
                }

                // Attempt #2: SonarPluginsomething
                match = new Regex(@"^SonarPlugin(?<flavor>.*)$", RegexOptions.CultureInvariant).Match(input);
                if (match.Success)
                {
                    var flavor = match.Groups["flavor"].Value;
                    if (string.IsNullOrEmpty(flavor)) return null; // "SonarPlugin" means no flavor
                    return flavor;
                }

                // Attempt #3: bin (likely to be a dev build)
                match = new Regex(@"^bin$", RegexOptions.CultureInvariant).Match(input);
                if (match.Success) return "dev";
            }

            // Attempt #4: input is the flavor
            if (!string.IsNullOrWhiteSpace(input)) return input;
            
            // Attempt #5: Give up
            return null;
        }

        private static string? GetFlavorResource()
        {
            // Open the Flavor.data embedded resource stream
            var assembly = typeof(SonarPluginStub).Assembly;
            var stream = assembly.GetManifestResourceStream("SonarPlugin.Resources.Flavor.data");
            if (stream is null)
            {
                PluginLog.Warning("Flavor resource not found!");
                return null;
            }

            // Read the stream into a bytes array
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);

            // Decode the flavor string
            var flavor = Encoding.UTF8.GetString(bytes);
            if (string.IsNullOrWhiteSpace(flavor))
            {
                PluginLog.Debug("Resource flavor is empty");
                return null;
            }
            return flavor;
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
            if (this.Plugin is not null) return;
            try
            {
                PluginLog.Debug("Starting Sonar");
                this.ReinitDelay.Wait();
                this.Plugin = new(this, this.PluginInterface);
                this.Plugin.StartServices();
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, string.Empty);
                this.ShowError(ex, "initialized", true);

                if (ex is ContainerException cex && this.Plugin is not null)
                {
                    PluginLog.Error(cex.TryGetDetails(this.Plugin.Container));
                }
                /* Swallow Exception */
            }
        }

        private void DestroySonar()
        {
            if (this.Plugin is null) return;
            try
            {
                PluginLog.Debug("Stopping Sonar");
                this.Plugin?.StopServices();
                this.Plugin?.Dispose();
                this.Plugin = null;
                this.ReinitDelay = Task.Delay(5000);
            }
            catch (Exception ex)
            {
                this.ShowError(ex, "disposed", false);
                PluginLog.Error(ex, string.Empty);
                if (ex is ContainerException cex)
                {
                    PluginLog.Error(cex.TryGetDetails(this.Plugin!.Container));
                }
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

        public void Dispose()
        {
            this.SonarOffCommand();

            this.Commands.RemoveHandler("/sonaron");
            this.Commands.RemoveHandler("/sonarenable");

            this.Commands.RemoveHandler("/sonaroff");
            this.Commands.RemoveHandler("/sonardisable");

            this.Commands.RemoveHandler("/sonarreload");

            GC.SuppressFinalize(this);
        }

        ~SonarPluginStub()
        {
            try { this.Dispose(); } catch (Exception ex) { PluginLog.Error(ex, string.Empty); }
        }
    }
}
