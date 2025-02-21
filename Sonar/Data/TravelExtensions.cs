using Sonar.Data.Rows;
using System.Linq;

namespace Sonar.Data
{
    public static class TravelExtensions
    {
        /// <summary>Determine if travel is possible departure <paramref name="departure"/> to <paramref name="arrival"/>.</summary>
        /// <param name="departure">Departure <see cref="WorldRow"/>.</param>
        /// <param name="arrival">Arrival <see cref="WorldRow"/>.</param>
        /// <returns>Whether travel is possible.</returns>
        public static bool CanTravelTo(this WorldRow departure, WorldRow arrival) => Database.WorldTravel.CanTravel(departure.Id, arrival.Id);

        /// <summary>Determine if travel is possible departure <paramref name="departure"/> to <paramref name="arrival"/>.</summary>
        /// <param name="departure">Departure <see cref="WorldRow"/>.</param>
        /// <param name="arrival">Arrival <see cref="DatacenterRow"/>.</param>
        /// <returns>Whether travel is possible.</returns>
        public static bool CanTravelTo(this WorldRow departure, DatacenterRow datacenter) => datacenter.GetWorlds().Any(departure.CanTravelTo);

        /// <summary>Determine if travel is possible departure <paramref name="departure"/> to <paramref name="arrival"/>.</summary>
        /// <param name="departure">Departure <see cref="WorldRow"/>.</param>
        /// <param name="arrival">Arrival <see cref="RegionRow"/>.</param>
        /// <returns>Whether travel is possible.</returns>
        public static bool CanTravelTo(this WorldRow departure, RegionRow region) => region.GetWorlds().Any(departure.CanTravelTo);

        /// <summary>Determine if travel is possible departure <paramref name="departure"/> to <paramref name="arrival"/>.</summary>
        /// <param name="departure">Departure <see cref="WorldRow"/>.</param>
        /// <param name="arrival">Arrival <see cref="AudienceRow"/>.</param>
        /// <returns>Whether travel is possible.</returns>
        public static bool CanTravelTo(this WorldRow departure, AudienceRow audience) => audience.GetWorlds().Any(departure.CanTravelTo);

        /// <summary>Determine if travel is possible departure <paramref name="departure"/> to <paramref name="arrival"/>.</summary>
        /// <param name="departure">Departure <see cref="DatacenterRow"/>.</param>
        /// <param name="arrival">Arrival <see cref="WorldRow"/>.</param>
        /// <returns>Whether travel is possible.</returns>
        public static bool CanTravelTo(this DatacenterRow departure, WorldRow world) => departure.GetWorlds().Any(departWorld => departWorld.CanTravelTo(world));

        /// <summary>Determine if travel is possible departure <paramref name="departure"/> to <paramref name="arrival"/>.</summary>
        /// <param name="departure">Departure <see cref="DatacenterRow"/>.</param>
        /// <param name="arrival">Arrival <see cref="DatacenterRow"/>.</param>
        /// <returns>Whether travel is possible.</returns>
        public static bool CanTravelTo(this DatacenterRow departure, DatacenterRow datacenter) => departure.GetWorlds().Any(departWorld => departWorld.CanTravelTo(datacenter));

        /// <summary>Determine if travel is possible departure <paramref name="departure"/> to <paramref name="arrival"/>.</summary>
        /// <param name="departure">Departure <see cref="DatacenterRow"/>.</param>
        /// <param name="arrival">Arrival <see cref="RegionRow"/>.</param>
        /// <returns>Whether travel is possible.</returns>
        public static bool CanTravelTo(this DatacenterRow departure, RegionRow region) => departure.GetWorlds().Any(departWorld => departWorld.CanTravelTo(region));

