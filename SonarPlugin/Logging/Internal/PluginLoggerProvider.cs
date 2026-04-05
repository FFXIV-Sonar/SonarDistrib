using Dalamud.Plugin.Services;
using Microsoft.Extensions.Logging;

namespace SonarPlugin.Logging.Internal
{
    public sealed class PluginLoggerProvider : ILoggerProvider
    {
        private IPluginLog Logger { get; }

        public PluginLoggerProvider(IPluginLog logger)
        {
            this.Logger = logger;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new PluginLogger(categoryName, this.Logger);
        }

        public void Dispose()
        {
            // Nothing to do
        }
    }
}
