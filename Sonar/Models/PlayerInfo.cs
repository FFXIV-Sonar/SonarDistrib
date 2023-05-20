using MessagePack;
using Newtonsoft.Json;
using Sonar.Data.Extensions;
using Sonar.Messages;
using System;
using System.Linq;
using Sonar.Data.Rows;

namespace Sonar.Models
{
    /// <summary>
    /// Represents Player Information
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [MessagePackObject]
    [Serializable]
    public sealed class PlayerInfo : IEquatable<PlayerInfo>, ISonarMessage
    {
        public PlayerInfo(string name, uint homeWorldId)
        {
            this.Name = name;
            this.HomeWorldId = homeWorldId;
        }
        public PlayerInfo() : this("Sonar Client", 0) { }
        public PlayerInfo(string name, WorldRow homeWorld) : this(name, homeWorld.Id) { }
        public PlayerInfo(PlayerInfo info) : this(info.Name, info.HomeWorldId) { }

        /// <summary>
        /// Player Full Name
        /// </summary>
        [JsonProperty]
        [Key(0)]
        public string Name { get; init; }

        /// <summary>
        /// Player Home World ID
        /// </summary>
        [JsonProperty]
        [Key(1)]
        public uint HomeWorldId { get; init; }

        /// <summary>
        /// Helper function for First and Last name getters
        /// </summary>
        [IgnoreMember]
        private int SpaceAt => this.Name.IndexOf(" ", StringComparison.Ordinal);

        /// <summary>
        /// Player First Name
        /// </summary>
        [IgnoreMember]
        public string First
        {
            get => this.Name[..this.SpaceAt];
            //set => this.Name = $"{value} {this.Last}";
        }

        /// <summary>
        /// Player Last Name
        /// </summary>
        [IgnoreMember]
        public string Last
        {
            get => this.Name[(this.SpaceAt + 1)..];
            //set => this.Name = $"{this.First} {value}";
        }

        /// <summary>
        /// Array of valid characters
        /// </summary>
        private static readonly char[] ValidCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-'".ToArray();

        /// <summary>
        /// Check name validity based on game constraints:
        /// - Both first and last names must be between 2 and 15 characters and not total more than 20 characters combined
        /// - Only letters, hyphens and apostrophes can be used.
        /// - The first character must be a letter (upper case).
        /// - Hyphens cannot be used in succession or placed before or after apostrophes
        /// </summary>
        /// <returns>Validity of the name</returns>
        [IgnoreMember]
        public bool IsNameValid
        {
            get
            {
                return this.Name.Length <= 32; // Due to CN (and maybe KR) other properties of the name cannot be checked

                // Lengths variables
                var firstLength = this.First.Length;
                var lastLength = this.Last.Length;
                var nameLength = firstLength + lastLength + 1;

                // Ensure length is correct
                if (this.Name.Length != nameLength) return false;

                // Maximum character count is 20 (+1 for space)
                if (nameLength > 32) return false; // 21
                return true; // Cutting it off from here
            }
        }

        public override string ToString() => $"{this.Name} <{this.GetWorld()}>";

        public static bool Equals(PlayerInfo? left, PlayerInfo? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Name == right.Name && left.HomeWorldId == right.HomeWorldId;
        }

        public bool Equals(PlayerInfo? other)
        {
            return Equals(this, other);
        }
        public override bool Equals(object? other) => other is PlayerPlace place && this.Equals(place);
        public override int GetHashCode() => HashCode.Combine(this.Name.GetHashCode(), this.HomeWorldId.GetHashCode());
    }
}