        /// <summary>Determine if travel is possible departure <paramref name="departure"/> to <paramref name="arrival"/>.</summary>
        /// <param name="departure">Departure <see cref="DatacenterRow"/>.</param>
        /// <param name="arrival">Arrival <see cref="AudienceRow"/>.</param>
        /// <returns>Whether travel is possible.</returns>
        public static bool CanTravelTo(this DatacenterRow departure, AudienceRow audience) => departure.GetWorlds().Any(departWorld => departWorld.CanTravelTo(audience));

        /// <summary>Determine if travel is possible departure <paramref name="departure"/> to <paramref name="arrival"/>.</summary>
        /// <param name="departure">Departure <see cref="RegionRow"/>.</param>
        /// <param name="arrival">Arrival <see cref="WorldRow"/>.</param>
        /// <returns>Whether travel is possible.</returns>
        public static bool CanTravelTo(this RegionRow departure, WorldRow world) => departure.GetWorlds().Any(departWorld => departWorld.CanTravelTo(world));

        /// <summary>Determine if travel is possible departure <paramref name="departure"/> to <paramref name="arrival"/>.</summary>
        /// <param name="departure">Departure <see cref="RegionRow"/>.</param>
        /// <param name="arrival">Arrival <see cref="DatacenterRow"/>.</param>
        /// <returns>Whether travel is possible.</returns>
        public static bool CanTravelTo(this RegionRow departure, DatacenterRow datacenter) => departure.GetWorlds().Any(departWorld => departWorld.CanTravelTo(datacenter));

        /// <summary>Determine if travel is possible departure <paramref name="departure"/> to <paramref name="arrival"/>.</summary>
        /// <param name="departure">Departure <see cref="RegionRow"/>.</param>
        /// <param name="arrival">Arrival <see cref="RegionRow"/>.</param>
        /// <returns>Whether travel is possible.</returns>
        public static bool CanTravelTo(this RegionRow departure, RegionRow region) => departure.GetWorlds().Any(departWorld => departWorld.CanTravelTo(region));

        /// <summary>Determine if travel is possible departure <paramref name="departure"/> to <paramref name="arrival"/>.</summary>
        /// <param name="departure">Departure <see cref="RegionRow"/>.</param>
        /// <param name="arrival">Arrival <see cref="AudienceRow"/>.</param>
        /// <returns>Whether travel is possible.</returns>
        public static bool CanTravelTo(this RegionRow departure, AudienceRow audience) => departure.GetWorlds().Any(departWorld => departWorld.CanTravelTo(audience));

        /// <summary>Determine if travel is possible departure <paramref name="departure"/> to <paramref name="arrival"/>.</summary>
        /// <param name="departure">Departure <see cref="AudienceRow"/>.</param>
        /// <param name="arrival">Arrival <see cref="WorldRow"/>.</param>
        /// <returns>Whether travel is possible.</returns>
        public static bool CanTravelTo(this AudienceRow departure, WorldRow world) => departure.GetWorlds().Any(departWorld => departWorld.CanTravelTo(world));

        /// <summary>Determine if travel is possible departure <paramref name="departure"/> to <paramref name="arrival"/>.</summary>
        /// <param name="departure">Departure <see cref="AudienceRow"/>.</param>
        /// <param name="arrival">Arrival <see cref="DatacenterRow"/>.</param>
        /// <returns>Whether travel is possible.</returns>
        public static bool CanTravelTo(this AudienceRow departure, DatacenterRow datacenter) => departure.GetWorlds().Any(departWorld => departWorld.CanTravelTo(datacenter));

        /// <summary>Determine if travel is possible departure <paramref name="departure"/> to <paramref name="arrival"/>.</summary>
        /// <param name="departure">Departure <see cref="AudienceRow"/>.</param>
        /// <param name="arrival">Arrival <see cref="RegionRow"/>.</param>
        /// <returns>Whether travel is possible.</returns>
        public static bool CanTravelTo(this AudienceRow departure, RegionRow region) => departure.GetWorlds().Any(departWorld => departWorld.CanTravelTo(region));

