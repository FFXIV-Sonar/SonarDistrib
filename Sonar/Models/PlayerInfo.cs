using MessagePack;
using Newtonsoft.Json;
using Sonar.Data.Extensions;
using Sonar.Messages;
using System;
using System.Linq;
using Sonar.Data.Rows;
using Sonar.Data;
using AG;
using System.Threading;
using System.Text;

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
        public required bool LoggedIn { get; init; }

        /// <summary>Player Full Name</summary>
        [JsonProperty]
        [Key(0)]
        public required string? Name { get; init; }

        /// <summary>Player Home World ID</summary>
        [JsonProperty]
        [Key(1)]
        public required uint HomeWorldId { get; init; }

        /// <summary>Player Hash</summary>
        [JsonProperty]
        [Key(2)]
        public required long Hash { get; init; }

        /// <summary>Lodestone ID</summary>
        [IgnoreMember]
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
        public bool IsLoggedInAndValid() => this.LoggedIn && this.IsValid();

        private int IsValidCore()
        {
            if (this.LoggedIn)
            {
                // Due to CN (and maybe KR) other properties of the name cannot be checked
                if (this.Name is not null && this.Name.Length <= 32 && (this.GetWorld()?.IsPublic ?? false)) return 1;
            }
            else if (this.Name is null && this.HomeWorldId == 0 && this.Hash == 0) return 1;
            return -1;
        }

        public override string ToString() => this._toString ??= $"{this.Name} <{this.GetWorld()}>";
        public static bool Equals(PlayerInfo? left, PlayerInfo? right) => ReferenceEquals(left, right) || (left is not null && right is not null && left.Hash == right.Hash && left.HomeWorldId == right.HomeWorldId && string.Equals(left.Name, right.Name, StringComparison.Ordinal));
        public bool Equals(PlayerInfo? other) => Equals(this, other);
        public override bool Equals(object? obj) => obj is PlayerInfo info && this.Equals(info);
        public override int GetHashCode() => HashCode.Combine(this.Name?.GetHashCode() ?? 0, this.HomeWorldId.GetHashCode(), SplitHash64.ConvertTo32Bit(this.Hash));
        public static bool operator ==(PlayerInfo left, PlayerInfo right) => left.Equals(right);
        public static bool operator !=(PlayerInfo left, PlayerInfo right) => !left.Equals(right);
    }
}
