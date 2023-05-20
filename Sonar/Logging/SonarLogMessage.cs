using MessagePack;
using Newtonsoft.Json;
using Sonar.Enums;
using Sonar.Logging;
using Sonar.Messages;
using System;

namespace Sonar.Logging
{
    /// <summary>Sonar log message</summary>
    [JsonObject]
    [MessagePackObject]
    [Serializable]
    public readonly struct SonarLogMessage : ISonarMessage
    {
        [Key(0)]
        public SonarLogLevel Level { get; init; }

        [Key(1)]
        public string Message { get; init; }

        public override string ToString() => $"{this.Level}|{this.Message}";
    }
}
