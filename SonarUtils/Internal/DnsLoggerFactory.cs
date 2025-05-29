using DnsClient.Internal;
using System.Collections.Concurrent;

namespace SonarUtils.Internal
{
    internal class DnsLoggerFactory : ILoggerFactory
    {
        private static readonly ConcurrentDictionary<string, DnsLogger> s_loggers = new();

        public ILogger CreateLogger(string categoryName)
            => s_loggers.GetOrAdd(categoryName, static categoryName => new(categoryName));
    }
}
