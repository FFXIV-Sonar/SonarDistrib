using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace SonarUtils
{
    public static class HappyHttpUtils
    {
        private const int DelayMsTick = 400;
        private static readonly SocketsHttpHandler s_sharedHandler = CreateHttpHandler();

        public static HttpClient CreateHttpClient(bool shared = false)
        {
            return new HttpClient(shared ? s_sharedHandler : CreateHttpHandler(), !shared);
        }

        public static SocketsHttpHandler CreateHttpHandler()
        {
            return new SocketsHttpHandler()
            {
                ConnectCallback = ConnectCallbackAsync
            };
        }

        private static async ValueTask<Stream> ConnectCallbackAsync(SocketsHttpConnectionContext context, CancellationToken cancellationToken)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            using var semaphore = new SemaphoreSlim(1);

            var entries = new Queue<IPAddress>((await Dns.GetHostAddressesAsync(context.DnsEndPoint.Host, cancellationToken).ConfigureAwait(false))
                .OrderBy(entry => System.Random.Shared.Next()));

            _ = TickRunnerAsync(semaphore, cts.Token);

            var streamTasks = new List<Task<Stream>>();
            do
            {
                await semaphore.WaitAsync(cts.Token).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing); // Force processing to allow disposing
                await Task.Delay(1, cts.Token).ConfigureAwait(false);

                var streamTask = streamTasks.Find(task => task.IsCompletedSuccessfully);
                if (streamTask is not null)
                {
                    try { cts.Cancel(); } catch { /* Swallow */ }
                    foreach (var task in streamTasks.Where(task => task != streamTask && task.IsCompletedSuccessfully)) task.Result.Dispose();
                    return streamTask.Result;
                }

                lock (entries)
                {
                    if (entries.TryDequeue(out var entry))
                    {
                        streamTasks.Add(CreateStreamAsync(entry, semaphore, context, cts.Token));
                    }
                }

                await Task.Delay(1, cts.Token).ConfigureAwait(false);
            }
            while (entries.Count != 0 || streamTasks.FindIndex(task => !task.IsCompleted) != -1);
            
            cancellationToken.ThrowIfCancellationRequested();

            var exceptions = streamTasks.Where(task => task.IsFaulted && !task.IsCanceled).Select(task => task.Exception).ToArray();
            AggregateException? aex = null;
            if (exceptions.Length > 0) aex = new(exceptions!);
            throw new HappySocketException($"Unable to happily connect to {context.DnsEndPoint.Host}:{context.DnsEndPoint.Port}", aex);
        }

        private static async Task<Stream> CreateStreamAsync(IPAddress entry, SemaphoreSlim semaphore, SocketsHttpConnectionContext context, CancellationToken cancellationToken)
        {
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
            semaphore.Release();
            await socket.ConnectAsync(entry, context.DnsEndPoint.Port, cancellationToken).ConfigureAwait(false);
            return new NetworkStream(socket, true);
        }

        private static async Task TickRunnerAsync(SemaphoreSlim semaphore, CancellationToken cancellationToken)
        {
            try
            {
                while (true)
                {
                    await Task.Delay(DelayMsTick, cancellationToken).ConfigureAwait(false);
                    if (semaphore.CurrentCount == 0) semaphore.Release();
                }
            }
            catch { /* Swallow */ }
        }
    }
}
