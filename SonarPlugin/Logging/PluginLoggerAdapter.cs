using Microsoft.Extensions.Logging;
using System;

namespace SonarPlugin.Logging
{
    public sealed class PluginLoggerAdapter<T> : ILogger<T>
    {
        private readonly ILogger _logger;

        public PluginLoggerAdapter(ILoggerFactory factory)
        {
            this._logger = factory.CreateLogger(typeof(T).Name);
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => this._logger.BeginScope(state);

        public bool IsEnabled(LogLevel logLevel) => this._logger.IsEnabled(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) => this._logger.Log(logLevel, eventId, state, exception, formatter);
    }
}
