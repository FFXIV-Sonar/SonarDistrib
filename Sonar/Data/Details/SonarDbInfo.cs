using MessagePack;
using Sonar.Messages;
using System;

namespace Sonar.Data.Details
{
    [MessagePackObject]
    public sealed class SonarDbInfo : ISonarMessage
    {
        [Key(0)]
        public double Timestamp { get; set; }

        [Key(1)]
        public byte[] Hash { get; set; } = default!;

        [IgnoreMember]
        public string HashString => Convert.ToBase64String(this.Hash).Replace("=", string.Empty);

        public override string ToString() => $"Sonar DB Timestamp: {this.Timestamp}, Hash: {this.HashString}";
    }
}
