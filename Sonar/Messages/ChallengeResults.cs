using System.Collections.Generic;
using System.Collections.Immutable;
using MessagePack;

namespace Sonar.Messages
{
    /// <summary>Represents a challenge results.</summary>
    [MessagePackObject]
    public sealed class ChallengeResults : ISonarMessage
    {
        /// <summary>Requested challenge.</summary>
        [Key(0)]
        public required ImmutableArray<byte> Key { get; init; }

        /// <summary>Challenge answers.</summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item><b>Key:</b> File path relative to plugin root.</item>
        /// <item><b>Value:</b> HMACSHA256 Computed hash using <see cref="Key"/> as key.</item>
        /// </list>
        /// </remarks>
        [Key(1)]
        public required IReadOnlyDictionary<string, ImmutableArray<byte>> Answers { get; init; }
    }
}
