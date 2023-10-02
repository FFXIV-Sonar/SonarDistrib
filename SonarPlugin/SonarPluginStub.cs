using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
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


        private readonly object _pluginLock = new object();
        private SonarPluginIoC? Plugin;
        private DalamudPluginInterface PluginInterface { get; }

        [PluginService] public ICommandManager Commands { get; private set; } = default!;
        [PluginService] public IChatGui Chat { get; private set; } = default!;
        [PluginService] public IPluginLog Logger { get; private set; } = default!;

        private readonly object _taskLock = new();
        private Task _sonarTask = Task.CompletedTask;

        public SonarPluginStub(DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Inject(this);
            this.Logger.Debug("Initializing Sonar [Stub]");
            this.PluginInterface = pluginInterface;
            this.PluginInterface.Inject(this);

            try
            {
                var flavor = this.DetermineFlavor();
                if (!string.IsNullOrWhiteSpace(flavor))
                {
                    this.Logger.Information($"Detected Flavor: {flavor}");
                    this.Name = $"{this.Name}-{flavor}";
                    this.PluginName = $"{this.PluginName}-{flavor}";
                    this.Flavor = flavor;
                }
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, "Exception occured while getting flavor");
            }

            this.Commands.AddHandler("/sonaron", new CommandInfo(this.SonarOnCommand) { HelpMessage = "Turn on / enable Sonar", ShowInHelp = true });
            this.Commands.AddHandler("/sonarenable", new CommandInfo(this.SonarOnCommand) { HelpMessage = "Turn on / enable Sonar", ShowInHelp = true });
            this.Commands.AddHandler("/sonaroff", new CommandInfo(this.SonarOffCommand) { HelpMessage = "Turn off / disable Sonar", ShowInHelp = true });
            this.Commands.AddHandler("/sonardisable", new CommandInfo(this.SonarOffCommand) { HelpMessage = "Turn off / disable Sonar", ShowInHelp = true });
            this.Commands.AddHandler("/sonarreload", new CommandInfo(this.SonarReloadCommand) { HelpMessage = "Reload Sonar", ShowInHelp = true });

            this.InitializeSonar();
        }

        private string? DetermineFlavor()
        {
            this.Logger.Debug("Determining Flavor");

            // Attempt #1: Flavor resource
            var flavor = this.GetFlavorResource();
            if (flavor is not null) return flavor;

            // Attempt #2: Testing
            if (this.PluginInterface.IsDev) return "dev";
            else if (this.PluginInterface.IsTesting) return "testing";

            // Attempt #3 and #4: Internal name and Directory name
            flavor = DetermineFlavorCore(this.PluginInterface.InternalName) ?? DetermineFlavorCore(this.PluginInterface.AssemblyLocation.Directory?.Name);
            if (flavor is not null) return flavor;

            // Attempt #5: Give up
            this.Logger.Warning("Unable to determine flavor");
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

        private string? GetFlavorResource()
        {
            // Open the Flavor.data embedded resource stream
            var assembly = typeof(SonarPluginStub).Assembly;
            var stream = assembly.GetManifestResourceStream("SonarPlugin.Resources.Flavor.data");
            if (stream is null)
            {
                this.Logger.Warning("Flavor resource not found!");
                return null;
            }

            // Read the stream into a bytes array
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);

            // Decode the flavor string
            var flavor = Encoding.UTF8.GetString(bytes);
            if (string.IsNullOrWhiteSpace(flavor))
            {
                this.Logger.Debug("Resource flavor is empty");
                return null;
            }
            return flavor;
        }

        private void SonarOnCommand(string? _ = null, string? __ = null)
        {
            lock (this._taskLock)
            {
                if (!this._sonarTask.IsCompleted) return;
                this._sonarTask = Task.Factory.StartNew(this.InitializeSonar, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach);
            }
        }

        private void SonarOffCommand(string? _ = null, string? __ = null)
        {
            lock (this._taskLock)
            {
                if (!this._sonarTask.IsCompleted) return;
                this._sonarTask = Task.Factory.StartNew(this.DestroySonar, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach);
            }
        }

        private void SonarReloadCommand(string? _ = null, string? __ = null)
        {
            lock (this._taskLock)
            {
                if (!this._sonarTask.IsCompleted) return;
                this._sonarTask = Task.Factory.StartNew(this.ReloadSonar, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach);
            }
        }

        private void InitializeSonar()
        {
            lock (this._pluginLock)
            {
                if (this.Plugin is not null) return;
                try
                {
                    this.Logger.Debug("Starting Sonar");
                    this.Plugin = new(this, this.PluginInterface);
                    this.Plugin.StartServices();
                }
                catch (Exception ex)
                {
                    this.Logger.Error(ex, string.Empty);
                    this.ShowError(ex, "initialized", true);

                    if (ex is ContainerException cex && this.Plugin is not null)
                    {
                        this.Logger.Error(cex.TryGetDetails(this.Plugin.Container));
                    }
                    /* Swallow Exception */
                }
            }
        }

        private void DestroySonar()
        {
            lock (this._pluginLock)
            {
                if (this.Plugin is null) return;
                try
                {
                    this.Logger.Debug("Stopping Sonar");
                    this.Plugin?.StopServices();
                    this.Plugin?.Dispose();
                    this.Plugin = null;
                }
                catch (Exception ex)
                {
                    this.ShowError(ex, "disposed", false);
                    this.Logger.Error(ex, string.Empty);
                    if (ex is ContainerException cex)
                    {
                        this.Logger.Error(cex.TryGetDetails(this.Plugin!.Container));
                    }
                    /* Swallow Exception */
                }
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

            this.Logger.Error(header);
            this.Chat.PrintError(header);

            try { if (ex is AggregateException aex) ex = aex.Flatten(); } catch { /* Swallow */ }
            this.Logger.Error($"{ex}");
            try
            {
                if (ex is ReflectionTypeLoadException rexs)
                {
                    foreach (var rex in rexs.LoaderExceptions.Where(ex => ex is not null))
                    {
                        var rrex = rex;
                        try { if (rrex is AggregateException aex) rrex = aex.Flatten(); } catch { /* Swallow */ }
                        this.Logger.Error($"{rrex}");
                    }
                }
            }
            catch (Exception ex2) { this.Logger.Error(ex2, string.Empty); }
            this.Chat.PrintError(dalamud);

            this.Logger.Error(footer);
            this.Chat.PrintError(footer);

            this.Logger.Error(contact);
            this.Chat.PrintError(contact);
        }

        public void Dispose()
        {
            this.Commands.RemoveHandler("/sonaron");
            this.Commands.RemoveHandler("/sonarenable");

            this.Commands.RemoveHandler("/sonaroff");
            this.Commands.RemoveHandler("/sonardisable");

            this.Commands.RemoveHandler("/sonarreload");

            lock (this._taskLock) this._sonarTask.Wait();
            this.DestroySonar();

            GC.SuppressFinalize(this);
        }

        ~SonarPluginStub()
        {
            try { this.Dispose(); } catch (Exception ex) { this.Logger.Error(ex, string.Empty); }
        }
    }
}
