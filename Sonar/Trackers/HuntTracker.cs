using Sonar.Data.Extensions;
using Sonar.Enums;
using Sonar.Messages;
using Sonar.Utilities;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using static Sonar.Utilities.UnixTimeHelper;
using System.Threading;
using Sonar.Data;
using Sonar.Data.Rows;
using Sonar.Config;
using DryIocAttributes;
using Sonar.Relays;

namespace Sonar.Trackers
{
    /// <summary>
    /// Handles, receives and relay hunt tracking information
    /// </summary>
    [SingletonReuse]
    [ExportEx]
    public sealed class HuntTracker : RelayTracker<HuntRelay>
    {
        internal HuntTracker(SonarClient client) : base(client) { }
        public override RelayConfig Config => this.Client.Configuration.HuntConfig;

        [Obsolete("Will be removed, use index instead and filter with .Where as needed")]
        public IEnumerable<KeyValuePair<string, RelayState<HuntRelay>>> GetRelays(bool withinJurisdiction = true, HuntRank? rank = null, ExpansionPack? expack = null, IReadOnlyCollection<HuntRank>? ranks = null, IReadOnlyCollection<ExpansionPack>? expacks = null, uint? regionId = null, uint? dcId = null, uint? worldId = null, uint? zoneId = null, uint? instanceId = null, uint? huntId = null, IReadOnlyCollection<uint>? regionIds = null, IReadOnlyCollection<uint>? dcIds = null, IReadOnlyCollection<uint>? worldIds = null, IReadOnlyCollection<uint>? zoneIds = null, IReadOnlyCollection<uint>? instanceIds = null, IReadOnlyCollection<uint>? huntIds = null, double? lastUpdatedAgo = null)
        {
            var query = this.GetRelaysQuery(withinJurisdiction, regionId, dcId, worldId, zoneId, instanceId, huntId, regionIds, dcIds, worldIds, zoneIds, instanceIds, huntIds, lastUpdatedAgo);
            if (ranks?.Count == 1 && !rank.HasValue) { rank = ranks.First(); ranks = null; }
            if (expacks?.Count == 1 && !expack.HasValue) { expack = expacks.First(); expacks = null; }

            if (expack.HasValue) query = query.Where(kv => kv.Value.Relay.GetExpansion() == expack.Value);
            if (rank.HasValue) query = query.Where(kv => kv.Value.Relay.GetRank() == rank.Value);

            if (expacks?.Count > 0) query = query.Where(kv => expacks.Contains(kv.Value.Relay.GetExpansion()));
            if (ranks?.Count > 0) query = query.Where(kv => ranks.Contains(kv.Value.Relay.GetRank()));

            return query;
        }

        [Obsolete("Will be removed, use index instead and filter with .Where as needed")]
        public IEnumerable<KeyValuePair<string, RelayState<HuntRelay>>> GetRelays(int count = 100, bool withinJurisdiction = true, HuntRank? rank = null, ExpansionPack? expack = null, IReadOnlyCollection<HuntRank>? ranks = null, IReadOnlyCollection<ExpansionPack>? expacks = null, uint? regionId = null, uint? dcId = null, uint? worldId = null, uint? zoneId = null, uint? instanceId = null, uint? huntId = null, IReadOnlyCollection<uint>? regionIds = null, IReadOnlyCollection<uint>? dcIds = null, IReadOnlyCollection<uint>? worldIds = null, IReadOnlyCollection<uint>? zoneIds = null, IReadOnlyCollection<uint>? instanceIds = null, IReadOnlyCollection<uint>? huntIds = null, double? lastUpdatedAgo = null)
        {
            // Not going to reinvent Math.Clamp
            if (count < 0) count = 0;
            if (count == 0) return Enumerable.Empty<KeyValuePair<string, RelayState<HuntRelay>>>();

            var query = this.GetRelaysQuery(withinJurisdiction, regionId, dcId, worldId, zoneId, instanceId, huntId, regionIds, dcIds, worldIds, zoneIds, instanceIds, huntIds, lastUpdatedAgo);
            if (ranks?.Count == 1 && !rank.HasValue) { rank = ranks.First(); ranks = null; }
            if (expacks?.Count == 1 && !expack.HasValue) { expack = expacks.First(); expacks = null; }

            if (expack.HasValue) query = query.Where(kv => kv.Value.Relay.GetExpansion() == expack.Value);
            if (rank.HasValue) query = query.Where(kv => kv.Value.Relay.GetRank() == rank.Value);

            if (expacks?.Count > 0) query = query.Where(kv => expacks.Contains(kv.Value.Relay.GetExpansion()));
            if (ranks?.Count > 0) query = query.Where(kv => ranks.Contains(kv.Value.Relay.GetRank()));

            return query
                .OrderByDescending(kv => kv.Value.LastSeen)
                .Take(count);
        }
    }
}
