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
        [EnumCheapLoc("SortingModeDefault", "Default")]
        Default,

        [EnumCheapLoc("SortingModeLastFound", "Last Found")]
        LastFound,

        [EnumCheapLoc("SortingModeLastUpdated", "Last Updated")]
        LastUpdated,

        [EnumCheapLoc("SortingModeAlphabetical", "Alphabetical")]
        Alphabetical,

        [EnumCheapLoc("SortingModeDatacenter", "Data Center ID")]
        Datacenter,

        [EnumCheapLoc("SortingModeWorld", "World ID")]
        World,

        [EnumCheapLoc("SortingModeZone", "Zone ID")]
        Zone,

        //[EnumCheapLoc("SortingModeLastUpdated", "Jurisdiction")]
        //Jurisdiction, // TODO: Figure out a way to implement this
    }

    public static class RelayStateSortingModeExtensions
    {
        public static IEnumerable<RelayState> SortBy(this IEnumerable<RelayState> states, RelayStateSortingMode mode, GamePlace place)
        {
            return mode switch
            {
                RelayStateSortingMode.LastFound => states.OrderByDescending(s => s.LastFound),
                RelayStateSortingMode.LastUpdated => states.OrderByDescending(s => s.LastUpdated),
                RelayStateSortingMode.Alphabetical => states.OrderBy(s => s.SortKey),
                RelayStateSortingMode.Datacenter => states.OrderBy(s => s.GetWorld()?.AudienceId ?? 0),
                RelayStateSortingMode.World => states.OrderBy(s => s.WorldId),
                RelayStateSortingMode.Zone => states.OrderBy(s => s.ZoneId),
                _ => states.SortBy(RelayStateSortingMode.LastFound, place),
            };
        }
    }
}
