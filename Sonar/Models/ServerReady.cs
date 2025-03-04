using Sonar.Messages;
using MessagePack;
using Sonar.Connections;

namespace Sonar.Models
{
    [MessagePackObject]
    public sealed class ServerReady : ISonarMessage
    {
        public ServerReady(uint connectionId = default, ConnectionType connectionType = ConnectionType.Unknown)
        {
            this.ConnectionId = connectionId;
            this.ConnectionType = connectionType;
        }

        [Key(0)]
        public uint ConnectionId { get; set; }

        [Key(1)]
        public ConnectionType ConnectionType { get; set; }
    }
}
