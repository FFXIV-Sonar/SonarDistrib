using MessagePack;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Windows.Markup;

namespace Sonar.Messages
{
    /// <summary>Sonar token.</summary>
    [MessagePackObject]
    public sealed class SonarToken : ISonarMessage
    {
        /// <summary>Gets an <see cref="ImmutableArray{byte}"/> represantation of this <see cref="SonarToken"/>.</summary>
        /// <remarks>Use <see cref="ToString"/> for textual representation.</remarks>
        [Key(0)]
        [JsonIgnore]
        public ImmutableArray<byte> Bytes { get; init; }

        /// <summary>Gets a <see cref="string"/> representation of this <see cref="SonarToken"/>.</summary>
        /// <remarks>This is a Base64Url representation of <see cref="Bytes"/>.</remarks>
        public override string ToString() => UrlBase64.Encode(this.Bytes.AsSpan());
    }
}
