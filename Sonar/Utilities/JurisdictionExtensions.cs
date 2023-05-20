using Sonar.Data.Extensions;
using Sonar.Data.Rows;
using Sonar.Enums;
using Sonar.Models;
using Sonar.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Sonar.Utilities
{
    public static class JurisdictionExtensions
    {
        
        /// <summary>
        /// Determine Jurisdiction between 2 worlds
        /// </summary>
        public static SonarJurisdiction WorldsJurisdiction(uint world1, uint world2, IReadOnlyDictionary<uint, WorldRow>? worlds = null)
        {
            if (world1 == world2) return SonarJurisdiction.World; // Simple case first

            worlds ??= Database.Worlds;
            if (!worlds.TryGetValue(world1, out var world1Info) || !worlds.TryGetValue(world2, out var world2Info)) return SonarJurisdiction.All;

            if (world1Info.AudienceId != world2Info.AudienceId) return SonarJurisdiction.All;
            if (world1Info.RegionId != world2Info.RegionId) return SonarJurisdiction.Audience;
            if (world1Info.DatacenterId != world2Info.DatacenterId) return SonarJurisdiction.Region;
            return SonarJurisdiction.Datacenter;
        }

        /// <summary>
        /// Determine if two places are within the specified jurisdiction
        /// </summary>
        public static bool IsWithinJurisdiction(this GamePlace place1, GamePlace place2, SonarJurisdiction jurisdiction, IReadOnlyDictionary<uint, WorldRow>? worlds = null)
        {
            if (jurisdiction == SonarJurisdiction.All) return true;
            if (jurisdiction == SonarJurisdiction.None) return false;
            return jurisdiction >= GetJurisdictionWith(place1, place2, worlds);
        }

        /// <summary>
        /// Gets Jurisdiction between two places
        /// </summary>
        public static SonarJurisdiction GetJurisdictionWith(this GamePlace place1, GamePlace place2, IReadOnlyDictionary<uint, WorldRow>? worlds = null)
        {
            if (place1 == place2) return SonarJurisdiction.Instance;

            var jurisdiction = WorldsJurisdiction(place1.WorldId, place2.WorldId, worlds);
            if (jurisdiction == SonarJurisdiction.World)
            {
                if (place1.ZoneId != place2.ZoneId) return SonarJurisdiction.World;
                if (place1.InstanceId != place2.InstanceId) return SonarJurisdiction.Zone;
                return SonarJurisdiction.Instance;
            }
            return jurisdiction;
        }
    }
}
