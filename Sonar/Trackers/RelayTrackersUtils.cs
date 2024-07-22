using DryIocAttributes;
using Sonar.Relays;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using AG;

namespace Sonar.Trackers
{
    public sealed class RelayTrackersUtils : IDisposable
    {
        private RelayTrackers Trackers { get; }
        private IRelayTracker<HuntRelay> Hunts => this.Trackers.Hunts;
        private IRelayTracker<FateRelay> Fates => this.Trackers.Fates;

        private readonly NonBlocking.NonBlockingDictionary<Type, NonBlocking.NonBlockingHashSet<uint>> _seenRelayIds = new();
        private readonly NonBlocking.NonBlockingHashSet<uint> _seenWorldIds = new();
        private readonly NonBlocking.NonBlockingHashSet<uint> _seenZonesIds = new();

        private uint _lowestInstanceId = uint.MaxValue;
        private uint _highestInstanceId = uint.MinValue;

        public RelayTrackersUtils(RelayTrackers trackers)
        {
            this.Trackers = trackers;

            this.Hunts.Data.Added += this.Data_Added;
            this.Fates.Data.Added += this.Data_Added;
        }

        public IReadOnlySet<uint> SeenWorldIds => this._seenWorldIds;
        public IReadOnlySet<uint> SeenZoneIds => this._seenZonesIds;

        public uint LowestInstanceId => this._lowestInstanceId;
        public uint HighestInstanceId => this._highestInstanceId;

        public IEnumerable<uint> GetInstances()
        {
            var instance = this._lowestInstanceId;
            while (instance <= this._highestInstanceId) yield return instance++;
        }

        public IReadOnlySet<uint> GetSeenRelayIds(Type type) => this._seenRelayIds.GetValueOrDefault(type) ?? (IReadOnlySet<uint>)ImmutableHashSet<uint>.Empty;
        public IReadOnlySet<uint> GetSeenRelayIds<T>() where T : Relay => this.GetSeenRelayIds(typeof(T));

        public void Dispose()
        {
            this.Hunts.Data.Added -= this.Data_Added;
            this.Fates.Data.Added -= this.Data_Added;
        }

        private void Data_Added<T>(IRelayTrackerData<T> tracker, RelayState<T> state) where T : Relay
        {
            var relay = state.Relay;
            this._seenRelayIds.GetOrAdd(relay.GetType(), static _ => new()).Add(relay.Id);
            this._seenWorldIds.Add(relay.WorldId);
            this._seenZonesIds.Add(relay.ZoneId);

            var instanceId = relay.InstanceId;
            InterlockedUtils.Min(ref this._lowestInstanceId, instanceId);
            InterlockedUtils.Max(ref this._highestInstanceId, instanceId);
        }
    }
}
