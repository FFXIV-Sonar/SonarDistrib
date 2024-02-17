using Sonar.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
using Newtonsoft.Json;
using Sonar.Connections;

namespace Sonar.Models
{
    [MessagePackObject]
    [JsonObject]
    public struct ServerReady : ISonarMessage
    {
        public ServerReady(uint connectionId = default, ConnectionType connectionType = ConnectionType.Unknown)
        {
            this.ConnectionId = connectionId;
            this.ConnectionType = connectionType;
        }

        [JsonProperty]
        [Key(0)]
        public uint ConnectionId { get; set; }

        [JsonProperty]
        [Key(1)]
        public ConnectionType ConnectionType { get; set; }
    }
}
