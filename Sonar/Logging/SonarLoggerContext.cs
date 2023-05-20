using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Logging
{
    public class SonarLoggerContext : ISonarLogger
    {
        private ISonarLogger Target { get; }
        public string ContextName { get; }

        public SonarLoggerContext(ISonarLogger target, string contextName)
        {
            this.Target = target;
            this.ContextName = contextName;
        }

        public void Log(string message, SonarLogLevel level = SonarLogLevel.Debug) => this.Target.Log($"[{this.ContextName}] {message}", level);
        public bool LogEnabled(SonarLogLevel level = SonarLogLevel.Debug) => this.Target.LogEnabled(level);
    }

    public sealed class ContextSonarLogger<T> : SonarLoggerContext
    {
        public ContextSonarLogger(ISonarLogger target) : base(target, typeof(T).Name) { }
    }
}