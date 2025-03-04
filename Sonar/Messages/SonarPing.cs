using MessagePack;
using System;

namespace Sonar.Messages
{
    /// <summary>Ping Request</summary>
    [MessagePackObject]
    [Serializable]
    public sealed class SonarPing : ISonarMessage
    {
        /// <summary>Sequence ID</summary>
        [Key(0)]
        public required uint Sequence { get; init; }
    }
}
