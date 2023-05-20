using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Logging
{
    public sealed class SonarLogger : ISonarLogger
    {
        public SonarLogLevel Level { get; set; } = SonarLogLevel.Information;

        public void Log(string message, SonarLogLevel level = SonarLogLevel.Debug) => this.LogMessage?.Invoke(this, new() { Level = level, Message = message });
        public bool LogEnabled(SonarLogLevel level = SonarLogLevel.Debug) => ISonarLogger.LogEnabled(level, this.Level);
        public event SonarLoggerHandler? LogMessage;
    }
    public delegate void SonarLoggerHandler(ISonarLogger source, SonarLogMessage log);
}
