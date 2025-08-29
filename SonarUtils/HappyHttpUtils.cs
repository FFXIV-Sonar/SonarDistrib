using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System;
using SonarUtils.Internal;

namespace SonarUtils
{
    public static class HappyHttpUtils
    {
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

        public static HttpClient CreateRandomlyHappyClient(double happyChance = 0.5)
        {
            return System.Random.Shared.NextDouble() < happyChance ?
                CreateHttpClient() : new HttpClient();
        }

        public static SocketsHttpHandler CreateRandomlyHappyHandler(double happyChance = 0.5)
        {
            return System.Random.Shared.NextDouble() < happyChance ?
                CreateHttpHandler() : new SocketsHttpHandler();
        }

        private static async ValueTask<Stream> ConnectCallbackAsync(SocketsHttpConnectionContext context, CancellationToken cancellationToken)
        {
            using var worker = new HappySocketWorker(context.DnsEndPoint.Host, context.DnsEndPoint.Port, TimeSpan.FromMicroseconds(400), cancellationToken);
            var socket = await worker.ConnectOrGetSocketAsync();
            return new NetworkStream(socket, true);
        }

    }
}
