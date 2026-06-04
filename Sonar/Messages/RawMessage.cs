using MessagePack;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Sonar.Messages
{
    [MessagePackObject]
    public sealed class RawMessage : ISonarMessage
    {
        [SetsRequiredMembers]
        public RawMessage(ImmutableArray<byte> bytes)
        {
            this.Bytes = bytes;
        }

        [SetsRequiredMembers]
        public RawMessage(ISonarMessage message) : this(ImmutableCollectionsMarshal.AsImmutableArray(SonarSerializer.SerializeClientToServer(message))) { /* Empty */ }

        /// <summary>Use <see cref="SonarSerializer.SerializeClientToServer{T}(T)"/> with <see cref="ISonarMessage"/>.</summary>
        [Key(0)]
        public required ImmutableArray<byte> Bytes { get; init; }

        public ISonarMessage Deserialize() => SonarSerializer.DeserializeClientToServer<ISonarMessage>(ImmutableCollectionsMarshal.AsArray(this.Bytes)!);
    }
}
