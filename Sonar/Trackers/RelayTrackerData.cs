using NonBlocking;
using Sonar.Data;
using Sonar.Extensions;
using Sonar.Indexes;
using Sonar.Relays;
using Sonar.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using SonarUtils;
using SonarUtils.Collections;
using DryIoc;
using Microsoft.VisualBasic;

namespace Sonar.Trackers
{
    public sealed partial class RelayTrackerData<T> where T : Relay
    {
        internal readonly ConcurrentDictionarySlim<string, RelayState<T>> _states = new(comparer: FarmHashStringComparer.Instance);
        private readonly ConcurrentDictionarySlim<string, ConcurrentHashSetSlim<string>> _index = new(comparer: FarmHashStringComparer.Instance);

        public IReadOnlyDictionary<string, RelayState<T>> States => this._states;
        public IReadOnlyDictionary<string, IReadOnlySet<string>> Index { get; }

        internal RelayTrackerData()
        {
            this.Index = TransformReadOnlyDictionary.Create(this._index, static entries => (IReadOnlySet<string>)entries);
        }

        internal RelayTrackerData(IEnumerable<RelayState<T>> states) : this()
        {
            this.TryAddStates(states);
        }

        internal bool TryAddState(RelayState<T> state)
        {
            if (this._states.TryAdd(state.RelayKey, state))
            {
                this.AddIndexEntries(state);
                this.Added?.SafeInvoke(this, state);
                return true;
            }
            return false;
        }

        internal void TryAddStates(IEnumerable<RelayState<T>> states)
        {
            foreach (var state in states) this.TryAddState(state);
        }

        public int Count => this._states.Count;

        public int IndexCount => this._index.Count;

        /// <summary>Remove a state through its key</summary>
        public bool Remove(string key)
        {
            if (!this._states.Remove(key, out var state)) return false;
            this.RemoveIndexEntries(state);
            this.Removed?.SafeInvoke(this, state);
            return true;
        }

        /// <summary>Remove a state</summary>
        public bool Remove(RelayState<T> state)
        {
            if (!this._states.Remove(KeyValuePair.Create(state.RelayKey, state))) return false;
            this.RemoveIndexEntries(state);
            this.Removed?.SafeInvoke(this, state);
            return true;
        }

        /// <summary>Remove a state associated with a specific relay</summary>
        public bool Remove(T relay)
        {
            if (!this._states.TryGetValue(relay.RelayKey, out var state)) return false;
            if (state.Relay != relay) return false;
            return this.Remove(state);
        }

        /// <summary>Get states from an index</summary>
        /// <param name="indexKey"><see cref="IndexType"/></param>
        /// <remarks>If index doesn't exist, an empty set is returned</remarks>
        public IReadOnlySet<string> GetIndexEntries(string indexKey)
        {
            if (indexKey == "all") return this.States.GetNonSnapshottingKeys();
            return this.Index.GetValueOrDefault(indexKey) ?? ImmutableHashSet<string>.Empty;
        }

        /// <summary>Get states from an index</summary>
        /// <param name="indexKey"><see cref="IndexType"/></param>
        /// <remarks>
        /// <para>If index doesn't exist, an empty collection is returned</para>
        /// <para>They're technically set except for <c>"all"</c>, please don't cast and assume its safe.</para>
        /// </remarks>
        public IReadOnlyCollection<RelayState<T>> GetIndexStates(string indexKey)
        {
            if (indexKey == "all") return this.States.GetNonSnapshottingValues(); // NOTE: This is the cause why this method cannot return an IReadOnlySet
            if (!this.Index.TryGetValue(indexKey, out var entries))
            {
                return ImmutableHashSet<RelayState<T>>.Empty;
            }
            return TransformReadOnlySet.Create(entries, entry => this.States[entry], state => state.RelayKey);
        }

        /// <summary>Clear all data</summary>
        public void Clear()
        {
            this._states.Clear();
            this._index.Clear();
            this.Cleared?.SafeInvoke(this);
        }

        public event Action<RelayTrackerData<T>, RelayState<T>>? Added;
        public event Action<RelayTrackerData<T>, RelayState<T>>? Removed;
        public event Action<RelayTrackerData<T>>? Cleared;

        [SuppressMessage("Major Code Smell", "S1121", Justification = "Used immediately")]
        private void AddIndexEntries(RelayState<T> state)
        {
            foreach (var indexKey in Unsafe.As<string[]>(state.IndexKeys))
            {
                ConcurrentHashSetSlim<string> entries;
                while (true)
                {
                    if (this._index.TryGetValue(indexKey, out entries!)) break;
                    if (this._index.TryAdd(indexKey, entries = new(comparer: FarmHashStringComparer.Instance))) break;
                }
                entries.Add(state.RelayKey);
            }
        }

        private void RemoveIndexEntries(RelayState<T> state)
        {
            foreach (var indexKey in state.IndexKeys)
            {
                if (!this._index.TryGetValue(indexKey, out var entries)) continue;
                entries.Remove(state.RelayKey);
            }
        }
    }
}
