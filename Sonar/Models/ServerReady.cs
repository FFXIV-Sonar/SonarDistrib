using Sonar.Messages;
using MessagePack;
using Sonar.Connections;
using System.Collections.Generic;
using System.Collections.Immutable;

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

        [Key(2)]
        public ImmutableArray<byte> Challenge { get; set; } = default!;
    }
}
