using Sonar.Data.Rows;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Sonar.Data
{
    public static class WorldsExtensions
    {
        #region GetWorlds
        /// <summary>Get all worlds in a specified <paramref name="datacenter"/>.</summary>
        /// <param name="datacenter"><see cref="DatacenterRow"/> to get the worlds for.</param>
        /// <returns>All worlds in the specified <paramref name="datacenter"/>.</returns>
        public static IEnumerable<WorldRow> GetWorlds(this DatacenterRow datacenter)
            => Database.Worlds.Values.Where(world => world.DatacenterId == datacenter.Id);

        /// <summary>Get all worlds in a specified <paramref name="region"/>.</summary>
        /// <param name="region"><see cref="RegionRow"/> to get the worlds for.</param>
        /// <returns>All worlds in the specified <paramref name="region"/>.</returns>
        public static IEnumerable<WorldRow> GetWorlds(this RegionRow region)
            => Database.Worlds.Values.Where(world => world.RegionId == region.Id);

        /// <summary>Get all worlds in a specified <paramref name="audience"/>.</summary>
        /// <param name="audience"><see cref="AudienceRow"/> to get the worlds for.</param>
        /// <returns>All worlds in the specified <paramref name="audience"/>.</returns>
        public static IEnumerable<WorldRow> GetWorlds(this AudienceRow audience)
            => Database.Worlds.Values.Where(world => world.AudienceId == audience.Id);
        #endregion

        #region GetDatacenters
        /// <summary>Get all datacenters in a specified <paramref name="region"/>.</summary>
        /// <param name="region"><see cref="RegionRow"/> to get the worlds for.</param>
        /// <returns>All datacenters in the specified <paramref name="region"/>.</returns>
        public static IEnumerable<DatacenterRow> GetDatacenters(this RegionRow region)
            => Database.Datacenters.Values.Where(datacenter => datacenter.RegionId == region.Id);

        /// <summary>Get all datacenters in a specified <paramref name="audience"/>.</summary>
        /// <param name="audience"><see cref="AudienceRow"/> to get the worlds for.</param>
        /// <returns>All datacenters in the specified <paramref name="audience"/>.</returns>
        public static IEnumerable<DatacenterRow> GetDatacenters(this AudienceRow audience)
            => Database.Datacenters.Values.Where(datacenter => datacenter.AudienceId == audience.Id);
        #endregion

        #region GetRegions
        /// <summary>Get all regions in a specified <paramref name="audience"/>.</summary>
        /// <param name="audience"><see cref="AudienceRow"/> to get the worlds for.</param>
        /// <returns>All regions in the specified <paramref name="audience"/>.</returns>
        public static IEnumerable<RegionRow> GetRegions(this AudienceRow audience)
            => Database.Regions.Values.Where(region => region.AudienceId == audience.Id);
        #endregion

        #region GetDatacenter
        /// <summary>Gets the <see cref="DatacenterRow"/> this <paramref name="world"/> belongs to.</summary>
        /// <param name="world">World to get the <see cref="DatacenterRow"/> for.</param>
        /// <returns><see cref="DatacenterRow"/> in which <paramref name="world"/> is at.</returns>
        public static DatacenterRow? GetDatacenter(this WorldRow world) => Database.Datacenters.GetValueOrDefault(world.DatacenterId);
        #endregion

        #region GetRegion
        /// <summary>Gets the <see cref="RegionRow"/> this <paramref name="world"/> belongs to.</summary>
        /// <param name="world">World to get the <see cref="RegionRow"/> for.</param>
        /// <returns><see cref="RegionRow"/> in which <paramref name="world"/> is at.</returns>
        public static RegionRow? GetRegion(this WorldRow world) => Database.Regions.GetValueOrDefault(world.RegionId);

        /// <summary>Gets the <see cref="RegionRow"/> this <paramref name="datacenter"/> belongs to.</summary>
        /// <param name="datacenter">World to get the <see cref="RegionRow"/> for.</param>
        /// <returns><see cref="RegionRow"/> in which <paramref name="datacenter"/> is at.</returns>
        public static RegionRow? GetRegion(this DatacenterRow datacenter) => Database.Regions.GetValueOrDefault(datacenter.RegionId);
        #endregion

        #region GetAudience
        /// <summary>Gets the <see cref="AudienceRow"/> this <paramref name="world"/> belongs to.</summary>
        /// <param name="world">World to get the <see cref="AudienceRow"/> for.</param>
        /// <returns><see cref="AudienceRow"/> in which <paramref name="world"/> is at.</returns>
        public static AudienceRow? GetAudience(this WorldRow world) => Database.Audiences.GetValueOrDefault(world.AudienceId);

        /// <summary>Gets the <see cref="AudienceRow"/> this <paramref name="datacenter"/> belongs to.</summary>
        /// <param name="datacenter">World to get the <see cref="AudienceRow"/> for.</param>
        /// <returns><see cref="AudienceRow"/> in which <paramref name="datacenter"/> is at.</returns>
        public static AudienceRow? GetAudience(this DatacenterRow datacenter) => Database.Audiences.GetValueOrDefault(datacenter.AudienceId);

        /// <summary>Gets the <see cref="AudienceRow"/> this <paramref name="region"/> belongs to.</summary>
        /// <param name="region">World to get the <see cref="AudienceRow"/> for.</param>
        /// <returns><see cref="AudienceRow"/> in which <paramref name="region"/> is at.</returns>
        public static AudienceRow? GetAudience(this RegionRow region) => Database.Audiences.GetValueOrDefault(region.AudienceId);
        #endregion
    }
}
