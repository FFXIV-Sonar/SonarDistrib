using Sonar.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Logging
{
    public interface ISonarLogger
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S4136:Method overloads should be grouped together", Justification = "Intentional")]
        public void Log(string message, SonarLogLevel level = SonarLogLevel.Debug);
        public bool LogEnabled(SonarLogLevel level = SonarLogLevel.Debug);

        #region Default Implementations
        public void Log(Exception? exception, string message, SonarLogLevel level = SonarLogLevel.Debug)
        {
            if (exception is not null) this.Log($"{message}\n{ExtractExceptionString(exception)}", level);
            else this.Log($"{message}", level);
        }

        public void Log(Func<string> messageFactory, SonarLogLevel level = SonarLogLevel.Debug) => this.Log(messageFactory(), level);
        public void Log(Exception? exception, Func<string> messageFactory, SonarLogLevel level = SonarLogLevel.Debug) => this.Log(exception, messageFactory(), level);
        public void LogVerbose(string message) => this.Log(message, SonarLogLevel.Verbose);
        public void LogVerbose(Func<string> messageFactory) { if (this.LogVerboseEnabled) this.LogVerbose(messageFactory()); }
        public void LogVerbose(Exception? exception, string message) => this.Log(exception, message, SonarLogLevel.Verbose);
        public void LogVerbose(Exception? exception, Func<string> messageFactory) { if (this.LogVerboseEnabled) this.LogVerbose(exception, messageFactory()); }
        public void LogDebug(string message) => this.Log(message, SonarLogLevel.Debug);
        public void LogDebug(Func<string> messageFactory) { if (this.LogDebugEnabled) this.LogDebug(messageFactory()); }
        public void LogDebug(Exception? exception, string message) => this.Log(exception, message, SonarLogLevel.Debug);
        public void LogDebug(Exception? exception, Func<string> messageFactory) { if (this.LogDebugEnabled) this.LogDebug(exception, messageFactory()); }
        public void LogInformation(string message) => this.Log(message, SonarLogLevel.Information);
        public void LogInformation(Func<string> messageFactory) { if (this.LogInformationEnabled) this.LogInformation(messageFactory()); }
        public void LogInformation(Exception? exception, string message) => this.Log(exception, message, SonarLogLevel.Information);
        public void LogInformation(Exception? exception, Func<string> messageFactory) { if (this.LogInformationEnabled) this.LogInformation(exception, messageFactory()); }
        public void LogWarning(string message) => this.Log(message, SonarLogLevel.Warning);
        public void LogWarning(Func<string> messageFactory) { if (this.LogWarningEnabled) this.LogWarning(messageFactory()); }
        public void LogWarning(Exception? exception, string message) => this.Log(exception, message, SonarLogLevel.Warning);
        public void LogWarning(Exception? exception, Func<string> messageFactory) { if (this.LogWarningEnabled) this.LogWarning(exception, messageFactory()); }
        public void LogError(string message) => this.Log(message, SonarLogLevel.Error);
        public void LogError(Func<string> messageFactory) { if (this.LogErrorEnabled) this.LogError(messageFactory()); }
        public void LogError(Exception? exception, string message) => this.Log(exception, message, SonarLogLevel.Error);
        public void LogError(Exception? exception, Func<string> messageFactory) { if (this.LogErrorEnabled) this.LogError(exception, messageFactory()); }
        public void LogFatal(string message) => this.Log(message, SonarLogLevel.Fatal);
        public void LogFatal(Func<string> messageFactory) { if (this.LogFatalEnabled) this.LogFatal(messageFactory()); }
        public void LogFatal(Exception? exception, string message) => this.Log(exception, message, SonarLogLevel.Fatal);
        public void LogFatal(Exception? exception, Func<string> messageFactory) { if (this.LogFatalEnabled) this.LogFatal(exception, messageFactory()); }

        public void Verbose(string message) => this.Log(message, SonarLogLevel.Verbose);
        public void Verbose(Func<string> messageFactory) { if (this.LogVerboseEnabled) this.LogVerbose(messageFactory()); }
        public void Verbose(Exception? exception, string message) => this.Log(exception, message, SonarLogLevel.Verbose);
        public void Verbose(Exception? exception, Func<string> messageFactory) { if (this.LogVerboseEnabled) this.LogVerbose(exception, messageFactory()); }
        public void Debug(string message) => this.Log(message, SonarLogLevel.Debug);
        public void Debug(Func<string> messageFactory) { if (this.LogDebugEnabled) this.LogDebug(messageFactory()); }
        public void Debug(Exception? exception, string message) => this.Log(exception, message, SonarLogLevel.Debug);
        public void Debug(Exception? exception, Func<string> messageFactory) { if (this.LogDebugEnabled) this.LogDebug(exception, messageFactory()); }
        public void Information(string message) => this.Log(message, SonarLogLevel.Information);
        public void Information(Func<string> messageFactory) { if (this.LogInformationEnabled) this.LogInformation(messageFactory()); }
        public void Information(Exception? exception, string message) => this.Log(exception, message, SonarLogLevel.Information);
        public void Information(Exception? exception, Func<string> messageFactory) { if (this.LogInformationEnabled) this.LogInformation(exception, messageFactory()); }
        public void Warning(string message) => this.Log(message, SonarLogLevel.Warning);
        public void Warning(Func<string> messageFactory) { if (this.LogWarningEnabled) this.LogWarning(messageFactory()); }
        public void Warning(Exception? exception, string message) => this.Log(exception, message, SonarLogLevel.Warning);
        public void Warning(Exception? exception, Func<string> messageFactory) { if (this.LogWarningEnabled) this.LogWarning(exception, messageFactory()); }
        public void Error(string message) => this.Log(message, SonarLogLevel.Error);
        public void Error(Func<string> messageFactory) { if (this.LogErrorEnabled) this.LogError(messageFactory()); }
        public void Error(Exception? exception, string message) => this.Log(exception, message, SonarLogLevel.Error);
        public void Error(Exception? exception, Func<string> messageFactory) { if (this.LogErrorEnabled) this.LogError(exception, messageFactory()); }
        public void Fatal(string message) => this.Log(message, SonarLogLevel.Fatal);
        public void Fatal(Func<string> messageFactory) { if (this.LogFatalEnabled) this.LogFatal(messageFactory()); }
        public void Fatal(Exception? exception, string message) => this.Log(exception, message, SonarLogLevel.Fatal);
        public void Fatal(Exception? exception, Func<string> messageFactory) { if (this.LogFatalEnabled) this.LogFatal(exception, messageFactory()); }

        public bool LogVerboseEnabled => this.LogEnabled(SonarLogLevel.Verbose);
        public bool LogDebugEnabled => this.LogEnabled(SonarLogLevel.Debug);
        public bool LogInformationEnabled => this.LogEnabled(SonarLogLevel.Information);
        public bool LogWarningEnabled => this.LogEnabled(SonarLogLevel.Warning);
        public bool LogErrorEnabled => this.LogEnabled(SonarLogLevel.Error);
        public bool LogFatalEnabled => this.LogEnabled(SonarLogLevel.Fatal);

        public bool VerboseEnabled => this.LogEnabled(SonarLogLevel.Verbose);
        public bool DebugEnabled => this.LogEnabled(SonarLogLevel.Debug);
        public bool InformationEnabled => this.LogEnabled(SonarLogLevel.Information);
        public bool WarningEnabled => this.LogEnabled(SonarLogLevel.Warning);
        public bool ErrorEnabled => this.LogEnabled(SonarLogLevel.Error);
        public bool FatalEnabled => this.LogEnabled(SonarLogLevel.Fatal);

        public bool IsEnabled(SonarLogLevel level = SonarLogLevel.Debug) => this.LogEnabled(level);
        public bool Enabled(SonarLogLevel level = SonarLogLevel.Debug) => this.LogEnabled(level);
        #endregion

        #region Static Methods
        private static string ExtractExceptionString(Exception exception)
        {
            if (exception is AggregateException aex) exception = aex.Flatten();
            return exception.ToString() ?? string.Empty;
        }

        public static bool LogEnabled(SonarLogLevel level, SonarLogLevel minLevel) => level >= minLevel;
        #endregion
    }
}
