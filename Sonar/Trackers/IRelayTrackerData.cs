using Sonar.Relays;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Sonar.Trackers
{
    public interface IRelayTrackerData<T> : IRelayTrackerData where T : Relay
    {
        public new IReadOnlyDictionary<string, RelayState<T>> States { get; }
        public new IReadOnlyDictionary<string, IReadOnlyCollection<RelayState<T>>> Index { get; }

        public new IReadOnlyCollection<RelayState<T>> GetIndexStates(string indexKey);

        public bool TryAddState(RelayState<T> state);
        public void TryAddStates(IEnumerable<RelayState<T>> states);

        public bool Remove(RelayState<T> state);
        public bool Remove(T relay);

        public new event Action<IRelayTrackerData<T>, RelayState<T>>? Added;
        public new event Action<IRelayTrackerData<T>, RelayState<T>>? Removed;
        public new event Action<IRelayTrackerData<T>>? Cleared;
        public new event Action<IRelayTrackerData<T>, Exception>? Exception;
    }

    public interface IRelayTrackerData
    {
        public RelayType RelayType { get; }
        public int Count { get; }
        public int IndexCount { get; }
        public bool Indexing { get; set; }

        public IReadOnlyDictionary<string, RelayState> States { get; }
        public IReadOnlyDictionary<string, IReadOnlyCollection<RelayState>> Index { get; }

        public IReadOnlyCollection<RelayState> GetIndexStates(string indexKey);

        public bool TryAddState(RelayState state);
        public void TryAddStates(IEnumerable<RelayState> states);

        public bool Remove(RelayState state);
        public bool Remove(Relay relay);
        public bool Remove(string key);
        public void Clear();

        public void RebuildIndex(bool parallel = false);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public IEnumerable<string> DebugIndexConsistencyCheck(bool parallel = false);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void DebugCleanupIndex();

        public event Action<IRelayTrackerData, RelayState>? Added;
        public event Action<IRelayTrackerData, RelayState>? Removed;
        public event Action<IRelayTrackerData>? Cleared;
        public event Action<IRelayTrackerData, Exception>? Exception;
    }
}