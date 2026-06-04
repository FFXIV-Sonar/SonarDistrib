using MessagePack;
using Sonar.Messages;
using System.Buffers.Text;
using System.Collections.Immutable;

namespace Sonar.Data.Details
{
    [MessagePackObject]
    public sealed class SonarDbInfo : ISonarMessage
    {
        [Key(0)]
        public double Timestamp { get; init; }

        [Key(1)]
        public ImmutableArray<byte> Hash { get; init; }

        [IgnoreMember]
        public string HashString => Base64Url.EncodeToString(this.Hash.AsSpan());

        public override string ToString() => $"Sonar DB Timestamp: {this.Timestamp}, Hash: {this.HashString}";
    }
}
