using Sonar.Messages;
using System.Net.WebSockets;

namespace Sonar.Sockets
{
    public sealed partial class SonarSocketWebSocket
    {
        /// <summary>
        /// Creates a <see cref="SonarSocketWebSocket"/> for client-side use.
        /// </summary>
        public static SonarSocketWebSocket CreateClientSocket(WebSocket webSocket)
        {
            return new SonarSocketWebSocket(webSocket, 4096, 67108864, 16, SonarSerializer.DeserializeServerToClient<ISonarMessage>, SonarSerializer.SerializeClientToServer<ISonarMessage>);
        }

        /// <summary>
        /// Creates a <see cref="SonarSocketWebSocket"/> for server-side use.
        /// </summary>
        public static SonarSocketWebSocket CreateServerSocket(WebSocket webSocket)
        {
            return new SonarSocketWebSocket(webSocket, 1024, 1048576, 16, SonarSerializer.DeserializeClientToServer<ISonarMessage>, SonarSerializer.SerializeServerToClient<ISonarMessage>);
        }

        /// <summary>
        /// Creates a <see cref="SonarSocketWebSocket"/> for server-peer use (both sides).
        /// </summary>
        public static SonarSocketWebSocket CreatePeerSocket(WebSocket webSocket)
        {
            return new SonarSocketWebSocket(webSocket, 4096, 16777216, 16, SonarSerializer.DeserializeClientToServer<ISonarMessage>, SonarSerializer.SerializeClientToServer<ISonarMessage>);
        }
    }
}
