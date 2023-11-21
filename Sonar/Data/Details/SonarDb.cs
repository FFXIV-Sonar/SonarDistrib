using MessagePack;
using Sonar.Data.Rows;
using System;
using System.Linq;
using System.Collections.Generic;
using Sonar.Messages;
using System.Security.Cryptography;
using System.ComponentModel;

#if NET8_0_OR_GREATER
using System.Collections.Frozen; // FrozenDictionary
#else
using System.Collections.ObjectModel; // ReadOnlyDictionary
#endif

namespace Sonar.Data.Details
{
    [MessagePackObject]
    public sealed class SonarDb : ISonarMessage
    {
        [Key(0)]
        public double Timestamp { get; set; }
        [Key(1)]
        public byte[] Hash { get; set; } = Array.Empty<byte>();
        [IgnoreMember]
        public string HashString => Convert.ToBase64String(this.Hash).Replace("=", string.Empty);

        #region Dictionaries and Lists
        [Key(2)]
        public IDictionary<uint, WorldRow> Worlds { get; set; } = new Dictionary<uint, WorldRow>();

        [Key(3)]
        public IDictionary<uint, DatacenterRow> Datacenters { get; set; } = new Dictionary<uint, DatacenterRow>();

        [Key(9)]
        public IDictionary<uint, RegionRow> Regions { get; set; } = new Dictionary<uint, RegionRow>();

        [Key(10)]
        public IDictionary<uint, AudienceRow> Audiences { get; set; } = new Dictionary<uint, AudienceRow>();

        [Key(4)]
        public IDictionary<uint, HuntRow> Hunts { get; set; } = new Dictionary<uint, HuntRow>();

        [Key(5)]
        public IDictionary<uint, FateRow> Fates { get; set; } = new Dictionary<uint, FateRow>();

        [Key(6)]
        public IDictionary<uint, MapRow> Maps { get; set; } = new Dictionary<uint, MapRow>();

        [Key(7)]
        public IDictionary<uint, ZoneRow> Zones { get; set; } = new Dictionary<uint, ZoneRow>();

        [Key(8)]
        public IDictionary<uint, WeatherRow> Weathers { get; set; } = new Dictionary<uint, WeatherRow>();
        #endregion

        /// <summary>Freeze all dictionaries</summary>
        public void Freeze()
        {
#if NET8_0_OR_GREATER
            this.Worlds = this.Worlds.ToFrozenDictionary();
            this.Datacenters = this.Datacenters.ToFrozenDictionary();
            this.Regions = this.Regions.ToFrozenDictionary();
            this.Audiences = this.Audiences.ToFrozenDictionary();
            this.Hunts = this.Hunts.ToFrozenDictionary();
            this.Fates = this.Fates.ToFrozenDictionary();
            this.Maps = this.Maps.ToFrozenDictionary();
            this.Zones = this.Zones.ToFrozenDictionary();
            this.Weathers = this.Weathers.ToFrozenDictionary();
#else
            this.Worlds = this.Worlds.AsReadOnly();
            this.Datacenters = this.Datacenters.AsReadOnly();
            this.Regions = this.Regions.AsReadOnly();
            this.Audiences = this.Audiences.AsReadOnly();
            this.Hunts = this.Hunts.AsReadOnly();
            this.Fates = this.Fates.AsReadOnly();
            this.Maps = this.Maps.AsReadOnly();
            this.Zones = this.Zones.AsReadOnly();
            this.Weathers = this.Weathers.AsReadOnly();
#endif
        }

        /// <summary>Thaw all dictionaries</summary>
        /// <remarks>To be used after using <see cref="Freeze"/> to make them modifiable</remarks>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void Thaw()
        {
            this.Worlds = this.Worlds.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            this.Datacenters = this.Datacenters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            this.Regions = this.Regions.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            this.Audiences = this.Audiences.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            this.Hunts = this.Hunts.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            this.Fates = this.Fates.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            this.Maps = this.Maps.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            this.Zones = this.Zones.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            this.Weathers = this.Weathers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>Warning: Slow</summary>
        public byte[] ComputeHash()
        {
            using var hasher = SHA256.Create();
            if (hasher is null) return Array.Empty<byte>();

            var byteList = new List<byte>(1048576);
            byteList.AddRange(MessagePackSerializer.Serialize(this.Worlds.OrderBy(kvp => kvp.Key).AsEnumerable(), MessagePackSerializerOptions.Standard));
            byteList.AddRange(MessagePackSerializer.Serialize(this.Datacenters.OrderBy(kvp => kvp.Key).AsEnumerable(), MessagePackSerializerOptions.Standard));
            byteList.AddRange(MessagePackSerializer.Serialize(this.Regions.OrderBy(kvp => kvp.Key).AsEnumerable(), MessagePackSerializerOptions.Standard));
            byteList.AddRange(MessagePackSerializer.Serialize(this.Audiences.OrderBy(kvp => kvp.Key).AsEnumerable(), MessagePackSerializerOptions.Standard));
            byteList.AddRange(MessagePackSerializer.Serialize(this.Hunts.OrderBy(kvp => kvp.Key).AsEnumerable(), MessagePackSerializerOptions.Standard));
            byteList.AddRange(MessagePackSerializer.Serialize(this.Fates.OrderBy(kvp => kvp.Key).AsEnumerable(), MessagePackSerializerOptions.Standard));
            byteList.AddRange(MessagePackSerializer.Serialize(this.Maps.OrderBy(kvp => kvp.Key).AsEnumerable(), MessagePackSerializerOptions.Standard));
            byteList.AddRange(MessagePackSerializer.Serialize(this.Zones.OrderBy(kvp => kvp.Key).AsEnumerable(), MessagePackSerializerOptions.Standard));
            byteList.AddRange(MessagePackSerializer.Serialize(this.Weathers.OrderBy(kvp => kvp.Key).AsEnumerable(), MessagePackSerializerOptions.Standard));

            var hash = new byte[32];
            if (!hasher.TryComputeHash(byteList.ToArray(), hash, out var bytesWritten)) return Array.Empty<byte>();
            return hash[..bytesWritten];
        }

        /// <summary>
        /// Warning: Slow
        /// </summary>
        public bool VerifyHash() => this.Hash.SequenceEqual(this.ComputeHash());

        public SonarDbInfo GetDbInfo() => new() { Timestamp = this.Timestamp, Hash = this.Hash };

        public override string ToString()
        {
            var lines = new List<string>(9)
            {
                $"Database Timestamp: {this.Timestamp}",
                $"Database Hash: {this.HashString}",
                $"Worlds: {this.Worlds.Count}",
                $"Datacenters: {this.Datacenters.Count}",
                $"Regions: {this.Regions.Count}",
                $"Audiences: {this.Audiences.Count}",
                $"Hunts: {this.Hunts.Count}",
                $"Fates: {this.Fates.Count}",
                $"Maps: {this.Maps.Count}",
                $"Zones: {this.Zones.Count}",
                $"Weathers: {this.Weathers.Count}",
            };
            return string.Join('\n', lines);
        }
    }
}