using MessagePack;
using System;

namespace Sonar.Messages
{
    /// <summary>Ping Response</summary>
    [MessagePackObject]
    [Serializable]
    public sealed class SonarPong : ISonarMessage
    {
        /// <summary>Sequence ID</summary>
        [Key(0)]
        public required uint Sequence { get; init; }
    }
}