        /// <summary>Determine if travel is possible departure <paramref name="departure"/> to <paramref name="arrival"/>.</summary>
        /// <param name="departure">Departure <see cref="AudienceRow"/>.</param>
        /// <param name="arrival">Arrival <see cref="AudienceRow"/>.</param>
        /// <returns>Whether travel is possible.</returns>
        public static bool CanTravelTo(this AudienceRow departure, AudienceRow audience) => departure.GetWorlds().Any(departWorld => departWorld.CanTravelTo(audience));

        /// <summary>Determine if travel is possible to <paramref name="arrival"/> departure <paramref name="departure"/>.</summary>
        /// <param name="arrival">Arrival <see cref="WorldRow"/>.</param>
        /// <param name="departure">Departure <see cref="WorldRow"/>.</param>
        /// <returns>Whether travel is possible.</returns>
        public static bool CanTravelFrom(this WorldRow arrival, WorldRow departure) => departure.CanTravelTo(arrival);

        /// <summary>Determine if travel is possible to <paramref name="arrival"/> departure <paramref name="departure"/>.</summary>
        /// <param name="arrival">Arrival <see cref="WorldRow"/>.</param>
        /// <param name="departure">Departure <see cref="DatacenterRow"/>.</param>
        /// <returns>Whether travel is possible.</returns>
        public static bool CanTravelFrom(this WorldRow arrival, DatacenterRow departure) => departure.CanTravelTo(arrival);

        /// <summary>Determine if travel is possible to <paramref name="arrival"/> departure <paramref name="departure"/>.</summary>
        /// <param name="arrival">Arrival <see cref="WorldRow"/>.</param>
        /// <param name="departure">Departure <see cref="RegionRow"/>.</param>
        /// <returns>Whether travel is possible.</returns>
        public static bool CanTravelFrom(this WorldRow arrival, RegionRow departure) => departure.CanTravelTo(arrival);

        /// <summary>Determine if travel is possible to <paramref name="arrival"/> departure <paramref name="departure"/>.</summary>
        /// <param name="arrival">Arrival <see cref="WorldRow"/>.</param>
        /// <param name="departure">Departure <see cref="AudienceRow"/>.</param>
        /// <returns>Whether travel is possible.</returns>
        public static bool CanTravelFrom(this WorldRow arrival, AudienceRow departure) => departure.CanTravelTo(arrival);

        /// <summary>Determine if travel is possible to <paramref name="arrival"/> departure <paramref name="departure"/>.</summary>
        /// <param name="arrival">Arrival <see cref="DatacenterRow"/>.</param>
        /// <param name="departure">Departure <see cref="WorldRow"/>.</param>
        /// <returns>Whether travel is possible.</returns>
        public static bool CanTravelFrom(this DatacenterRow arrival, WorldRow departure) => departure.CanTravelTo(arrival);

        /// <summary>Determine if travel is possible to <paramref name="arrival"/> departure <paramref name="departure"/>.</summary>
        /// <param name="arrival">Arrival <see cref="DatacenterRow"/>.</param>
        /// <param name="departure">Departure <see cref="DatacenterRow"/>.</param>
        /// <returns>Whether travel is possible.</returns>
        public static bool CanTravelFrom(this DatacenterRow arrival, DatacenterRow departure) => departure.CanTravelTo(arrival);

        /// <summary>Determine if travel is possible to <paramref name="arrival"/> departure <paramref name="departure"/>.</summary>
        /// <param name="arrival">Arrival <see cref="DatacenterRow"/>.</param>
        /// <param name="departure">Departure <see cref="RegionRow"/>.</param>
        /// <returns>Whether travel is possible.</returns>
        public static bool CanTravelFrom(this DatacenterRow arrival, RegionRow departure) => departure.CanTravelTo(arrival);

        /// <summary>Determine if travel is possible to <paramref name="arrival"/> departure <paramref name="departure"/>.</summary>
        /// <param name="arrival">Arrival <see cref="DatacenterRow"/>.</param>
        /// <param name="departure">Departure <see cref="AudienceRow"/>.</param>
        /// <returns>Whether travel is possible.</returns>
        public static bool CanTravelFrom(this DatacenterRow arrival, AudienceRow departure) => departure.CanTravelTo(arrival);

