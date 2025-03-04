using MessagePack;
using Sonar.Enums;
using Sonar.Logging;
using Sonar.Messages;
using System;

namespace Sonar.Logging
{
    /// <summary>Sonar log message</summary>
    [MessagePackObject]
    [Serializable]
    public sealed class SonarLogMessage : ISonarMessage
    {
        [Key(0)]
        public SonarLogLevel Level { get; init; }

        [Key(1)]
        public required string Message { get; init; }

        public override string ToString() => $"{this.Level}|{this.Message}";
    }
}
