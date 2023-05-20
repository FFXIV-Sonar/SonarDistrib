using Loyc;
using Sonar.Logging;
using System;

namespace Sonar
{
    public sealed partial class SonarClient
    {
        private readonly SonarLogger baseLogger = new();
        private readonly ISonarLogger serverLogger;

        internal ISonarLogger Logger { get; }
        
        /// <summary>
        /// Debug log level
        /// </summary>
        public SonarLogLevel LogLevel
        {
            get => this.Configuration.LogLevel;
            set
            {
                this.baseLogger.Level = value;
                this.Configuration.LogLevel = value;
            }
        }

        internal void Log(string message, SonarLogLevel level = SonarLogLevel.Debug) => this.Logger.Log(message, level);
        internal void Log(Func<string> messageFactory, SonarLogLevel level = SonarLogLevel.Debug) => this.Logger.Log(messageFactory(), level);
        internal void Log(Exception? exception, string message, SonarLogLevel level = SonarLogLevel.Debug) => this.Logger.Log(exception, message, level);
        internal void Log(Exception? exception, Func<string> messageFactory, SonarLogLevel level = SonarLogLevel.Debug) => this.Logger.Log(exception, messageFactory(), level);
        internal void LogVerbose(string message) => this.Log(message, SonarLogLevel.Verbose);
        internal void LogVerbose(Func<string> messageFactory) { if (this.LogVerboseEnabled) this.LogVerbose(messageFactory()); }
        internal void LogVerbose(Exception? exception, string message) => this.Log(exception, message, SonarLogLevel.Verbose);
        internal void LogVerbose(Exception? exception, Func<string> messageFactory) { if (this.LogVerboseEnabled) this.LogVerbose(exception, messageFactory()); }
        internal void LogDebug(string message) => this.Log(message, SonarLogLevel.Debug);
        internal void LogDebug(Func<string> messageFactory) { if (this.LogDebugEnabled) this.LogDebug(messageFactory()); }
        internal void LogDebug(Exception? exception, string message) => this.Log(exception, message, SonarLogLevel.Debug);
        internal void LogDebug(Exception? exception, Func<string> messageFactory) { if (this.LogDebugEnabled) this.LogDebug(exception, messageFactory()); }
        internal void LogInformation(string message) => this.Log(message, SonarLogLevel.Information);
        internal void LogInformation(Func<string> messageFactory) { if (this.LogInformationEnabled) this.LogInformation(messageFactory()); }
        internal void LogInformation(Exception? exception, string message) => this.Log(exception, message, SonarLogLevel.Information);
        internal void LogInformation(Exception? exception, Func<string> messageFactory) { if (this.LogInformationEnabled) this.LogInformation(exception, messageFactory()); }
        internal void LogWarning(string message) => this.Log(message, SonarLogLevel.Warning);
        internal void LogWarning(Func<string> messageFactory) { if (this.LogWarningEnabled) this.LogWarning(messageFactory()); }
        internal void LogWarning(Exception? exception, string message) => this.Log(exception, message, SonarLogLevel.Warning);
        internal void LogWarning(Exception? exception, Func<string> messageFactory) { if (this.LogWarningEnabled) this.LogWarning(exception, messageFactory()); }
        internal void LogError(string message) => this.Log(message, SonarLogLevel.Error);
        internal void LogError(Func<string> messageFactory) { if (this.LogErrorEnabled) this.LogError(messageFactory()); }
        internal void LogError(Exception? exception, string message) => this.Log(exception, message, SonarLogLevel.Error);
        internal void LogError(Exception? exception, Func<string> messageFactory) { if (this.LogErrorEnabled) this.LogError(exception, messageFactory()); }
        internal void LogFatal(string message) => this.Log(message, SonarLogLevel.Fatal);
        internal void LogFatal(Func<string> messageFactory) { if (this.LogFatalEnabled) this.LogFatal(messageFactory()); }
        internal void LogFatal(Exception? exception, string message) => this.Log(exception, message, SonarLogLevel.Fatal);
        internal void LogFatal(Exception? exception, Func<string> messageFactory) { if (this.LogFatalEnabled) this.LogFatal(exception, messageFactory()); }

        internal bool LogEnabled(SonarLogLevel level = SonarLogLevel.Debug) => this.baseLogger.LogEnabled(level);
        internal bool LogVerboseEnabled => this.LogEnabled(SonarLogLevel.Verbose);
        internal bool LogDebugEnabled => this.LogEnabled(SonarLogLevel.Debug);
        internal bool LogInformationEnabled => this.LogEnabled(SonarLogLevel.Information);
        internal bool LogWarningEnabled => this.LogEnabled(SonarLogLevel.Warning);
        internal bool LogErrorEnabled => this.LogEnabled(SonarLogLevel.Error);
        internal bool LogFatalEnabled => this.LogEnabled(SonarLogLevel.Fatal);

        private void LogHandler(ISonarLogger source, SonarLogMessage log)
        {
            try
            {
                this.LogMessage?.Invoke(this, log);
            }
            catch
            {
                /* Swallow */
            }
        }

        public event SonarClientLogHandler? LogMessage;
    }
}
