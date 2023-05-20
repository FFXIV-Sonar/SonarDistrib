using Sonar.Data.Extensions;
using Sonar.Enums;
using Sonar.Messages;
using Sonar.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using static Sonar.Utilities.UnixTimeHelper;
using System.Threading;
using Sonar.Data;
using Sonar.Config;
using Sonar.Data.Rows;
using Collections.Pooled;
using DryIocAttributes;
using Sonar.Relays;

namespace Sonar.Trackers
{
    /// <summary>
    /// Handles, receives and relay fate tracking information
    /// </summary>
    [SingletonReuse]
    [ExportEx]
    public sealed class FateTracker : RelayTracker<FateRelay>
    {
        internal FateTracker(SonarClient client) : base(client) { }
        public override RelayConfig Config => this.Client.Configuration.FateConfig;

        [Obsolete("Will be removed, use index instead and filter with .Where as needed")]
        public IEnumerable<KeyValuePair<string, RelayState<FateRelay>>> GetRelays(bool withinJurisdiction = true, FateStatus? status = null, ExpansionPack? expack = null, IReadOnlyCollection<FateStatus>? statuses = null, IReadOnlyCollection<ExpansionPack>? expacks = null, uint? regionId = null, uint? dcId = null, uint? worldId = null, uint? zoneId = null, uint? instanceId = null, uint? fateId = null, IReadOnlyCollection<uint>? regionIds = null, IReadOnlyCollection<uint>? dcIds = null, IReadOnlyCollection<uint>? worldIds = null, IReadOnlyCollection<uint>? zoneIds = null, IReadOnlyCollection<uint>? instanceIds = null, IReadOnlyCollection<uint>? fateIds = null, double? lastUpdatedAgo = null)
        {
            var query = this.GetRelaysQuery(withinJurisdiction, regionId, dcId, worldId, zoneId, instanceId, fateId, regionIds, dcIds, worldIds, zoneIds, instanceIds, fateIds, lastUpdatedAgo);
            if (statuses?.Count == 1 && !status.HasValue) { status = statuses.First(); statuses = null; }
            if (expacks?.Count == 1 && !expack.HasValue) { expack = expacks.First(); expacks = null; }

            if (expack.HasValue) query = query.Where(kv => kv.Value.Relay.GetExpansion() == expack.Value);
            if (status.HasValue) query = query.Where(kv => kv.Value.Relay.Status == status.Value); // FateRelay.Status does a few checks internally

            if (expacks?.Count > 0) query = query.Where(kv => expacks.Contains(kv.Value.Relay.GetExpansion()));
            if (statuses?.Count > 0) query = query.Where(kv => statuses.Contains(kv.Value.Relay.Status));

            return query;
        }

        [Obsolete("Will be removed, use index instead and filter with .Where as needed")]
        public IEnumerable<KeyValuePair<string, RelayState<FateRelay>>> GetRelays(int count = 100, bool withinJurisdiction = true, FateStatus? status = null, ExpansionPack? expack = null, IReadOnlyCollection<FateStatus>? statuses = null, IReadOnlyCollection<ExpansionPack>? expacks = null, uint? regionId = null, uint? dcId = null, uint? worldId = null, uint? zoneId = null, uint? instanceId = null, uint? fateId = null, IReadOnlyCollection<uint>? regionIds = null, IReadOnlyCollection<uint>? dcIds = null, IReadOnlyCollection<uint>? worldIds = null, IReadOnlyCollection<uint>? zoneIds = null, IReadOnlyCollection<uint>? instanceIds = null, IReadOnlyCollection<uint>? fateIds = null, double? lastUpdatedAgo = null)
        {
            // Not going to reinvent Math.Clamp
            if (count < 0) count = 0;
            if (count == 0) return Enumerable.Empty<KeyValuePair<string, RelayState<FateRelay>>>();

            var query = this.GetRelaysQuery(withinJurisdiction, regionId, dcId, worldId, zoneId, instanceId, fateId, regionIds, dcIds, worldIds, zoneIds, instanceIds, fateIds, lastUpdatedAgo);
            if (statuses?.Count == 1 && !status.HasValue) { status = statuses.First(); statuses = null; }
            if (expacks?.Count == 1 && !expack.HasValue) { expack = expacks.First(); expacks = null; }

            if (expack.HasValue) query = query.Where(kv => kv.Value.Relay.GetExpansion() == expack.Value);
            if (status.HasValue) query = query.Where(kv => kv.Value.Relay.Status == status.Value); // FateRelay.Status does a few checks internally

            if (expacks?.Count > 0) query = query.Where(kv => expacks.Contains(kv.Value.Relay.GetExpansion()));
            if (statuses?.Count > 0) query = query.Where(kv => statuses.Contains(kv.Value.Relay.Status));

            return query
                .OrderByDescending(kv => kv.Value.LastSeen)
                .Take(count);
        }
    }
}