        /// <summary>Determine if travel is possible to <paramref name="arrival"/> departure <paramref name="departure"/>.</summary>
        /// <param name="arrival">Arrival <see cref="RegionRow"/>.</param>
        /// <param name="departure">Departure <see cref="WorldRow"/>.</param>
        /// <returns>Whether travel is possible.</returns>
        public static bool CanTravelFrom(this RegionRow arrival, WorldRow departure) => departure.CanTravelTo(arrival);

        /// <summary>Determine if travel is possible to <paramref name="arrival"/> departure <paramref name="departure"/>.</summary>
        /// <param name="arrival">Arrival <see cref="RegionRow"/>.</param>
        /// <param name="departure">Departure <see cref="DatacenterRow"/>.</param>
        /// <returns>Whether travel is possible.</returns>
        public static bool CanTravelFrom(this RegionRow arrival, DatacenterRow departure) => departure.CanTravelTo(arrival);

        /// <summary>Determine if travel is possible to <paramref name="arrival"/> departure <paramref name="departure"/>.</summary>
        /// <param name="arrival">Arrival <see cref="RegionRow"/>.</param>
        /// <param name="departure">Departure <see cref="RegionRow"/>.</param>
        /// <returns>Whether travel is possible.</returns>
        public static bool CanTravelFrom(this RegionRow arrival, RegionRow departure) => departure.CanTravelTo(arrival);

        /// <summary>Determine if travel is possible to <paramref name="arrival"/> departure <paramref name="departure"/>.</summary>
        /// <param name="arrival">Arrival <see cref="RegionRow"/>.</param>
        /// <param name="departure">Departure <see cref="AudienceRow"/>.</param>
        /// <returns>Whether travel is possible.</returns>
        public static bool CanTravelFrom(this RegionRow arrival, AudienceRow departure) => departure.CanTravelTo(arrival);

        /// <summary>Determine if travel is possible to <paramref name="arrival"/> departure <paramref name="departure"/>.</summary>
        /// <param name="arrival">Arrival <see cref="AudienceRow"/>.</param>
        /// <param name="departure">Departure <see cref="WorldRow"/>.</param>
        /// <returns>Whether travel is possible.</returns>
        public static bool CanTravelFrom(this AudienceRow arrival, WorldRow departure) => departure.CanTravelTo(arrival);

        /// <summary>Determine if travel is possible to <paramref name="arrival"/> departure <paramref name="departure"/>.</summary>
        /// <param name="arrival">Arrival <see cref="AudienceRow"/>.</param>
        /// <param name="departure">Departure <see cref="DatacenterRow"/>.</param>
        /// <returns>Whether travel is possible.</returns>
        public static bool CanTravelFrom(this AudienceRow arrival, DatacenterRow departure) => departure.CanTravelTo(arrival);

        /// <summary>Determine if travel is possible to <paramref name="arrival"/> departure <paramref name="departure"/>.</summary>
        /// <param name="arrival">Arrival <see cref="AudienceRow"/>.</param>
        /// <param name="departure">Departure <see cref="RegionRow"/>.</param>
        /// <returns>Whether travel is possible.</returns>
        public static bool CanTravelFrom(this AudienceRow arrival, RegionRow departure) => departure.CanTravelTo(arrival);

        /// <summary>Determine if travel is possible to <paramref name="arrival"/> departure <paramref name="departure"/>.</summary>
        /// <param name="arrival">Arrival <see cref="AudienceRow"/>.</param>
        /// <param name="departure">Departure <see cref="AudienceRow"/>.</param>
        /// <returns>Whether travel is possible.</returns>
        public static bool CanTravelFrom(this AudienceRow arrival, AudienceRow departure) => departure.CanTravelTo(arrival);
    }
}
