using Microsoft.AspNetCore.SignalR.Client;
using Sonar.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Sockets
{
    public sealed partial class SonarSocketSignalR
    {
        /// <summary>
        /// Creates a <see cref="SonarSocketSignalR"/> for client-side use.
        /// </summary>
        public static SonarSocketSignalR CreateClientSocket(HubConnection connection)
        {
            return new SonarSocketSignalR(connection, 16, SonarSerializer.DeserializeServerToClient<ISonarMessage>, SonarSerializer.SerializeClientToServer<ISonarMessage>);
        }
    }
}
