using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sonar.Models;
using SonarPlugin.Attributes;
using Sonar.Utilities;

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
        Alphabetical

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
                _ => states.SortBy(RelayStateSortingMode.LastFound, place),
            };
        }
    }
}
