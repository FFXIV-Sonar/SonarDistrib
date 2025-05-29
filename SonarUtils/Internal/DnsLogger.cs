using DnsClient.Internal;
using System;

namespace SonarUtils.Internal
{
    internal class DnsLogger : ILogger
    {
        private readonly string _category;

        public DnsLogger(string categoryName)
        {
            this._category = categoryName;
        }

        public bool IsEnabled(LogLevel logLevel) => logLevel >= DnsUtils.LogLevel;

        public void Log(LogLevel logLevel, int eventId, Exception exception, string message, params object[] args)
        {
            if (this.IsEnabled(logLevel)) DnsUtils.DispatchLogEvent(this._category, logLevel, eventId, exception, message, args);
        }
    }
}
