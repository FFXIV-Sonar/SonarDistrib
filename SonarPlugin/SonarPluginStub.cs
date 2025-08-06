using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using DryIoc;
using SonarPlugin.Utility;
using SonarUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SonarPlugin
{
    public sealed class SonarPluginStub : IDalamudPlugin
    {
        private readonly Lock _pluginLock = new(); // Load and unload lock
        private bool _disposed; // Interlocked

        /// <summary>Sonar name</summary>
        public string Name { get; } = "Sonar";

        /// <summary>Sonar name</summary>
        public string PluginName { get; } = "SonarPlugin";

        /// <summary>Sonar flavor</summary>
        public string? Flavor { get; } = null;

        /// <summary>SonarPlugin's IoC class.</summary>
        private SonarPluginIoC? Plugin;

        private IDalamudPluginInterface PluginInterface { get; }
        private ICommandManager Commands { get; }
        private IChatGui Chat { get; }
        private IPluginLog Logger { get; }

        public SonarPluginStub(IDalamudPluginInterface pluginInterface, ICommandManager commands, IChatGui chat, IPluginLog logger)
        {
            this.PluginInterface = pluginInterface;
            this.Commands = commands;
            this.Chat = chat;
            this.Logger = logger;
            
            this.Logger.Debug("Initializing Sonar [Stub]");
            this.PluginInterface = pluginInterface;
            this.PluginInterface.Inject(this);

            try
            {
                var flavor = FlavorUtils.DetermineFlavor(pluginInterface, logger);
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

            this.Commands.AddHandler("/sonarload", new CommandInfo(this.SonarLoadCommand) { HelpMessage = "Turn on / enable Sonar", ShowInHelp = false });
            this.Commands.AddHandler("/sonarunload", new CommandInfo(this.SonarUnloadCommand) { HelpMessage = "Turn off / disable Sonar", ShowInHelp = false });
            this.Commands.AddHandler("/sonarreload", new CommandInfo(this.SonarReloadCommand) { HelpMessage = "Reload Sonar", ShowInHelp = false });

            DnsUtils.Log += this.DnsLogHandler;

            this.InitializeSonar();
        }

        private void SonarLoadCommand(string? _ = null, string? __ = null)
        {
            this.Chat.PrintError("WARNING: /sonarload, /sonarunload and /sonarreload are not yet fixed! Use /sonaron, /sonaroff, /sonarenable and /sonardisable instead.");
            Task.Factory.StartNew(this.InitializeSonar, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach);
        }

        private void SonarUnloadCommand(string? _ = null, string? __ = null)
        {
            this.Chat.PrintError("WARNING: /sonarload, /sonarunload and /sonarreload are not yet fixed! Use /sonaron, /sonaroff, /sonarenable and /sonardisable instead.");
            Task.Factory.StartNew(this.DestroySonar, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach);
        }

        private void SonarReloadCommand(string? _ = null, string? __ = null)
        {
            this.Chat.PrintError("WARNING: /sonarload, /sonarunload and /sonarreload are not yet fixed! Use /sonaron, /sonaroff, /sonarenable and /sonardisable instead.");
#if !DEBUG
            return; // TODO: Remove once fixed
#endif
            Task.Factory.StartNew(this.ReloadSonar, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach);
        }

        private void InitializeSonar()
        {
            if (Volatile.Read(ref this._disposed)) return;
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
            var dalamud = "Check /xllog for more information";
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

        [SuppressMessage("Minor Code Smell", "S3458", Justification = "Clarity")]
        private void DnsLogHandler(string categoryName, DnsClient.Internal.LogLevel logLevel, int eventId, Exception? exception, string message, object[] args)
        {
            message = $"{(string.IsNullOrEmpty(categoryName) ? "[DnsClient]" : $"[{categoryName}]")} {message}";
            switch (logLevel)
            {
                case DnsClient.Internal.LogLevel.Trace:
                    this.Logger.Verbose(exception, message, args);
                    break;
                case DnsClient.Internal.LogLevel.Debug:
                    this.Logger.Debug(exception, message, args);
                    break;
                case DnsClient.Internal.LogLevel.Information:
                    this.Logger.Information(exception, message, args);
                    break;
                case DnsClient.Internal.LogLevel.Warning:
                    this.Logger.Warning(exception, message, args);
                    break;
                case DnsClient.Internal.LogLevel.Error:
                    this.Logger.Error(exception, message, args);
                    break;
                case DnsClient.Internal.LogLevel.Critical:
                default:
                    this.Logger.Fatal(exception, message, args);
                    break;
            }
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref this._disposed, true, false)) return;

            this.Commands.RemoveHandler("/sonaron");
            this.Commands.RemoveHandler("/sonarenable");

            this.Commands.RemoveHandler("/sonaroff");
            this.Commands.RemoveHandler("/sonardisable");

            this.Commands.RemoveHandler("/sonarreload");

            this.DestroySonar();
            DnsUtils.Log -= this.DnsLogHandler;

            GC.SuppressFinalize(this);
        }

        ~SonarPluginStub()
        {
            try { this.Dispose(); } catch (Exception ex) { this.Logger.Error(ex, string.Empty); }
        }
    }
}
