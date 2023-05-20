using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Indexes
{
    public sealed partial class IndexInfo
    {
        /// <summary>This can be ignored if using <see cref="GetIndexKey(IndexType)"/> or <see cref="DeriveIndexType(bool)"/></summary>
        public IndexType Type { get; init; }
        public uint? WorldId { get; init; }
        public uint? ZoneId { get; init; }
        public uint? InstanceId { get; init; }
        public uint? DatacenterId { get; init; }
        public uint? RegionId { get; init; }
        public uint? AudienceId { get; init; }

        /// <summary>Derive key type from assigned values. Ignores <see cref="Type"/></summary>
        /// <param name="noValueIsAll">Causes <see cref="IndexType.None"/> to become <see cref="IndexType.All"/> when no values are assigned</param>
        public IndexType DeriveIndexType(bool noValueIsAll = false)
        {
            var excl = 0;
            if (this.WorldId.HasValue) excl++;
            if (this.DatacenterId.HasValue) excl++;
            if (this.RegionId.HasValue) excl++;
            if (this.AudienceId.HasValue) excl++;
            if (excl > 1) throw new InvalidOperationException($"Only one of {nameof(this.WorldId)}, {nameof(this.DatacenterId)}, {nameof(this.RegionId)} and {nameof(this.AudienceId)} may be assigned");

            if (excl == 0)
            {
                if (this.ZoneId.HasValue && this.InstanceId.HasValue) return IndexType.ZoneInstance;
                if (this.ZoneId.HasValue) return IndexType.Zone;
                if (this.InstanceId.HasValue) return IndexType.Instance;
                return noValueIsAll ? IndexType.All : IndexType.None;
            }

            if (this.WorldId.HasValue)
            {
                if (this.ZoneId.HasValue && this.InstanceId.HasValue) return IndexType.WorldZoneInstance;
                if (this.ZoneId.HasValue) return IndexType.WorldZone;
                if (this.InstanceId.HasValue) return IndexType.WorldInstance;
                return IndexType.World;
            }

            if (this.DatacenterId.HasValue)
            {
                if (this.ZoneId.HasValue && this.InstanceId.HasValue) return IndexType.DatacenterZoneInstance;
                if (this.ZoneId.HasValue) return IndexType.DatacenterZone;
                if (this.InstanceId.HasValue) return IndexType.DatacenterInstance;
                return IndexType.Datacenter;
            }

            if (this.RegionId.HasValue)
            {
                if (this.ZoneId.HasValue && this.InstanceId.HasValue) return IndexType.RegionZoneInstance;
                if (this.ZoneId.HasValue) return IndexType.RegionZone;
                if (this.InstanceId.HasValue) return IndexType.RegionInstance;
                return IndexType.Region;
            }

            if (this.AudienceId.HasValue)
            {
                if (this.ZoneId.HasValue && this.InstanceId.HasValue) return IndexType.AudienceZoneInstance;
                if (this.ZoneId.HasValue) return IndexType.AudienceZone;
                if (this.InstanceId.HasValue) return IndexType.AudienceInstance;
                return IndexType.Audience;
            }

            if (Debugger.IsAttached) Debugger.Break();
            throw new InvalidOperationException($"This exception should not happen ({nameof(this.WorldId)}: {this.WorldId} | {nameof(this.ZoneId)}: {this.ZoneId} | {nameof(this.InstanceId)}: {this.InstanceId} | {nameof(this.DatacenterId)}: {this.DatacenterId} | {nameof(this.RegionId)}: {this.RegionId} | {nameof(this.AudienceId)}: {this.AudienceId}");
        }

        private static uint? ParseUintOrNull(string? s)
        {
            if (s == null) return null;
            return uint.Parse(s);
        }

        public static bool TryParse(string indexKey, [NotNullWhen(true)] out IndexInfo indexInfo)
        {
            foreach (var (type, regex) in GetRegexComparers())
            {
                var match = regex.Match(indexKey);
                if (match.Success)
                {
                    indexInfo = new IndexInfo()
                    {
                        Type = type,
                        WorldId = ParseUintOrNull(match.Groups.GetValueOrDefault("world")?.Value),
                        ZoneId = ParseUintOrNull(match.Groups.GetValueOrDefault("zone")?.Value),
                        InstanceId = ParseUintOrNull(match.Groups.GetValueOrDefault("instance")?.Value),
                        DatacenterId = ParseUintOrNull(match.Groups.GetValueOrDefault("datacenter")?.Value),
                        RegionId = ParseUintOrNull(match.Groups.GetValueOrDefault("region")?.Value),
                        AudienceId = ParseUintOrNull(match.Groups.GetValueOrDefault("audience")?.Value),
                    };
                    return true;
                }
            }
            indexInfo = default!;
            return false;
        }

        public static bool TryParse(string indexKey, IndexType type, [NotNullWhen(true)] out IndexInfo indexInfo)
        {
            var regex = GetRegexComparers()[type];
            var match = regex.Match(indexKey);
            if (match.Success)
            {
                indexInfo = new IndexInfo()
                {
                    Type = type,
                    WorldId = uint.Parse(match.Groups.GetValueOrDefault("worldId")?.Value!),
                    ZoneId = uint.Parse(match.Groups.GetValueOrDefault("zoneId")?.Value!),
                    InstanceId = uint.Parse(match.Groups.GetValueOrDefault("instanceId")?.Value!),
                    DatacenterId = uint.Parse(match.Groups.GetValueOrDefault("datacenterId")?.Value!),
                    RegionId = uint.Parse(match.Groups.GetValueOrDefault("regionId")?.Value!),
                    AudienceId = uint.Parse(match.Groups.GetValueOrDefault("audienceId")?.Value!),
                };
                return true;
            }

            indexInfo = default!;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IndexInfo Parse(string indexKey)
        {
            if (!TryParse(indexKey, out var ret))
            {
                ThrowFormatException();
            }
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IndexInfo Parse(string indexKey, IndexType type)
        {
            if (!TryParse(indexKey, type, out var ret))
            {
                ThrowFormatException();
            }
            return ret;
        }

        [DoesNotReturn] public static void ThrowFormatException() => throw new FormatException();

        /// <summary>Get index key. Can throw if information is missing.</summary>
        public string GetIndexKey() => this.GetIndexKey(this.Type);

        /// <summary>Get index key of a specified type. Can throw if information is missing.</summary>
        public string GetIndexKey(IndexType type)
        {
            return type switch
            {
                IndexType.None => IndexUtils.GetNoneIndexKey(),

                IndexType.World => IndexUtils.GetWorldIndexKey(this.WorldId!.Value),
                IndexType.WorldZone => IndexUtils.GetWorldZoneIndexKey(this.WorldId!.Value, this.ZoneId!.Value),
                IndexType.WorldZoneInstance => IndexUtils.GetWorldZoneInstanceIndexKey(this.WorldId!.Value, this.ZoneId!.Value, this.InstanceId!.Value),
                IndexType.WorldInstance => IndexUtils.GetWorldInstanceIndexKey(this.WorldId!.Value, this.InstanceId!.Value),

                IndexType.Zone => IndexUtils.GetZoneIndexKey(this.ZoneId!.Value),
                IndexType.ZoneInstance => IndexUtils.GetZoneInstanceIndexKey(this.ZoneId!.Value, this.InstanceId!.Value),
                IndexType.Instance => IndexUtils.GetInstanceIndexKey(this.InstanceId!.Value),

                IndexType.Datacenter => IndexUtils.GetDatacenterIndexKey(this.DatacenterId!.Value),
                IndexType.DatacenterZone => IndexUtils.GetDatacenterZoneIndexKey(this.DatacenterId!.Value, this.ZoneId!.Value),
                IndexType.DatacenterZoneInstance => IndexUtils.GetDatacenterZoneInstanceIndexKey(this.DatacenterId!.Value, this.ZoneId!.Value, this.InstanceId!.Value),
                IndexType.DatacenterInstance => IndexUtils.GetDatacenterInstanceIndexKey(this.DatacenterId!.Value, this.InstanceId!.Value),

                IndexType.Region => IndexUtils.GetRegionIndexKey(this.RegionId!.Value),
                IndexType.RegionZone => IndexUtils.GetRegionZoneIndexKey(this.RegionId!.Value, this.ZoneId!.Value),
                IndexType.RegionZoneInstance => IndexUtils.GetRegionZoneInstanceIndexKey(this.RegionId!.Value, this.ZoneId!.Value, this.InstanceId!.Value),
                IndexType.RegionInstance => IndexUtils.GetRegionInstanceIndexKey(this.RegionId!.Value, this.InstanceId!.Value),

                IndexType.Audience => IndexUtils.GetAudienceIndexKey(this.AudienceId!.Value),
                IndexType.AudienceZone => IndexUtils.GetAudienceZoneIndexKey(this.AudienceId!.Value, this.ZoneId!.Value),
                IndexType.AudienceZoneInstance => IndexUtils.GetAudienceZoneInstanceIndexKey(this.AudienceId!.Value, this.ZoneId!.Value, this.InstanceId!.Value),
                IndexType.AudienceInstance => IndexUtils.GetAudienceInstanceIndexKey(this.AudienceId!.Value, this.InstanceId!.Value),

                IndexType.All => IndexUtils.GetAllIndexKey(),

                _ => throw new ArgumentException("Invalid Index Type", nameof(type))
            };
        }

        public IEnumerable<string> GetIndexKeys()
        {
            var types = IndexUtils.GetIndexTypes();
            foreach (var type in types)
            {
                string? key;
                try
                {
                    key = this.GetIndexKey(type);
                }
                catch { continue; }
                yield return key;
            }
        }
    }
}
