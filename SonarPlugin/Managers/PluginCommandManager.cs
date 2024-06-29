using SonarPlugin.Attributes;
using Dalamud.Game.Command;
using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Dalamud.Game.Command.CommandInfo;
using Dalamud.Plugin.Services;

namespace SonarPlugin.Managers
{
    public sealed class PluginCommandManager<THost> : IDisposable where THost : notnull
    {
        private readonly (string, CommandInfo)[] _pluginCommands;
        private readonly MethodInfo? _loaderAssemblySetter;

        private ICommandManager Commands { get; }
        private THost Host { get; }
        private IPluginLog Logger { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "<Pending>")]
        public PluginCommandManager(THost host, ICommandManager commands, IPluginLog logger)
        {
            this.Commands = commands;
            this.Host = host;
            this.Logger = logger;

            this._loaderAssemblySetter = typeof(CommandInfo).GetProperty("LoaderAssemblyName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty)?.SetMethod;

            this._pluginCommands = host!.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                .Where(method => method.GetCustomAttribute<CommandAttribute>() != null)
                .SelectMany(this.GetCommandInfoTuple)
                .ToArray();

            this.AddComandHandlers();
        }

        private void AddComandHandlers()
        {
            this.Logger.Debug("Adding command handlers for {type}", this.Host.GetType().Name);
            for (var i = 0; i < this._pluginCommands.Length; i++)
            {
                var (command, commandInfo) = this._pluginCommands[i];
                this.Logger.Verbose(" - {command}", command);
                this.Commands.AddHandler(command, commandInfo);
            }
        }

        private void RemoveCommandHandlers()
        {
            this.Logger.Debug("Removing command handlers for {type}", this.Host.GetType().Name);
            for (var i = 0; i < this._pluginCommands.Length; i++)
            {
                var (command, _) = this._pluginCommands[i];
                this.Logger.Verbose(" - {command}", command);
                this.Commands.RemoveHandler(command);
            }
        }

        private IEnumerable<(string, CommandInfo)> GetCommandInfoTuple(MethodInfo method)
        {
            var handlerDelegate = (IReadOnlyCommandInfo.HandlerDelegate)Delegate.CreateDelegate(typeof(IReadOnlyCommandInfo.HandlerDelegate), this.Host, method);

            var command = handlerDelegate.Method.GetCustomAttribute<CommandAttribute>()!;
            var aliases = handlerDelegate.Method.GetCustomAttribute<AliasesAttribute>();
            var helpMessage = handlerDelegate.Method.GetCustomAttribute<HelpMessageAttribute>();
            var showInHelp = handlerDelegate.Method.GetCustomAttribute<ShowInHelpAttribute>();

            var commandInfo = new CommandInfo(handlerDelegate)
            {
                HelpMessage = helpMessage?.HelpMessage ?? string.Empty,
                ShowInHelp = showInHelp is not null,
            };

            // Fix commands not showing in plugins window
            this._loaderAssemblySetter?.Invoke(commandInfo, new[] { "SonarPlugin" }); // SonarPlugin is so...special D:

            // Create list of tuples that will be filled with one tuple per alias, in addition to the base command tuple.
            var commandInfoTuples = new List<(string, CommandInfo)> { (command.Command, commandInfo) };
            if (aliases != null)
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                for (var i = 0; i < aliases.Aliases.Length; i++)
                {
                    commandInfoTuples.Add((aliases.Aliases[i], commandInfo));
                }
            }

            return commandInfoTuples;
        }

        public void Dispose()
        {
            this.RemoveCommandHandlers();
        }
    }
}
