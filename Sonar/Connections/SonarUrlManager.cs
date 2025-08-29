using DnsClient;
using SonarUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Sonar.Connections
{
    /// <summary>Manages Sonar connection URLs</summary>
    internal sealed partial class SonarUrlManager : IDisposable, IAsyncDisposable
    {
        private readonly Task _bootstrapTask;
        private readonly CancellationTokenSource _cts = new();
        private readonly NonBlocking.NonBlockingHashSet<SonarUrl> _urls = [];
        private SonarUrl[]? _urlArray;

        private SonarUrl[] GetSonarUrls() => this._urlArray ??= this._urls.ToArray();

        public SonarUrlManager()
        {
            this.ReadEmbeddedUrls();
            this._bootstrapTask = Task.WhenAll(this.BootstrapDnsTask(this._cts.Token), this.BootstrapWebTask(this._cts.Token));
        }

        /// <param name="proxy">Allow proxy URLs</param>
        /// <param name="reconnect">Allow reconnect only restricted URLs, for reconnects only</param>
        /// <param name="retries">Number of retries to return a URL with specified properties. Should this happen a completely random URL is returned instead. (You must really have bad luck for this to happen)</param>
        public SonarUrl GetRandomUrl(bool proxy = false, bool reconnect = false, int retries = 255)
        {
            var urls = this.GetSonarUrls();
            var random = SonarStatic.Random;
            var loop = 0;
            while (true)
            {
                var url = urls[random.Next(urls.Length)];
                if ((loop++ < retries) && ((url.Proxy && !proxy) || (url.ReconnectOnly && !reconnect))) continue;
                return url;
            }
        }

        /// <summary>Read URLs from embedded resource</summary>
        private void ReadEmbeddedUrls()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("Sonar.Resources.Urls.data")
                ?? throw new FileNotFoundException($"Couldn't read url resources");

            var bytes = new byte[stream.Length];
            stream.ReadExactly(bytes, 0, bytes.Length);
            this.ProcessBytes(bytes);
        }

        /// <summary>Read URLs from TXT DNS query</summary>
        // [SuppressMessage("Minor Code Smell", "S1075", Justification = "Well known dns entry")]
        private async Task BootstrapDnsTask(CancellationToken cancellationToken)
        {
            await Task.Yield();
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var response = await DnsUtils.QueryAsync("bootstrap.ffxivsonar.com", QueryType.TXT, cancellationToken: cancellationToken);
                    var records = response.AllRecords.TxtRecords();
                    foreach (var record in records)
                    {
                        try
                        {
                            this.ProcessBytes(Convert.FromBase64String(string.Join(string.Empty, record.Text)));
                        }
                        catch { /* Swallow */ }
                    }
                    await Task.Delay(TimeSpan.FromHours(6 * (SonarStatic.Random.NextDouble() + 0.5)), cancellationToken);
                }
                catch (OperationCanceledException) { return; }
                catch (ObjectDisposedException) { return; }
                catch
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromMinutes(6 * (SonarStatic.Random.NextDouble() + 0.5)), cancellationToken);
                    }
                    catch (OperationCanceledException) { return; }
                    catch (ObjectDisposedException) { return; }
                }
            }
        }

        /// <summary>Read URLs from Sonar assets server</summary>
        [SuppressMessage("Minor Code Smell", "S1075", Justification = "Well known url")]
        private async Task BootstrapWebTask(CancellationToken cancellationToken)
        {
            await Task.Yield();
            while (!cancellationToken.IsCancellationRequested)
            {
                using var httpClient = HappyHttpUtils.CreateRandomlyHappyClient();
                try
                {
                    var response = await httpClient.GetAsync("https://assets.ffxivsonar.com/bootstrap/Urls.data", cancellationToken);
                    response.EnsureSuccessStatusCode();
                    this.ProcessBytes(await response.Content.ReadAsByteArrayAsync(cancellationToken));
                    await Task.Delay(TimeSpan.FromHours(6 * (SonarStatic.Random.NextDouble() + 0.5)), cancellationToken);
                }
                catch (OperationCanceledException) { return; }
                catch (ObjectDisposedException) { return; }
                catch
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromMinutes(6 * (SonarStatic.Random.NextDouble() + 0.5)), cancellationToken);
                    }
                    catch (OperationCanceledException) { return; }
                    catch (ObjectDisposedException) { return; }
                }
            }
        }

        private void ProcessBytes(byte[] bytes)
        {
            var urls = DeserializeUrls(bytes);
            foreach (var url in urls.Where(u => u.Debug == SonarConstants.DebugBuild))
            {
                this._urls.Remove(url); 
                if (url.Enabled) this._urls.Add(url);
            }
            this._urlArray = null;
        }

        public void Dispose()
        {
            this._cts.Cancel();
            this._cts.Dispose();
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await this._cts.CancelAsync();
            await this._bootstrapTask;
            this._cts.Dispose();
            GC.SuppressFinalize(this);
        }

        ~SonarUrlManager() => this.Dispose();
    }
}
