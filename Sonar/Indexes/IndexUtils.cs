using Sonar.Data;
using Sonar.Data.Extensions;
using Sonar.Data.Rows;
using Sonar.Enums;
using Sonar.Models;
using Sonar.Utilities;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sonar.Indexes
{
    public static partial class IndexUtils
    {
        private static IEnumerable<IndexType>? s_indexTypes;

        [GeneratedRegex(@"^none$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase)]
        private static partial Regex GetNoneRegex();

        [GeneratedRegex(@"^(?<worldId>\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase)]
        private static partial Regex GetWorldRegex();

        [GeneratedRegex(@"^(?<worldId>\d+)_(?<zoneId>\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase)]
        private static partial Regex GetWorldZoneRegex();

        [GeneratedRegex(@"^(?<worldId>\d+)_(?<zoneId>\d+)_(?<instanceId>\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase)]
        private static partial Regex GetWorldZoneInstanceRegex();

        [GeneratedRegex(@"^wi(?<worldId>\d+)_(?<instanceId>\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase)]
        private static partial Regex GetWorldInstanceRegex();

        [GeneratedRegex(@"^z(?<zoneId>\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase)]
        private static partial Regex GetZoneRegex();

        [GeneratedRegex(@"^z(?<zoneId>\d+)_(?<instanceId>\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase)]
        private static partial Regex GetZoneInstanceRegex();

        [GeneratedRegex(@"^i(?<instanceId>\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase)]
        private static partial Regex GetInstanceRegex();

        [GeneratedRegex(@"^d(?<datacenterId>\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase)]
        private static partial Regex GetDatacenterRegex();

        [GeneratedRegex(@"^d(?<datacenterId>\d+)_(?<zoneId>\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase)]
        private static partial Regex GetDatacenterZoneRegex();

        [GeneratedRegex(@"^d(?<datacenterId>\d+)_(?<zoneId>\d+)_(?<instanceId>\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase)]
        private static partial Regex GetDatacenterZoneInstanceRegex();

        [GeneratedRegex(@"^di(?<datacenterId>\d+)_(?<instanceId>\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase)]
        private static partial Regex GetDatacenterInstanceRegex();

        [GeneratedRegex(@"^r(?<regionId>\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase)]
        private static partial Regex GetRegionRegex();

        [GeneratedRegex(@"^r(?<regionId>\d+)_(?<zoneId>\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase)]
        private static partial Regex GetRegionZoneRegex();

        [GeneratedRegex(@"^r(?<regionId>\d+)_(?<zoneId>\d+)_(?<instanceId>\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase)]
        private static partial Regex GetRegionZoneInstanceRegex();

        [GeneratedRegex(@"^ri(?<regionId>\d+)_(?<instanceId>\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase)]
        private static partial Regex GetRegionInstanceRegex();

        [GeneratedRegex(@"^a(?<audienceId>\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase)]
        private static partial Regex GetAudienceRegex();

        [GeneratedRegex(@"^a(?<audienceId>\d+)_(?<zoneId>\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase)]
        private static partial Regex GetAudienceZoneRegex();

        [GeneratedRegex(@"^a(?<audienceId>\d+)_(?<zoneId>\d+)_(?<instanceId>\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase)]
        private static partial Regex GetAudienceZoneInstanceRegex();

        [GeneratedRegex(@"^ai(?<audienceId>\d+)_(?<instanceId>\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase)]
        private static partial Regex GetAudienceInstanceRegex();

        [GeneratedRegex(@"^all$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase)]
        private static partial Regex GetAllRegex();

        /// <summary>Regex for parsing <c>"none"</c></summary>
        public static Regex NoneRegex => GetNoneRegex();

        /// <summary>Regex for parsing World ID indexes</summary>
        public static Regex WorldRegex => GetWorldRegex();

        /// <summary>Regex for parsing World and Zone ID indexes</summary>
        public static Regex WorldZoneRegex => GetWorldZoneRegex();

        /// <summary>Regex for parsing World, Zone and Instance ID indexes</summary>
        public static Regex WorldZoneInstanceRegex => GetWorldZoneInstanceRegex();

        /// <summary>Regex for parsing World and Instance ID indexes</summary>
        public static Regex WorldInstanceRegex => GetWorldInstanceRegex();

        /// <summary>Regex for parsing Zone ID indexes</summary>
        public static Regex ZoneRegex => GetZoneRegex();

        /// <summary>Regex for parsing Zone and Instance ID indexes</summary>
        public static Regex ZoneInstanceRegex => GetZoneInstanceRegex();

        /// <summary>Regex for parsing Instance ID indexes</summary>
        public static Regex InstanceRegex => GetInstanceRegex();

        /// <summary>Regex for parsing Datacenter ID indexes</summary>
        public static Regex DatacenterRegex => GetDatacenterRegex();

        /// <summary>Regex for parsing Datacenter and Zone ID indexes</summary>
        public static Regex DatacenterZoneRegex => GetDatacenterZoneRegex();

        /// <summary>Regex for parsing Datacenter, Zone and Instance ID indexes</summary>
        public static Regex DatacenterZoneInstanceRegex => GetDatacenterZoneInstanceRegex();

        /// <summary>Regex for parsing Instance ID indexes</summary>
        public static Regex DatacenterInstanceRegex => GetDatacenterInstanceRegex();

        /// <summary>Regex for parsing Region ID indexes</summary>
        public static Regex RegionRegex => GetRegionRegex();

        /// <summary>Regex for parsing Region and Zone ID indexes</summary>
        public static Regex RegionZoneRegex => GetRegionZoneRegex();

        /// <summary>Regex for parsing Region, Zone and Instance ID indexes</summary>
        public static Regex RegionZoneInstanceRegex => GetRegionZoneInstanceRegex();

        /// <summary>Regex for parsing Region and Instance ID indexes</summary>
        public static Regex RegionInstanceRegex => GetRegionInstanceRegex();

        /// <summary>Regex for parsing Audience ID indexes</summary>
        public static Regex AudienceRegex => GetAudienceRegex();

        /// <summary>Regex for parsing Audience and Zone ID indexes</summary>
        public static Regex AudienceZoneRegex => GetAudienceZoneRegex();

        /// <summary>Regex for parsing Audience, Zone and Instance ID indexes</summary>
        public static Regex AudienceZoneInstanceRegex => GetAudienceZoneInstanceRegex();

        /// <summary>Regex for parsing Audience and Instance ID indexes</summary>
        public static Regex AudienceInstanceRegex => GetAudienceInstanceRegex();

        /// <summary>Regex for parsing <c>"all"</c></summary>
        public static Regex AllRegex => GetAllRegex();

        /// <summary>Check whether a key is a valid index key</summary>
        public static bool IsValidKey(string indexKey) => IndexInfo.TryParse(indexKey, out _);

        /// <summary>Get the corresponding <see cref="IndexType"/> that can cover a specific <see cref="SonarJurisdiction"/></summary>
        public static IndexType GetIndexType(this SonarJurisdiction jurisdiction)
        {
            return jurisdiction switch
            {
                SonarJurisdiction.None => IndexType.None,
                SonarJurisdiction.Instance => IndexType.WorldZoneInstance,
                SonarJurisdiction.Zone => IndexType.WorldZone,
                SonarJurisdiction.World => IndexType.World,
                SonarJurisdiction.Datacenter => IndexType.Datacenter,
                SonarJurisdiction.Region => IndexType.Region,
                SonarJurisdiction.Audience => IndexType.Audience,
                SonarJurisdiction.All => IndexType.All,
                _ => IndexType.None
            };
        }

        /// <summary>Get all index types except <see cref="IndexType.None"/> and <see cref="IndexType.All"/></summary>
        public static IEnumerable<IndexType> GetIndexTypes()
        {
            return s_indexTypes ??= Enum.GetValues<IndexType>().Where(t => t is not IndexType.None and not IndexType.All).ToArray();
        }

        /// <summary>Generate an index key of a specified type.</summary>
        public static string GetIndexKey(this GamePlace place, IndexType type)
        {
            return GetIndexKey(type, place.WorldId, place.ZoneId, place.InstanceId);
        }

        /// <summary>General function for generating index keys of a specified type</summary>
        /// <param name="arg1">Arg for first placeholder</param>
        /// <param name="arg2">Arg for second placeholder</param>
        /// <param name="arg3">Arg for third placeholder</param>
        public static string GetIndexKey(IndexType type, uint arg1, uint arg2, uint arg3)
        {
            return type switch
            {
                IndexType.None => GetNoneIndexKey(),

                IndexType.World => GetWorldIndexKey(arg1),
                IndexType.WorldZone => GetWorldZoneIndexKey(arg1, arg2),
                IndexType.WorldZoneInstance => GetWorldZoneInstanceIndexKey(arg1, arg2, arg3),
                IndexType.WorldInstance => GetWorldInstanceIndexKey(arg1, arg2),

                IndexType.Zone => GetZoneIndexKey(arg1),
                IndexType.ZoneInstance => GetZoneInstanceIndexKey(arg1, arg2),
                IndexType.Instance => GetInstanceIndexKey(arg1),

                IndexType.Datacenter => GetDatacenterIndexKey(arg1),
                IndexType.DatacenterZone => GetDatacenterZoneIndexKey(arg1, arg2),
                IndexType.DatacenterZoneInstance => GetDatacenterZoneInstanceIndexKey(arg1, arg2, arg3),
                IndexType.DatacenterInstance => GetDatacenterInstanceIndexKey(arg1, arg2),

                IndexType.Region => GetRegionIndexKey(arg1),
                IndexType.RegionZone => GetRegionZoneIndexKey(arg1, arg2),
                IndexType.RegionZoneInstance => GetRegionZoneInstanceIndexKey(arg1, arg2, arg3),
                IndexType.RegionInstance => GetRegionInstanceIndexKey(arg1, arg2),

                IndexType.Audience => GetAudienceIndexKey(arg1),
                IndexType.AudienceZone => GetAudienceZoneIndexKey(arg1, arg2),
                IndexType.AudienceZoneInstance => GetAudienceZoneInstanceIndexKey(arg1, arg2, arg3),
                IndexType.AudienceInstance => GetAudienceInstanceIndexKey(arg1, arg2),

                IndexType.All => GetAllIndexKey(),

                _ => GetNoneIndexKey()
            };
        }

#pragma warning disable S3400 // Methods should not return constants (Justification: Consistency)
        /// <summary>Generate a world index key.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetNoneIndexKey() => "none";

        /// <summary>Generate a world index key.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetAllIndexKey() => "all";
#pragma warning restore S3400 // Methods should not return constants

        /// <summary>Generate a world index key.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetWorldIndexKey(uint worldId) => StringUtils.Intern($"{worldId}");

        /// <summary>Generate a world zone index key.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetWorldZoneIndexKey(uint worldId, uint zoneId) => StringUtils.Intern($"{worldId}_{zoneId}");

        /// <summary>Generate a world zone instance index key.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetWorldZoneInstanceIndexKey(uint worldId, uint zoneId, uint instanceId) => StringUtils.Intern($"{worldId}_{zoneId}_{instanceId}");

        /// <summary>Generate a world instance index key.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetWorldInstanceIndexKey(uint worldId, uint instanceId) => StringUtils.Intern($"wi{worldId}_{instanceId}");

        /// <summary>Generate a zone index key.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetZoneIndexKey(uint zoneId) => StringUtils.Intern($"z{zoneId}");

        /// <summary>Generate a zone instance index key.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetZoneInstanceIndexKey(uint zoneId, uint instanceId) => StringUtils.Intern($"z{zoneId}_{instanceId}");

        /// <summary>Generate an instance index key.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetInstanceIndexKey(uint instanceId) => StringUtils.Intern($"i{instanceId}");

        /// <summary>Generate a datacenter index key.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetDatacenterIndexKey(uint datacenterId) => StringUtils.Intern($"d{datacenterId}");

        /// <summary>Generate a datacenter zone index key.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetDatacenterZoneIndexKey(uint datacenterId, uint zoneId) => StringUtils.Intern($"d{datacenterId}_{zoneId}");

        /// <summary>Generate a datacenter zone instance index key.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetDatacenterZoneInstanceIndexKey(uint datacenterId, uint zoneId, uint instanceId) => StringUtils.Intern($"d{datacenterId}_{zoneId}_{instanceId}");

        /// <summary>Generate a datacenter instance index key.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetDatacenterInstanceIndexKey(uint datacenterId, uint instanceId) => StringUtils.Intern($"di{datacenterId}_{instanceId}");

        /// <summary>Generate a region index key.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetRegionIndexKey(uint regionId) => StringUtils.Intern($"r{regionId}");

        /// <summary>Generate a region zone index key.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetRegionZoneIndexKey(uint regionId, uint zoneId) => StringUtils.Intern($"r{regionId}_{zoneId}");

        /// <summary>Generate a region zone instance index key.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetRegionZoneInstanceIndexKey(uint regionId, uint zoneId, uint instanceId) => StringUtils.Intern($"r{regionId}_{zoneId}_{instanceId}");

        /// <summary>Generate a region instance index key.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetRegionInstanceIndexKey(uint regionId, uint instanceId) => StringUtils.Intern($"ri{regionId}_{instanceId}");

        /// <summary>Generate an audience index key.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetAudienceIndexKey(uint audienceId) => StringUtils.Intern($"a{audienceId}");

        /// <summary>Generate an audience zone index key.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetAudienceZoneIndexKey(uint audienceId, uint zoneId) => StringUtils.Intern($"a{audienceId}_{zoneId}");

        /// <summary>Generate an audience zone instance index key.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetAudienceZoneInstanceIndexKey(uint audienceId, uint zoneId, uint instanceId) => StringUtils.Intern($"a{audienceId}_{zoneId}_{instanceId}");

        /// <summary>Generate an audience instance index key.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetAudienceInstanceIndexKey(uint audienceId, uint instanceId) => StringUtils.Intern($"ai{audienceId}_{instanceId}");
    }
}
