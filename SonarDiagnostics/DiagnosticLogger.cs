using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using DryIocAttributes;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SonarDiagnostics
{
    public sealed class DiagnosticLogger : IPluginLog, IDisposable
    {
        private readonly SemaphoreSlim _semaphore = new(1);
        private readonly LinkedList<string> _log = new();
        private StreamWriter? _fileOutput;

        public IPluginLog Parent { get; }

        /// <inheritdoc/>
        public ILogger Logger => this.Parent.Logger;

        public string? LogPath { get; }

        public DiagnosticLogger(IPluginLog parent, IDalamudPluginInterface pluginInterface)
        {
            this.Parent = parent;
            this.Info($"{nameof(DiagnosticLogger)} constructed");

            var configDir = pluginInterface.ConfigDirectory;
            try
            {
                var path = Path.Join(configDir.FullName, $"diagnostic-{DateTimeOffset.UtcNow:yyyyMMdd}.log");
                this.LogPath = path;

                var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                stream.Seek(0, SeekOrigin.End);

                this._fileOutput = new(stream, encoding: Encoding.UTF8, leaveOpen: false);
                this._fileOutput.AutoFlush = true;
            }
            catch (Exception ex)
            {
                this.Parent.Error(ex, "Exception creating file stream");
            }
        }

        /// <inheritdoc/>
        public LogEventLevel MinimumLogLevel
        {
            get => this.Parent.MinimumLogLevel;
            set => this.Parent.MinimumLogLevel = value;
        }

        /// <inheritdoc/>
        public void Write(LogEventLevel level, Exception? exception, string messageTemplate, params object[] values)
        {
            if (level < this.MinimumLogLevel) return;
            this.Parent.Write(level, exception, messageTemplate, values);

            this._semaphore.Wait();
            try
            {
                var timestamp = DateTimeOffset.UtcNow;
                if (!this.Logger.BindMessageTemplate(messageTemplate, values, out var parsedTemplate, out var boundProperties)) return;
                var logEvent = new LogEvent(timestamp, level, exception, parsedTemplate, boundProperties);
                var output = $"{timestamp:u} [{level}]: {logEvent.RenderMessage()}";
                this._log.AddLast(output);
                this._fileOutput?.WriteLine(output);
            }
            catch (Exception ex)
            {
                this.Parent.Error(ex, "Logging to file failed");
            }
            finally
            {
                this._semaphore.Release();
            }
        }


        /// <inheritdoc/>
        public void Verbose(string messageTemplate, params object[] values) => this.Write(LogEventLevel.Verbose, null, messageTemplate, values);

        /// <inheritdoc/>
        public void Verbose(Exception? exception, string messageTemplate, params object[] values) => this.Write(LogEventLevel.Verbose, exception, messageTemplate, values);

        /// <inheritdoc/>
        public void Debug(string messageTemplate, params object[] values) => this.Write(LogEventLevel.Debug, null, messageTemplate, values);

        /// <inheritdoc/>
        public void Debug(Exception? exception, string messageTemplate, params object[] values) => this.Write(LogEventLevel.Debug, exception, messageTemplate, values);

        /// <inheritdoc/>
        public void Info(string messageTemplate, params object[] values) => this.Write(LogEventLevel.Information, null, messageTemplate, values);

        /// <inheritdoc/>
        public void Info(Exception? exception, string messageTemplate, params object[] values) => this.Write(LogEventLevel.Information, exception, messageTemplate, values);

        /// <inheritdoc/>
        public void Information(string messageTemplate, params object[] values) => this.Write(LogEventLevel.Information, null, messageTemplate, values);

        /// <inheritdoc/>
        public void Information(Exception? exception, string messageTemplate, params object[] values) => this.Write(LogEventLevel.Information, exception, messageTemplate, values);

        /// <inheritdoc/>
        public void Warning(string messageTemplate, params object[] values) => this.Write(LogEventLevel.Warning, null, messageTemplate, values);

        /// <inheritdoc/>
        public void Warning(Exception? exception, string messageTemplate, params object[] values) => this.Write(LogEventLevel.Warning, exception, messageTemplate, values);

        /// <inheritdoc/>
        public void Error(string messageTemplate, params object[] values) => this.Write(LogEventLevel.Error, null, messageTemplate, values);

        /// <inheritdoc/>
        public void Error(Exception? exception, string messageTemplate, params object[] values) => this.Write(LogEventLevel.Error, exception, messageTemplate, values);

        /// <inheritdoc/>
        public void Fatal(string messageTemplate, params object[] values) => this.Write(LogEventLevel.Fatal, null, messageTemplate, values);

        /// <inheritdoc/>
        public void Fatal(Exception? exception, string messageTemplate, params object[] values) => this.Write(LogEventLevel.Fatal, exception, messageTemplate, values);

        public void Dispose()
        {
            this._semaphore.Wait();
            try
            {
                var fileOutput = this._fileOutput;
                if (fileOutput is not null && Interlocked.CompareExchange(ref this._fileOutput, null, fileOutput) == fileOutput)
                {
                    fileOutput.Dispose();
                }
            }
            finally
            {
                this._semaphore.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            await this._semaphore.WaitAsync();
            try
            {
                var fileOutput = this._fileOutput;
                if (fileOutput is not null && Interlocked.CompareExchange(ref this._fileOutput, null, fileOutput) == fileOutput)
                {
                    await fileOutput.DisposeAsync();
                }
            }
            finally
            {
                this._semaphore.Release();
            }
        }
    }
}
