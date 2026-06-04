using SonarUtils.Internal;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SonarUtils
{
    public static class HappyHttpUtils
    {
        private static readonly SocketsHttpHandler s_sharedHandler = CreateHttpHandler();

        [SuppressMessage("Reliability", "CA2000", Justification = "Not applicable.")]
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

        [SuppressMessage("Security", "CA5394", Justification = "Intended.")]
        public static HttpClient CreateRandomlyHappyClient(double happyChance = 0.5)
        {
            return System.Random.Shared.NextDouble() < happyChance ?
                CreateHttpClient() : new HttpClient();
        }

        [SuppressMessage("Security", "CA5394", Justification = "Intended.")]
        public static SocketsHttpHandler CreateRandomlyHappyHandler(double happyChance = 0.5)
        {
            return System.Random.Shared.NextDouble() < happyChance ?
                CreateHttpHandler() : new SocketsHttpHandler();
        }

        private static async ValueTask<Stream> ConnectCallbackAsync(SocketsHttpConnectionContext context, CancellationToken cancellationToken)
        {
            using var worker = new HappySocketWorker(context.DnsEndPoint.Host, context.DnsEndPoint.Port, TimeSpan.FromMicroseconds(400), cancellationToken);
            var socket = await worker.ConnectOrGetSocketAsync().ConfigureAwait(false);
            return new NetworkStream(socket, true);
        }

    }
}
