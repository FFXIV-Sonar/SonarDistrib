using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SonarUtils.Internal
{
    /// <summary>Happy Eyeballs socket worker.</summary>
    public sealed class HappySocketWorker : IDisposable
    {
        private readonly string _host;
        private readonly int _port;
        private readonly TimeSpan _attemptDelay;
        private readonly CancellationTokenSource _cts;

        private readonly Channel<IPAddress> _ipAddresses = Channel.CreateUnbounded<IPAddress>(new UnboundedChannelOptions() { AllowSynchronousContinuations = true, SingleReader = true });
        private readonly TaskCompletionSource<Socket> _tcs = new();
        private readonly List<Task> _tasks = [];

        /// <summary>Initializes a <see cref="HappySocketWorker"/> that connects to a specified <paramref name="host"/> at port <paramref name="port"/>.</summary>
        /// <param name="host">Host.</param>
        /// <param name="port">Port.</param>
        /// <param name="attemptDelay">Delay between each connection attempt.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public HappySocketWorker(string host, int port, TimeSpan attemptDelay = default, CancellationToken cancellationToken = default)
        {
            this._host = host; this._port = port; this._attemptDelay = attemptDelay;
            this._cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        }

        /// <summary>Initiates the connectivity process if not started and gets the connected <see cref="Socket"/> asynchronously.</summary>
        /// <remarks><see cref="CancellationToken"/> is passed during this <see cref="HappySocketWorker"/>'s construction.</remarks>
        /// <returns>Connected <see cref="Socket"/>.</returns>
        public Task<Socket> ConnectOrGetSocketAsync()
        {
            // If no tasks have started yet lets spool them up
            if (this._tasks.Count is 0)
            {
                // This lousy List<Task> is not synchronized, lets lock on it.
                lock (this._tasks)
                {
                    // Double-checked pattern
                    if (this._tasks.Count is 0)
                    {
                        // DNS queries
                        var dnsTasks = new List<Task>();
                        dnsTasks.Add(Task.Run(() => this.DnsWorkerAsync(this._cts.Token))); // .NET built-in resolver
                        this._tasks.AddRange(dnsTasks);

                        // DNS watcher
                        _ = Task.Run(async () =>
                        {
                            await Task.WhenAll(dnsTasks).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
                            this._ipAddresses.Writer.TryComplete();
                        });

                        // Connection loop
                        this._tasks.Add(Task.Run(() => this.ConnectLoopAsync(this._cts.Token)));

                        // Tasks watcher
                        _ = Task.Run(this.TaskWatcher);
                    }
                }
            }

            // Return awaitable task
            return this._tcs.Task;
        }

        /// <summary>Watches all tasks for completion. Sets an exception if all finishes but without result.</summary>
        private async Task TaskWatcher()
        {
            // Local copy of this._tasks
            var tasks = new List<Task>();

            // Await all tasks recursively
            while (true)
            {
                lock (this._tasks) tasks.AddRange(this._tasks);
                if (tasks.All(task => task.IsCompleted)) break;
                await Task.WhenAll(tasks).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
                tasks.Clear();
            }
            Debug.Assert(tasks.Count > 0 && tasks.All(task => task.IsCompleted));

            // If the task was never completed, lets throw an exception
            // AggregateException of AggregateExceptions casted as Exceptions!
            if (!this._tcs.Task.IsCompleted)
            {
                // There's a possibility of throwing an AggregateException with no inner exception. This should never happen.
                this._tcs.TrySetException(new AggregateException(tasks.Where(task => task.IsFaulted).Select(task => task.Exception).Cast<Exception>()));
            }
            else
            {
                // Observe exceptions. This prevent them from being dispatched as UnobservedTaskException events.
                foreach (var task in tasks.Where(task => task.IsFaulted)) _ = task.Exception;
            }
        }

        /// <summary>Connects to host IPs returned from the DNS tasks.</summary>
        private async Task ConnectLoopAsync(CancellationToken cancellationToken)
        {
            var reader = this._ipAddresses.Reader;
            await foreach (var address in reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
            {
                // Start and addsa new connect worker to this._tasks.
                lock (this._tasks) this._tasks.Add(Task.Run(() => this.ConnectWorkerAsync(address, cancellationToken), cancellationToken));

                // Wait this._attemptDelay before awaiting and attempting another connection.
                await Task.Delay(this._attemptDelay, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task DnsWorkerAsync(CancellationToken cancellationToken)
        {
            var writer = this._ipAddresses.Writer;
            if (IPAddress.TryParse(this._host, out var ipAddress))
            {
                await writer.WriteAsync(ipAddress, cancellationToken).ConfigureAwait(false);
                return;
            }
            
            var addresses = await Dns.GetHostAddressesAsync(this._host, cancellationToken);
            foreach (var address in addresses) await writer.WriteAsync(address, cancellationToken);
        }

        /// <summary>Attempts a connection to an IP <paramref name="address"/>.</summary>
        private async Task ConnectWorkerAsync(IPAddress address, CancellationToken cancellationToken)
        {
            var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
            try
            {
                await socket.ConnectAsync(address, this._port, cancellationToken).ConfigureAwait(false);
                this._tcs.TrySetResult(socket);
                await this._cts.CancelAsync().ConfigureAwait(false);
            }
            finally
            {
                // Dispose of the socket if not returned.
                if (!this._tcs.Task.IsCompletedSuccessfully || this._tcs.Task.Result != socket) socket.Dispose();
            }
        }

        public void Dispose()
        {
            try { this._cts.Cancel(); } catch { /* Swallow */ }
            this._cts.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            try { await this._cts.CancelAsync().ConfigureAwait(false); } catch { /* Swalllow */ }
            this._cts.Dispose();
        }
    }
}
