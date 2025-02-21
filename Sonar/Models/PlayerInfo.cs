using MessagePack;
using Newtonsoft.Json;
using Sonar.Data.Extensions;
using Sonar.Messages;
using System;
using AG;
using System.Threading;

namespace Sonar.Models
{
    /// <summary>Represents Player Information</summary>
    [JsonObject(MemberSerialization.OptIn)]
    [MessagePackObject]
    [Serializable]
    public sealed partial class PlayerInfo : IEquatable<PlayerInfo>, ISonarMessage
    {
        private string? _toString;
        internal int _valid; // Interlocked 0 = Unchecked, 1 = Valid, -1 = Not valid

        /// <summary>Logged in</summary>
        [JsonProperty]
        [Key(3)]
        public required bool? LoggedIn { get; init; }

        /// <summary>Player Full Name</summary>
        [JsonProperty]
        [Key(0)]
        public required string? Name { get; init; }

        /// <summary>Player Home World ID</summary>
        [JsonProperty]
        [Key(1)]
        public required uint HomeWorldId { get; init; }

        /// <summary>Player Hash 1</summary>
        [JsonProperty]
        [Key(2)]
        public required long Hash1 { get; init; }

        /// <summary>Player Hash 2</summary>
        [JsonProperty]
        [Key(4)]
        public required long Hash2 { get; init; }

        /// <summary>Lodestone ID</summary>
        internal int LodestoneId { get; set; } // Server-side only

        /// <summary>Check name validity</summary>
        /// <returns>Validity of the name</returns>
        public bool IsValid()
        {
            if (this._valid is 1) return true;
            else if (this._valid is 0)
            {
                var result = this.IsValidCore();
                if (Interlocked.CompareExchange(ref this._valid, result, 0) == 0) return result is 1;
            }
            return false;
        }

        /// <summary>Check that its logged in and valid</summary>
        public bool IsLoggedInAndValid() => this.LoggedIn is not false && this.IsValid();

        private int IsValidCore()
        {
            if (this.LoggedIn is not false)
            {
                // Due to CN (and maybe KR) other properties of the name cannot be checked
                if (this.Name is not null && this.Name.Length <= 32 && (this.GetHomeWorld()?.IsPublic ?? false)) return 1;
            }
            else if (this.Name is null && this.HomeWorldId == 0 && this.Hash1 == 0 && this.Hash2 == 0) return 1;
            return -1;
        }

        public override string ToString() => this._toString ??= $"{this.Name} <{this.GetHomeWorld()}>";
        public static bool Equals(PlayerInfo? left, PlayerInfo? right) => ReferenceEquals(left, right) || (left is not null && right is not null && left.Hash1 == right.Hash1 && left.Hash2 == right.Hash2  && left.HomeWorldId == right.HomeWorldId && string.Equals(left.Name, right.Name, StringComparison.Ordinal));
        public bool Equals(PlayerInfo? other) => Equals(this, other);
        public override bool Equals(object? obj) => obj is PlayerInfo info && this.Equals(info);
        public override int GetHashCode() => HashCode.Combine(this.Name?.GetHashCode() ?? 0, this.HomeWorldId.GetHashCode(), BitUtils.Fold(this.Hash1), BitUtils.Fold(this.Hash2));
        public static bool operator ==(PlayerInfo left, PlayerInfo right) => left.Equals(right);
        public static bool operator !=(PlayerInfo left, PlayerInfo right) => !left.Equals(right);
    }
}
