using Sonar.Data.Rows;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace Sonar.Data.Details
{
    public sealed class WorldTravelHelper
    {
        private FrozenDictionary<uint, FrozenDictionary<uint, WorldTravelRow>> _fromTo = default!;
        private FrozenDictionary<uint, FrozenDictionary<uint, WorldTravelRow>> _toFrom = default!;

        private SonarDb Db { get; }

        internal WorldTravelHelper(SonarDb db)
        {
            this.Db = db;
            this.Rebuild();
        }

        /// <summary>Determine if travel is possible from <paramref name="startWorldId"/> to <paramref name="endWorldId"/>.</summary>
        /// <param name="startWorldId">Starting World ID.</param>
        /// <param name="endWorldId">Ending World ID.</param>
        /// <returns>Whether travel is possible.</returns>
        public bool CanTravel(uint startWorldId, uint endWorldId)
        {
            return this._fromTo.TryGetValue(startWorldId, out var set) && set.ContainsKey(endWorldId);
        }

        /// <summary>Get all destinations from a specific <paramref name="worldId"/>.</summary>
        /// <param name="worldId">Departing world ID.</param>
        /// <returns>All available destinations, keyed by destination world IDs (end).</returns>
        public IReadOnlyDictionary<uint, WorldTravelRow> GetDepartures(uint worldId)
        {
            return this._fromTo.GetValueOrDefault(worldId, FrozenDictionary<uint, WorldTravelRow>.Empty);
        }

        /// <summary>Get all arrivals to a specific <paramref name="worldId"/>.</summary>
        /// <param name="worldId">Arriving world ID.</param>
        /// <returns>All available arrivals, keyed by departing world IDs (start).</returns>
        public IReadOnlyDictionary<uint, WorldTravelRow> GetArrivals(uint worldId)
        {
            return this._toFrom.GetValueOrDefault(worldId, FrozenDictionary<uint, WorldTravelRow>.Empty);
        }

        /// <summary>Rebuild helper.</summary>
        public void Rebuild()
        {
            var fromTo = new Dictionary<uint, Dictionary<uint, WorldTravelRow>>();
            var toFrom = new Dictionary<uint, Dictionary<uint, WorldTravelRow>>();

            foreach (var travel in this.Db.WorldTravelData.Values)
            {
                if (!fromTo.TryGetValue(travel.StartWorldId, out var endDict)) fromTo[travel.StartWorldId] = endDict = [];
                if (!toFrom.TryGetValue(travel.EndWorldId, out var startDict)) toFrom[travel.EndWorldId] = startDict = [];
                startDict.Add(travel.StartWorldId, travel);
                endDict.Add(travel.EndWorldId, travel);
            }

            this._fromTo = fromTo.ToFrozenDictionary(kvp => kvp.Key, kvp => kvp.Value.ToFrozenDictionary());
            this._toFrom = toFrom.ToFrozenDictionary(kvp => kvp.Key, kvp => kvp.Value.ToFrozenDictionary());
        }
    }
}
