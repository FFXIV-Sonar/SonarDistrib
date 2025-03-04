using MessagePack;
using System.Text.Json.Serialization;
using Sonar.Data.Extensions;
using System;
using Sonar.Data;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SonarUtils;
using Sonar.Threading;
using Sonar.Trackers;
using Cysharp.Text;

namespace Sonar.Models
{
    /// <summary>
    /// Represent a game place (World, Zone and Instance)
    /// </summary>
    [MessagePackObject]
    [Serializable]
    public partial class GamePlace : ICloneable
    {
        private string? _placeKey;
        private bool? _valid;

        public GamePlace() { }
        public GamePlace(GamePlace p) : this(p.WorldId, p.ZoneId, p.InstanceId) { }
        public GamePlace(uint worldId, uint zoneId, uint instanceId)
        {
            this.WorldId = worldId;
            this.ZoneId = zoneId;
            this.InstanceId = instanceId;
        }

        protected virtual void ResetKeyCache() => this._placeKey = null;


        /// <summary>
        /// Place Key
        /// </summary>
        [JsonIgnore]
        [IgnoreMember]
        public string PlaceKey => this._placeKey ??= StringUtils.Intern(ZString.Format("{0}_{1}_{2}", this.WorldId, this.ZoneId, this.InstanceId));

        /// <summary>
        /// World ID
        /// </summary>
        [Key(0)]
        public uint WorldId { get; init; }

        /// <summary>
        /// Zone ID
        /// </summary>
        [Key(1)]
        public uint ZoneId { get; init; }

        /// <summary>
        /// Instance ID
        /// </summary>
        [Key(2)]
        public uint InstanceId { get; init; }

        /// <summary>
        /// Check for World, Zone and Instance IDs for equality
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool WorldZoneInstanceEquals(uint worldId, uint zoneId, uint instanceId) =>
            this.ZoneId == zoneId && this.WorldId == worldId && this.InstanceId == instanceId;

        /// <summary>
        /// Check for World, Zone and Instance IDs for equality
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool WorldZoneInstanceEquals(GamePlace place) => this.WorldZoneInstanceEquals(place.WorldId, place.ZoneId, place.InstanceId);

        /// <summary>
        /// Check for World and Zone IDs for equality
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool WorldZoneEquals(uint worldId, uint zoneId) =>
            this.ZoneId == zoneId && this.WorldId == worldId;

        /// <summary>
        /// Check for World and Zone IDs for equality
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool WorldZoneEquals(GamePlace place) => this.WorldZoneEquals(place.WorldId, place.ZoneId);

        /// <summary>
        /// Check for Zone and Instance IDs for equality
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ZoneInstanceEquals(uint zoneId, uint instanceId) =>
            this.ZoneId == zoneId && this.InstanceId == instanceId;

        /// <summary>
        /// Check for Zone and Instance IDs for equality
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ZoneInstanceEquals(GamePlace place) => this.ZoneInstanceEquals(place.ZoneId, place.InstanceId);

        /// <summary>
        /// Check for World and Instance IDs for equality
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool WorldInstanceEquals(uint worldId, uint instanceId) =>
            this.WorldId == worldId && this.InstanceId == instanceId;

        /// <summary>
        /// Check for World and Instance IDs for equality
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool WorldInstanceEquals(GamePlace place) => this.WorldInstanceEquals(place.WorldId, place.InstanceId);

        public override string ToString()
        {
            var ret = new List<string>() { $"{this.GetZone()}" };
            if (this.InstanceId > 0) ret.Add($"i{this.InstanceId}");
            ret.Add($"<{this.GetWorld()}>");
            return string.Join(' ', ret);
        }

        /// <summary>
        /// Check if the place represented is valid
        /// </summary>
        public bool IsValid() => this._valid ??= this.IsValidCore();

        private bool IsValidCore()
        {
            if (this.WorldId == 0 || this.ZoneId == 0 || !IsValidInstance(this.InstanceId)) return false; // NOTE: Instance ID 0 is a valid instance for non-instanced zones
            if (!Database.Worlds.ContainsKey(this.WorldId)) return false;
            if (!Database.Zones.ContainsKey(this.ZoneId)) return false;
            return true;
        }

        /// <summary>
        /// Check if the instance ID is valid (not checked in GamePlace.IsValid())
        /// </summary>
        public static bool IsValidInstance(uint instanceId) => instanceId >= SonarConstants.LowestInstanceId && instanceId <= SonarConstants.HighestInstanceId;

        public GamePlace Clone() => Unsafe.As<GamePlace>(this.MemberwiseClone());
        object ICloneable.Clone() => this.MemberwiseClone();
    }
}
