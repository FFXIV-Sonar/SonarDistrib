using Sonar.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
using Newtonsoft.Json;

namespace Sonar.Models
{
    [MessagePackObject]
    [JsonObject]
    public struct ServerReady : ISonarMessage
    {
        public ServerReady(uint connectionId = default)
        {
            this.ConnectionId = connectionId;
        }

        [JsonProperty]
        [Key(0)]
        public uint ConnectionId { get; set; }
    }
}
