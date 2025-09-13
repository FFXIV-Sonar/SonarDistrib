using Dalamud.Plugin.Services;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SonarDiagnostics.Dns
{
    public sealed class DnsWorker : IDisposable, IAsyncDisposable
    {
        private static readonly ImmutableArray<string> DefaultHosts = [
            // Sonar domains
            "api.ffxivsonar.com", "assets.ffxivsonar.com", "proxy.ffxivsonar.com", "news.ffxivsonar.com",

            // Major domains
            "example.com", "microsoft.com", "bing.com", "google.com", "yahoo.com", "duckduckgo.com",

            // Git domains
            "github.com", "raw.githubusercontent.com", "gitlab.com",

            // Dalamud domains
            "kamori.goats.dev",

            // FFXIV domains
            "na.finalfantasyxiv.com", "eu.finalfantasyxiv.com", "fr.finalfantasyxiv.com", "de.finalfantasyxiv.com", "jp.finalfantasyxiv.com",
        ];

        private readonly CancellationTokenSource _cts = new();
        private readonly StringBuilder _output = new();
        private readonly Lazy<Task> _task;

        private readonly ImmutableArray<string> _hosts;

        private IPluginLog Logger { get; }

        public DnsWorker(IPluginLog logger, ImmutableArray<string>? hosts = null)
        {
            this.Logger = logger;

            this._hosts = hosts ?? DefaultHosts;
            this._task = new(this.WorkerAsync);
        }

        public DnsWorker(IPluginLog logger, IEnumerable<string> hosts) : this(logger, hosts.ToImmutableArray()) { /* Empty */ }

        public string Output => this._output.ToString();

        public Task CreateOrGetTask() => this._task.Value;

        private async Task WorkerAsync()
        {
            this._output.AppendLine("Running DNS Tests");
            foreach (var host in this._hosts)
            {
                try
                {
                    this.Log(LogEventLevel.Information, $"- Querying {host}...");
                    var addresses = await System.Net.Dns.GetHostAddressesAsync(host, this._cts.Token);
                    this.Log(LogEventLevel.Information, $" -> {string.Join(", ", addresses.Select(address => address.ToString()))}");
                }
                catch (Exception ex)
                {
                    this.Log(LogEventLevel.Error, $" -> {ex.GetType().Name} ({ex.Message})");
                }
            }
            this._output.AppendLine("DNS Tests finished!");
        }

        private void Log(LogEventLevel level, string message)
        {
            switch (level)
            {
                case LogEventLevel.Verbose:
                    this.Logger.Verbose(message);
                    this._output.AppendLine(message);
                    break;

                case LogEventLevel.Debug:
                    this.Logger.Debug(message);
                    this._output.AppendLine(message);
                    break;

                case LogEventLevel.Information:
                    this.Logger.Info(message);
                    this._output.AppendLine(message);
                    break;

                case LogEventLevel.Warning:
                    this.Logger.Warning(message);
                    this._output.AppendLine(message);
                    break;

                case LogEventLevel.Error:
                    this.Logger.Error(message);
                    this._output.AppendLine(message);
                    break;

                case LogEventLevel.Fatal:
                    this.Logger.Fatal(message);
                    this._output.AppendLine(message);
                    break;

                default:
                    this.Logger.Info(message);
                    this._output.AppendLine(message);
                    break;
            }
        }

        public void Dispose()
        {
            this._cts.Cancel();
            this._cts.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await this._cts.CancelAsync();
            this._cts.Dispose();
        }
    }
}
