using Sonar.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
using Newtonsoft.Json;

namespace Sonar.Messages
{
    /// <summary>Sonar Heartbeat with some server information</summary>
    [JsonObject]
    [MessagePackObject]
    [Serializable]
    public readonly struct SonarHeartbeat : ISonarMessage // This used to be TimeSyncMessage
    {
        /// <summary>Unix Epoch time</summary>
        [Key(0)]
        public required double UnixTime { get; init; }

        /// <summary>Aggregate player place (as in where the players are) counts. Keys are index keys. Only counts >= 100 are shown.</summary>
        /// <remarks>Only counts &gt;= 100 are shown. Counts &lt; 1000 are in increments of 10.</remarks>
        [Key(1)]
        public required IReadOnlyDictionary<string, int> ClientPlaceCounts { get; init; }

        /// <summary>Aggregate player home world counts. Keys are index keys. Only counts >= 100 are shown.</summary>
        /// <remarks>Only counts &gt;= 100 are shown. Counts &lt; 1000 are in increments of 10.</remarks>
        [Key(2)]
        public required IReadOnlyDictionary<string, int> ClientHomeCounts { get; init; }
    }
}
