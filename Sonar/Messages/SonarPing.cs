using MessagePack;
using Newtonsoft.Json;
using System;

namespace Sonar.Messages
{
    /// <summary>Ping Request</summary>
    [JsonObject]
    [MessagePackObject]
    [Serializable]
    public readonly struct SonarPing : ISonarMessage
    {
        /// <summary>Sequence ID</summary>
        [Key(0)]
        public required uint Sequence { get; init; }
    }
}
