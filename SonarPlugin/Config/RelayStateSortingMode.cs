using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sonar.Models;
using SonarPlugin.Attributes;
using Sonar.Utilities;
using Sonar.Trackers;
using Sonar.Data.Extensions;
using AG.EnumLocalization.Attributes;

namespace SonarPlugin.Config
{
    [EnumLocStrings("RelaySortingMode")]
    public enum RelayStateSortingMode
    {
        [EnumLoc(Fallback = "Default")]
        Default,

        [EnumLoc(Fallback = "Last Found")]
        LastFound,

        [EnumLoc(Fallback = "Last Updated")]
        LastUpdated,

        [EnumLoc(Fallback = "Alphabetical")]
        Alphabetical,

        [EnumLoc(Fallback = "Data Center ID")]
        Datacenter,

        [EnumLoc(Fallback = "World ID")]
        World,

        [EnumLoc(Fallback = "Zone ID")]
        Zone,

        [EnumLoc(Fallback = "World then Zone then Instance then Relay IDs")]
        WorldZoneInstanceRelay,

        [EnumLoc(Fallback = "Zone then Instance then Relay IDs")]
        ZoneInstanceRelay,

        [EnumLoc(Fallback = "Jurisdiction")]
        Jurisdiction,

        [EnumLoc(Fallback = "Jurisdiction then World then Zone then Instance then Relay IDs")]
        JurisdictionWorldZoneInstanceRelay,
    }

    public static class RelayStateSortingModeExtensions
    {
        public static IOrderedEnumerable<RelayState> SortBy(this IEnumerable<RelayState> states, RelayStateSortingMode mode, GamePlace place)
        {
            return mode switch
            {
                RelayStateSortingMode.LastFound => states.OrderByDescending(s => s.LastFound),
                RelayStateSortingMode.LastUpdated => states.OrderByDescending(s => s.LastUpdated),
                RelayStateSortingMode.Alphabetical => states.OrderBy(s => s.SortKey),
                RelayStateSortingMode.Datacenter => states.OrderBy(s => s.GetWorld()?.AudienceId ?? 0),
                RelayStateSortingMode.World => states.OrderBy(s => s.WorldId),
                RelayStateSortingMode.Zone => states.OrderBy(s => s.ZoneId),
                RelayStateSortingMode.WorldZoneInstanceRelay => states.OrderBy(s => s.WorldId).ThenBy(s => s.ZoneId).ThenBy(s => s.InstanceId).ThenBy(s => s.Relay.Id),
                RelayStateSortingMode.ZoneInstanceRelay => states.OrderBy(s => s.ZoneId).ThenBy(s => s.InstanceId).ThenBy(s => s.Relay.Id),
                RelayStateSortingMode.Jurisdiction => states.OrderBy(s => place.GetJurisdictionWith(s.Relay)),
                RelayStateSortingMode.JurisdictionWorldZoneInstanceRelay => states.OrderBy(s => place.GetJurisdictionWith(s.Relay)).ThenBy(s => s.WorldId).ThenBy(s => s.ZoneId).ThenBy(s => s.InstanceId).ThenBy(s => s.Relay.Id),
                _ => states.SortBy(RelayStateSortingMode.LastFound, place),
            };
        }
    }
}
