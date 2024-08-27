using MessagePack;
using Newtonsoft.Json;
using Sonar.Data.Extensions;
using Sonar.Messages;
using System;
using System.Linq;
using Sonar.Data.Rows;
using Sonar.Data;

namespace Sonar.Models
{
    /// <summary>Represents Player Information</summary>
    [JsonObject(MemberSerialization.OptIn)]
    [MessagePackObject]
    [Serializable]
    public sealed partial class PlayerInfo : IEquatable<PlayerInfo>, ISonarMessage
    {
        private bool? _valid;

        /// <summary>Player Full Name</summary>
        [JsonProperty]
        [Key(0)]
        public required string Name { get; init; }

        /// <summary>Player Home World ID</summary>
        [JsonProperty]
        [Key(1)]
        public required uint HomeWorldId { get; init; }

        /// <summary>Player Hash</summary>
        [JsonProperty]
        [Key(2)]
        public required long Hash { get; init; }

        /// <summary>Check name validity</summary>
        /// <returns>Validity of the name</returns>
        public bool IsValid() => this._valid ??= this.Name.Length <= 32 && (this.GetWorld()?.IsPublic ?? false); // Due to CN (and maybe KR) other properties of the name cannot be checked

        public override string ToString() => $"{this.Name} <{this.GetWorld()}>";

        public static bool Equals(PlayerInfo? left, PlayerInfo? right) => ReferenceEquals(left, right) || (left is not null && right is not null && left.HomeWorldId == right.HomeWorldId && string.Equals(left.Name, right.Name, StringComparison.Ordinal));
        public bool Equals(PlayerInfo? other) => Equals(this, other);
        public override bool Equals(object? obj) => obj is PlayerPosition place && this.Equals(place);
        public override int GetHashCode() => HashCode.Combine(this.Name.GetHashCode(), this.HomeWorldId.GetHashCode());
        public static bool operator ==(PlayerInfo left, PlayerInfo right) => left.Equals(right);
        public static bool operator !=(PlayerInfo left, PlayerInfo right) => !left.Equals(right);
    }
}
