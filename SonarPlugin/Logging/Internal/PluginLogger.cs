using Dalamud.Plugin.Services;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using System;

namespace SonarPlugin.Logging.Internal
{
    public class PluginLogger : ILogger
    {
        private readonly string? _categoryName;
        private IPluginLog Logger { get; }


        public PluginLogger(string? categoryName, IPluginLog logger)
        {
            this._categoryName = categoryName;
            this.Logger = logger;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

        public bool IsEnabled(LogLevel logLevel)
        {
            return (LogEventLevel)logLevel >= this.Logger.MinimumLogLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel is LogLevel.None || !this.IsEnabled(logLevel)) return;

            var message = formatter(state, exception);
            if (this._categoryName is not null) message = $"[{this._categoryName}] {message}";

            switch (logLevel)
            {
                case LogLevel.Trace:
                    this.Logger.Verbose(exception, message);
                    break;
                case LogLevel.Debug:
                    this.Logger.Debug(exception, message);
                    break;
                case LogLevel.Information:
                    this.Logger.Information(exception, message);
                    break;
                case LogLevel.Warning:
                    this.Logger.Warning(exception, message);
                    break;
                case LogLevel.Error:
                    this.Logger.Error(exception, message);
                    break;
                case LogLevel.Critical:
                    this.Logger.Fatal(exception, message);
                    break;
                default:
                    this.Logger.Information(exception, $"[LogLevel: {logLevel}] {message}");
                    break;
            }
        }
    }
}
