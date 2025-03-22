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

namespace SonarPlugin.Config
{
    public enum RelayStateSortingMode
    {
        [EnumCheapLoc("SortingModeDefault", "기본값")]
        Default,

        [EnumCheapLoc("SortingModeLastFound", "마지막 발견")]
        LastFound,

        [EnumCheapLoc("SortingModeLastUpdated", "마지막 갱신")]
        LastUpdated,

        [EnumCheapLoc("SortingModeAlphabetical", "이름")]
        Alphabetical,

        [EnumCheapLoc("SortingModeDatacenter", "데이터 센터 ID")]
        Datacenter,

        [EnumCheapLoc("SortingModeWorld", "서버 ID")]
        World,

        [EnumCheapLoc("SortingModeZone", "지역 ID")]
        Zone,

        [EnumCheapLoc("SortingModeWorldZoneInstanceRelay", "서버, 지역, 인스턴스, 릴레이 ID")]
        WorldZoneInstanceRelay,

        [EnumCheapLoc("SortingModeZoneInstanceRelay", "지역, 인스턴스, 릴레이 ID")]
        ZoneInstanceRelay,

        [EnumCheapLoc("SortingModeJurisdiction", "관할구역")]
        Jurisdiction,

        [EnumCheapLoc("SortingModeJurisdictionWorldZoneInstanceRelay", "관할구역, 서버, 인스턴스, 릴레이 ID")]
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
