using Dalamud.Plugin.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using DryIoc;
using SonarPlugin.Logging.Internal;

namespace SonarPlugin.Logging
{
    public static class PluginLoggerExtensions
    {
        public static ILoggingBuilder AddPluginLogger(this ILoggingBuilder builder)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, PluginLoggerProvider>(static services => new(services.GetRequiredService<IPluginLog>())));
            return builder;
        }

        public static Container AddPluginLogger(this Container container)
        {
            container.RegisterMany<PluginLoggerProvider>(Reuse.Singleton);
            return container;
        }
    }
}
