using Dalamud.Plugin.Services;
using DnsClient;
using SonarUtils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SonarDiagnostics.Dns
{
    public sealed class DnsWorker
    {
        private static (string hostname, QueryType type)[] s_testQueries =
        [
            // Sonar bootstrap queries
            ("bootstrap.ffxivsonar.com", QueryType.TXT),
            ("assets.ffxivsonar.com", QueryType.A),
            ("assets.ffxivsonar.com", QueryType.AAAA),

            // Some additional queries
            ("www.google.com", QueryType.A),
            ("www.google.com", QueryType.AAAA),
            ("github.com", QueryType.A),
            ("github.com", QueryType.AAAA),
        ];

        private readonly ImmutableList<string>.Builder _output = ImmutableList.CreateBuilder<string>();
        private bool _running;
        private ImmutableList<string>? _outputPublic;

        public ILookupClient Client { get; }
        private IPluginLog Logger { get; }
        public bool IsRunning => this._running;
        public ImmutableList<string> Output => this._outputPublic ??= this._output.ToImmutable();


        public DnsWorker(NameServer nameserver, IPluginLog logger)
        {
            this.Logger = logger;
            this.Client = DnsUtils.CreateLookupClient([nameserver]);
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            if (Interlocked.CompareExchange(ref this._running, true, false)) return;
            try
            {
                this._output.Clear();
                this._outputPublic = null;

                foreach (var (hostname, type) in s_testQueries)
                {
                    await this.TestCoreAsync(hostname, type, cancellationToken);
                }
            }
            finally
            {
                Volatile.Write(ref this._running, false);
            }
        }

        public async Task TestAsync(string hostname, QueryType type, CancellationToken cancellationToken = default)
        {
            if (Interlocked.CompareExchange(ref this._running, true, false)) return;
            try
            {
                this._output.Clear();
                this._outputPublic = null;
                await this.TestCoreAsync(hostname, type, cancellationToken);
            }
            finally
            {
                Volatile.Write(ref this._running, false);
            }
        }

        private async Task TestCoreAsync(string hostname, QueryType type, CancellationToken cancellationToken)
        {
            var nameservers = string.Join(", ", this.Client.NameServers);
            try
            {
                this.Logger.Information("Querying {hostname} {type} via {nameservers}...", hostname, type, nameservers);
                var result = await this.Client.QueryAsync(hostname, type, cancellationToken: cancellationToken);
                if (result.HasError) this.Logger.Error("Error for {hostname} {type} via {nameservers}: {message}", hostname, type, nameservers, result.ErrorMessage);

                if (result?.Answers is not null && result.Answers.Count > 0)
                {
                    var answers = string.Join(", ", result.Answers);
                    this.Logger.Information("Answers for {hostname} {type} via {nameservers}: {answers}", hostname, type, nameservers, answers);
                    this._output.Add(answers);
                }
                else
                {
                    this._output.Add($"{hostname} {type} => {(result is null ? "NULL" : result.HasError ? "ERROR" : "Unresolved")}");
                }
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, "Exception during query for {hostname} {type} via {nameservers}", hostname, type, nameservers);
                this._output.Add($"{hostname} {type} => EXCEPTION");
            }
            this._outputPublic = null;
        }
    }
}
